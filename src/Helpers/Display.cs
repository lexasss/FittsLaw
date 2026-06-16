using System.Management;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace FittsLaw.Helpers;

internal static class Display
{
    public static int Count => Screen.AllScreens.Length;

    public static Models.MonitorInfo[] Monitors => Screen.AllScreens
        .Select(s => GetMonitorFromDeviceName(s.DeviceName))
        .OfType<Models.MonitorInfo>()
        .ToArray();

    public static bool MoveToScreen(Window window, int screenIndex)
    {
        if (screenIndex == GetScreenIndex(window))
            return true;

        var screens = Screen.AllScreens;

        if (screens.Length == 0)
            return false;

        // Clamp index
        if (screenIndex < 0 || screenIndex >= screens.Length)
            screenIndex = 0;

        var target = screens[screenIndex];

        // Convert screen working area to WPF units
        var dpi = VisualTreeHelper.GetDpi(window);

        double x = target.WorkingArea.Left / dpi.DpiScaleX;
        double y = target.WorkingArea.Top / dpi.DpiScaleX;

        // Optionally center the window on that screen
        double centeredX = x + (target.WorkingArea.Width / dpi.DpiScaleX - window.Width) / 2;
        double centeredY = y + (target.WorkingArea.Height / dpi.DpiScaleY - window.Height) / 2;

        window.Left = centeredX;
        window.Top = centeredY;

        return true;
    }

    public static int GetScreenIndex(Window window)
    {
        // Convert window center to device pixels
        var dpi = VisualTreeHelper.GetDpi(window);

        double centerX = (window.Left + window.Width / 2) * dpi.DpiScaleX;
        double centerY = (window.Top + window.Height / 2) * dpi.DpiScaleY;

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

    public static Size GetScreenSize(int screenIndex)
    {
        var dpi = VisualTreeHelper.GetDpi(App.Current.MainWindow);
        var rect = Screen.AllScreens[screenIndex].Bounds;
        return new Size(rect.Width / dpi.DpiScaleX, rect.Height / dpi.DpiScaleY);
    }

    #region Internal

    private static Models.MonitorInfo[] GetMonitors()
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
            string model = Decode((ushort[])obj["UserFriendlyName"]) ?? "Integrated Monitor";
            string deviceId = (string)obj["InstanceName"] ?? string.Empty;
            monitors.Add(new Models.MonitorInfo
            {
                SerialNumberID = Decode((ushort[])obj["SerialNumberID"]) ?? string.Empty,
                Name = model,
                Manufacturer = Decode((ushort[])obj["ManufacturerName"]) ?? "Unknown",
                FrendlyName = model,
                DeviceID = deviceId.Split('_')[0],  // removes weird _0 at the end
            });
        }

        return monitors.ToArray();
    }

    private static Models.MonitorInfo? GetMonitorFromDeviceName(string screenDeviceName)
    {
        var monitors = GetMonitors();

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
            }
        }

        return null;
    }

    #endregion

    #region WinAPI

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
