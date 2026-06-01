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
    public partial IReadOnlyDictionary<string, string[]> Items { get; set; } = new Dictionary<string, string[]>();

    public event EventHandler? HideCopyToClipboardConfirmation;

    #region Commands

    [RelayCommand]
    private void CopyToClipboard()
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

        CopyToClipboardConfirmationVisibility = Visibility.Visible;
        Task.Run(async () =>
        {
            await Task.Delay(2000);
            HideCopyToClipboardConfirmation?.Invoke(this, EventArgs.Empty);
        });
    }

    #endregion
}
