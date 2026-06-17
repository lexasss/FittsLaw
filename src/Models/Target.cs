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

    public object[] LogValues => [
        Id,
        Position.X,
        Position.Y,
        ActivationTimestamp,
        ActivationOffset.X.ToString("F0"),
        ActivationOffset.Y.ToString("F0"),
    ];

    public static string[] LogFields => [
        "Target" + nameof(Id),
        nameof(Position) + "X",
        nameof(Position) + "Y",
        nameof(ActivationTimestamp),
        nameof(ActivationOffset) + "X",
        nameof(ActivationOffset) + "Y",
    ];

    #region Internal

    private readonly UiSettings _settings = UiSettings.From(Properties.Settings.Default);

    static Target()
    {
        if (new Target().LogValues.Length != LogFields.Length)
            throw new ApplicationException("Invalid log output");
    }

    #endregion
}