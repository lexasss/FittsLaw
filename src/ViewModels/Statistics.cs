using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScottPlot;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace FittsLaw.ViewModels;

internal partial class Statistics : ObservableObject
{
    [ObservableProperty]
    public partial Visibility CopyToClipboardConfirmationVisibility { get; set; } = Visibility.Collapsed;

    [ObservableProperty]
    public partial IReadOnlyDictionary<string, string[]> Items { get; set; } = new Dictionary<string, string[]>();

    public WpfPlot MtAndTp { get; } = new WpfPlot();

    public event EventHandler? HideCopyToClipboardConfirmation;

    public Statistics()
    {
        MtAndTp.Plot.Legend(location: Alignment.UpperLeft);
        MtAndTp.Plot.BottomAxis.Label(Services.Statistics.Fields[5]);
        MtAndTp.Plot.LeftAxis.Label(Services.Statistics.Fields[12]);
        MtAndTp.Plot.RightAxis.Label(Services.Statistics.Fields[9]);
        MtAndTp.Plot.RightAxis.Ticks(true);
    }

    #region Property Setters

    partial void OnItemsChanged(IReadOnlyDictionary<string, string[]> value)
    {
        MtAndTp.Plot.Clear();

        if (!value.ContainsKey(Services.Statistics.Fields[5]) || 
            !value.ContainsKey(Services.Statistics.Fields[12]) || 
            !value.ContainsKey(Services.Statistics.Fields[9]))
            return;

        var dataX = value.FirstOrDefault(kv => kv.Key == Services.Statistics.Fields[5])
            .Value
            .Select(double.Parse)
            .ToArray();
        var dataY = value.FirstOrDefault(kv => kv.Key == Services.Statistics.Fields[12])
            .Value
            .Select(double.Parse)
            .ToArray();
        MtAndTp.Plot
            .AddScatter(dataX, dataY, Color.Blue, lineStyle: LineStyle.None, label: "TP", markerShape: MarkerShape.cross, markerSize: 10);

        dataY = value.FirstOrDefault(kv => kv.Key == Services.Statistics.Fields[9])
            .Value
            .Select(double.Parse)
            .ToArray();
        MtAndTp.Plot
            .AddScatter(dataX, dataY, Color.Red, lineStyle: LineStyle.None, label: "MT")
            .YAxisIndex = 1;

        MtAndTp.Refresh();
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void CopyTableToClipboard()
    {
        List<string> lines = [];

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            foreach (var item in Items)
                lines.Add($"{item.Key}\t{string.Join("\t", item.Value)}");
        }
        else
        {
            foreach (var item in Items)
                lines.Add($"{string.Join("\t", item.Value)}");
        }

        Clipboard.SetText(string.Join('\n', lines));

        ShowConfirmation();
    }


    [RelayCommand]
    private void CopyPlotToClipboard(WpfPlot plot)
    {
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            Clipboard.SetText(plot.Plot.GetImageHtml());
        }
        else
        {
            Clipboard.SetImage(ToBitmapSource(plot.Plot.GetBitmap()));
        }

        ShowConfirmation();
    }

    #endregion

    #region Internal

    const int CONFIRMATION_VISIBILITY_DURATION = 2000;

    private void ShowConfirmation()
    {
        CopyToClipboardConfirmationVisibility = Visibility.Visible;
        Task.Run(async () =>
        {
            await Task.Delay(CONFIRMATION_VISIBILITY_DURATION);
            HideCopyToClipboardConfirmation?.Invoke(this, EventArgs.Empty);
        });
    }

    private static BitmapSource ToBitmapSource(Bitmap bitmap)
    {
        IntPtr hBitmap = bitmap.GetHbitmap();

        try
        {
            return Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
        finally
        {
            DeleteObject(hBitmap);
        }
    }

    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    static extern bool DeleteObject(IntPtr hObject);

    #endregion
}
