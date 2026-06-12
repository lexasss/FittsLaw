using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FittsLaw.Models;
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

    public WpfPlot TpAndMt { get; } = new WpfPlot();
    public WpfPlot EffTpAndMt { get; } = new WpfPlot();

    public event EventHandler? HideCopyToClipboardConfirmation;

    public Statistics()
    {
        TpAndMt.Plot.Legend(location: Alignment.UpperLeft);
        TpAndMt.Plot.BottomAxis.Label(IDField);
        TpAndMt.Plot.LeftAxis.Label(TPField);
        TpAndMt.Plot.RightAxis.Label(MTField);
        TpAndMt.Plot.RightAxis.Ticks(true);

        EffTpAndMt.Plot.Legend(location: Alignment.UpperLeft);
        EffTpAndMt.Plot.BottomAxis.Label(IDEffField);
        EffTpAndMt.Plot.LeftAxis.Label(TPEffField);
        EffTpAndMt.Plot.RightAxis.Label(MTField);
        EffTpAndMt.Plot.RightAxis.Ticks(true);
    }

    #region Property Setters

    partial void OnItemsChanged(IReadOnlyDictionary<string, string[]> value)
    {
        TpAndMt.Plot.Clear();
        EffTpAndMt.Plot.Clear();

        if (value.First().Value.Length == 0)
            return;

        if (!value.ContainsKey(IDField) || 
            !value.ContainsKey(TPField) || 
            !value.ContainsKey(MTField) ||
            !value.ContainsKey(TPEffField) || 
            !value.ContainsKey(IDEffField))
            return;

        var dataX = value.FirstOrDefault(kv => kv.Key == IDField)
            .Value
            .Select(double.Parse)
            .ToArray();
        var dataY = value.FirstOrDefault(kv => kv.Key == TPField)
            .Value
            .Select(double.Parse)
            .ToArray();
        TpAndMt.Plot
            .AddScatter(dataX, dataY, Color.Blue, lineStyle: LineStyle.None, label: "TP", markerShape: MarkerShape.cross, markerSize: 10);

        dataY = value.FirstOrDefault(kv => kv.Key == MTField)
            .Value
            .Select(double.Parse)
            .ToArray();
        TpAndMt.Plot
            .AddScatter(dataX, dataY, Color.Red, lineStyle: LineStyle.None, label: "MT")
            .YAxisIndex = 1;

        TpAndMt.Refresh();

        dataX = value.FirstOrDefault(kv => kv.Key == IDEffField)
            .Value
            .Select(double.Parse)
            .ToArray();
        dataY = value.FirstOrDefault(kv => kv.Key == TPEffField)
            .Value
            .Select(double.Parse)
            .ToArray();
        EffTpAndMt.Plot
            .AddScatter(dataX, dataY, Color.Blue, lineStyle: LineStyle.None, label: "TPe", markerShape: MarkerShape.cross, markerSize: 10);

        dataY = value.FirstOrDefault(kv => kv.Key == MTField)
            .Value
            .Select(double.Parse)
            .ToArray();
        EffTpAndMt.Plot
            .AddScatter(dataX, dataY, Color.Red, lineStyle: LineStyle.None, label: "MT")
            .YAxisIndex = 1;

        EffTpAndMt.Refresh();
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

    static string IDField => Services.Statistics.Fields[5];
    static string IDEffField => Services.Statistics.Fields[8];
    static string MTField => Services.Statistics.Fields[9];
    static string TPField => Services.Statistics.Fields[12];
    static string TPEffField => Services.Statistics.Fields[13];

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
