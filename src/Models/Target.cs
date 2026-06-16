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

    public override string ToString()
    {
        return string.Join('\t', new object[] {
            Id,
            ActivationTimestamp,
            ActivationOffset.X,
            ActivationOffset.Y,
        });
    }

    public static string[] Fields => [
            "Target" + nameof(Id),
            nameof(ActivationTimestamp),
            nameof(ActivationOffset) + "X",
            nameof(ActivationOffset) + "Y",
        ];

    private readonly UiSettings _settings = UiSettings.From(Properties.Settings.Default);
}