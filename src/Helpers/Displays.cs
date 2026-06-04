using System.Management;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;

namespace FittsLaw.Helpers;

internal static class Displays
{
    public static int Count => Screen.AllScreens.Length;
    public static Models.MonitorInfo[] Items => Screen.AllScreens
        .Select(s => GetMonitorFromDeviceName(s.DeviceName))
        .OfType<Models.MonitorInfo>()
        .ToArray();

    public static bool MoveToScreen(Window window, int screenIndex)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (screenIndex == GetScreenIndex(window))
            return true;

        var screens = Screen.AllScreens;

        if (screens.Length == 0)
            return false;

        // Clamp index
        if (screenIndex < 0 || screenIndex >= screens.Length)
            screenIndex = 0;

        var target = screens[screenIndex];

        // Get DPI scaling for WPF → device pixel conversion
        var source = PresentationSource.FromVisual(window);
        double dpiX = 1.0, dpiY = 1.0;

        if (source?.CompositionTarget != null)
        {
            dpiX = source.CompositionTarget.TransformFromDevice.M11;
            dpiY = source.CompositionTarget.TransformFromDevice.M22;
        }

        // Convert screen working area to WPF units
        double x = target.WorkingArea.Left * dpiX;
        double y = target.WorkingArea.Top * dpiY;

        // Optionally center the window on that screen
        double centeredX = x + (target.WorkingArea.Width * dpiX - window.Width) / 2;
        double centeredY = y + (target.WorkingArea.Height * dpiY - window.Height) / 2;

        window.Left = centeredX;
        window.Top = centeredY;

        return true;
    }

    public static int GetScreenIndex(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        // Get DPI transform (WPF → device pixels)
        var source = PresentationSource.FromVisual(window);
        double dpiX = 1.0, dpiY = 1.0;

        if (source?.CompositionTarget != null)
        {
            dpiX = 1 / source.CompositionTarget.TransformToDevice.M11;
            dpiY = 1 / source.CompositionTarget.TransformToDevice.M22;
        }

        // Convert window center to device pixels
        double centerX = (window.Left + window.Width / 2) * dpiX;
        double centerY = (window.Top + window.Height / 2) * dpiY;

        var point = new System.Drawing.Point((int)centerX, (int)centerY);

        // Find the screen containing this point
        var screens = Screen.AllScreens;
        for (int i = 0; i < screens.Length; i++)
        {
            if (screens[i].Bounds.Contains(point))
                return i;
        }

        // Fallback: return primary screen index
        return screens.ToList().FindIndex(s => s.Primary);
    }

    #region Internal

    private static Models.MonitorInfo[] GetMonitorInfo()
    {
        static string? Decode(ushort[] chars)
        {
            return chars == null ? null : new string(chars
                    .TakeWhile(c => c != 0)
                    .Select(c => (char)c)
                    .ToArray());
        }

        List<Models.MonitorInfo> monitors = [];

        var searcher = new ManagementObjectSearcher(
            @"root\wmi",
            "SELECT * FROM WmiMonitorID");
        foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
        {
            string manufacturer = Decode((ushort[])obj["ManufacturerName"]) ?? "Unknown";
            string model = Decode((ushort[])obj["UserFriendlyName"]) ?? "Integrated Monitor";
            string serial = Decode((ushort[])obj["SerialNumberID"]) ?? string.Empty;
            monitors.Add(new Models.MonitorInfo
            {
                SerialNumberID = serial,
                Name = model,
                Manufacturer = manufacturer,
                FrendlyName = model,
            });
        }

        searcher = new ManagementObjectSearcher(
            @"root\cimv2",
            @"SELECT * FROM Win32_PnPEntity WHERE PNPClass='Monitor'");

        foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
        {
            string deviceId = obj["DeviceID"]?.ToString() ?? string.Empty;
            foreach (var monitor in monitors)
            {
                if (deviceId.StartsWith($"DISPLAY\\{monitor.Manufacturer}"))
                {
                    monitor.DeviceID = deviceId;
                    monitor.FullFrendlyName = obj["Name"]?.ToString() ?? string.Empty;
                    monitor.Description = obj["Description"]?.ToString() ?? string.Empty;
                    break;
                }
            }
        }

        return monitors.ToArray();
    }

    private static Models.MonitorInfo? GetMonitorFromDeviceName(string screenDeviceName)
    {
        var monitors = GetMonitorInfo();

        int err = GetDisplayConfigBufferSizes(
            QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS,
            out uint pathCount,
            out uint modeCount);

        if (err != ERROR_SUCCESS)
            throw new InvalidOperationException($"GetDisplayConfigBufferSizes failed: {err}");

        var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
        var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];

        err = QueryDisplayConfig(
            QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS,
            ref pathCount,
            paths,
            ref modeCount,
            modes,
            IntPtr.Zero);

        if (err != ERROR_SUCCESS)
            throw new InvalidOperationException($"QueryDisplayConfig failed: {err}");

        foreach (var path in paths)
        {
            var sourceName = new DISPLAYCONFIG_SOURCE_DEVICE_NAME
            {
                header = new DISPLAYCONFIG_DEVICE_INFO_HEADER
                {
                    type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME,
                    size = Marshal.SizeOf<DISPLAYCONFIG_SOURCE_DEVICE_NAME>(),
                    adapterId = path.sourceInfo.adapterId,
                    id = path.sourceInfo.id
                }
            };

            err = DisplayConfigGetDeviceInfo(ref sourceName);

            if (err != ERROR_SUCCESS)
                continue;

            if (!string.Equals(
                    sourceName.viewGdiDeviceName,
                    screenDeviceName,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var targetName = new DISPLAYCONFIG_TARGET_DEVICE_NAME
            {
                header = new DISPLAYCONFIG_DEVICE_INFO_HEADER
                {
                    type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME,
                    size = Marshal.SizeOf<DISPLAYCONFIG_TARGET_DEVICE_NAME>(),
                    adapterId = path.targetInfo.adapterId,
                    id = path.targetInfo.id
                }
            };

            err = DisplayConfigGetDeviceInfo(ref targetName);

            if (err == ERROR_SUCCESS)
            {
                string devicePath = targetName.monitorDevicePath[4..].Replace('#', '\\');  // remove \\.\ from the path start
                return monitors.FirstOrDefault(m => devicePath.StartsWith(m.DeviceID, StringComparison.OrdinalIgnoreCase));
                /* return !string.IsNullOrEmpty(targetName.monitorFriendlyDeviceName)
                    ? targetName.monitorFriendlyDeviceName
                    : "Integrated Monitor";*/
            }
        }

        return null;
    }

    private const int ERROR_SUCCESS = 0;

    [DllImport("user32.dll")]
    private static extern int GetDisplayConfigBufferSizes(
        QUERY_DEVICE_CONFIG_FLAGS flags,
        out uint numPathArrayElements,
        out uint numModeInfoArrayElements);

    [DllImport("user32.dll")]
    private static extern int QueryDisplayConfig(
        QUERY_DEVICE_CONFIG_FLAGS flags,
        ref uint numPathArrayElements,
        [Out] DISPLAYCONFIG_PATH_INFO[] pathArray,
        ref uint numModeInfoArrayElements,
        [Out] DISPLAYCONFIG_MODE_INFO[] modeInfoArray,
        IntPtr currentTopologyId);

    [DllImport("user32.dll")]
    private static extern int DisplayConfigGetDeviceInfo(
        ref DISPLAYCONFIG_SOURCE_DEVICE_NAME requestPacket);

    [DllImport("user32.dll")]
    private static extern int DisplayConfigGetDeviceInfo(
        ref DISPLAYCONFIG_TARGET_DEVICE_NAME requestPacket);

    private enum QUERY_DEVICE_CONFIG_FLAGS : uint
    {
        QDC_ONLY_ACTIVE_PATHS = 0x00000002
    }

    private enum DISPLAYCONFIG_DEVICE_INFO_TYPE : uint
    {
        DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME = 1,
        DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LUID
    {
        public uint LowPart;
        public int HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DISPLAYCONFIG_PATH_SOURCE_INFO
    {
        public LUID adapterId;
        public uint id;
        public uint modeInfoIdx;
        public uint statusFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DISPLAYCONFIG_PATH_TARGET_INFO
    {
        public LUID adapterId;
        public uint id;
        public uint modeInfoIdx;
        public uint outputTechnology;
        public uint rotation;
        public uint scaling;
        public uint refreshRateNumerator;
        public uint refreshRateDenominator;
        public uint scanLineOrdering;
        [MarshalAs(UnmanagedType.Bool)]
        public bool targetAvailable;
        public uint statusFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DISPLAYCONFIG_PATH_INFO
    {
        public DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo;
        public DISPLAYCONFIG_PATH_TARGET_INFO targetInfo;
        public uint flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DISPLAYCONFIG_MODE_INFO
    {
        public uint infoType;
        public uint id;
        public LUID adapterId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] modeInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DISPLAYCONFIG_DEVICE_INFO_HEADER
    {
        public DISPLAYCONFIG_DEVICE_INFO_TYPE type;
        public int size;
        public LUID adapterId;
        public uint id;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DISPLAYCONFIG_SOURCE_DEVICE_NAME
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string viewGdiDeviceName;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DISPLAYCONFIG_TARGET_DEVICE_NAME
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;

        public uint flags;
        public uint outputTechnology;
        public ushort edidManufactureId;
        public ushort edidProductCodeId;
        public uint connectorInstance;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string monitorFriendlyDeviceName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string monitorDevicePath;
    }

    #endregion
}
