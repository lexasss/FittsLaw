using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Input;

namespace FittsLaw.ViewModels;

internal partial class Statistics : ObservableObject
{
    [ObservableProperty]
    public partial Visibility CopyToClipboardConfirmationVisibility { get; set; } = Visibility.Collapsed;

    [ObservableProperty]
    public partial Models.StatisticsData[] Items { get; set; } = [];

    public event EventHandler? HideCopyToClipboardConfirmation;

    #region Commands

    [RelayCommand]
    private void CopyToClipboard()
    {
        List<string> lines = [];

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            foreach (var item in Items)
                lines.Add($"{item.Name}\t{item.Value}");
        }
        else
        {
            foreach (var item in Items)
                lines.Add($"{item.Value}");
        }

        Clipboard.SetText(string.Join('\n', lines));

        CopyToClipboardConfirmationVisibility = Visibility.Visible;
        Task.Run(async () =>
        {
            await Task.Delay(2000);
            HideCopyToClipboardConfirmation?.Invoke(this, EventArgs.Empty);
        });
    }

    #endregion
}
