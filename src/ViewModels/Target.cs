using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;

namespace FittsLaw.ViewModels;

internal partial class Target : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Background))]
    public partial bool IsActive { get; set; } = false;

    public Brush? Background => IsActive ? _settings.ActiveTarget : null;

    public Brush BorderBrush => _settings.Border;

    public Models.Target Data { get; } = new Models.Target();

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
