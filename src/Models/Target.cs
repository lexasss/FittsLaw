using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace FittsLaw.Models;

internal partial class Target : ObservableObject
{
    public int Id { get; set; }
    public Point Position { get; set; }
    public double Size { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Background))]
    [NotifyPropertyChangedFor(nameof(BorderBrush))]
    public partial bool IsActive { get; set; } = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BorderBrush))]
    public partial long ActivationTimestamp { get; set; }

    public Point ActivationOffset { get; set; }

    public Brush? Background => IsActive ? _settings.ActiveTargetBrush : null;
    public Brush BorderBrush =>
        IsActive
            ? _settings.ActiveTargetBorderBrush
            : ActivationTimestamp == 0
                ? _settings.BorderBrush
                : _settings.CompletedTargetBorderBrush;

    private readonly UiSettings _settings = UiSettings.From(Properties.Settings.Default);
}