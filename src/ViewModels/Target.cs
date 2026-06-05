using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;

namespace FittsLaw.ViewModels;

internal partial class Target : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Background))]
    [NotifyPropertyChangedFor(nameof(BorderBrush))]
    public partial bool IsActive { get; set; } = false;

    public Brush? Background => IsActive ? _settings.ActiveTargetBrush : null;

    public Brush BorderBrush =>
        IsActive
            ? _settings.ActiveTargetBorderBrush
            : Data.ActivationTimestamp == 0
                ? _settings.BorderBrush
                : _settings.CompletedTargetBorderBrush;

    public Models.Target Data { get; init; } = new Models.Target();

    public Target()
    {
        _settings = Models.UiSettings.From(Properties.Settings.Default);
    }

    #region Internal

    readonly Models.UiSettings _settings;

    partial void OnIsActiveChanged(bool value)
    {
        Data.IsActive = value;
    }

    #endregion
}
