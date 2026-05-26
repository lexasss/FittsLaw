using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Media;

namespace FittsLaw.ViewModels;

internal partial class MainWindowModel : ObservableObject
{
    [ObservableProperty]
    public partial bool IsRandomized { get; set; }

    [ObservableProperty]
    public partial bool HasAudioFeedback { get; set; } = false;

    [ObservableProperty]
    public partial Brush Background { get; set; }

    [ObservableProperty]
    public partial Brush Foreground { get; set; }

    [ObservableProperty]
    public partial Brush Target { get; set; }

    public MainWindowModel()
    {
        var props = Properties.Settings.Default;

        IsRandomized = props.IsRandomized;
        HasAudioFeedback = props.HasAudioFeedback;
        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(props.Background));
        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(props.Foreground));
        Target = new SolidColorBrush((Color)ColorConverter.ConvertFromString(props.Target));
    }

    public bool Save()
    {
        var props = Properties.Settings.Default;
        props.IsRandomized = IsRandomized;
        props.HasAudioFeedback = HasAudioFeedback;
        props.Background = (Background as SolidColorBrush)?.Color.ToString() ?? "#FFFFFFFF";
        props.Foreground = (Foreground as SolidColorBrush)?.Color.ToString() ?? "#FFFFFFFF";
        props.Target = (Target as SolidColorBrush)?.Color.ToString() ?? "#FFFFFFFF";
        try
        {
            props.Save();
            return true;
        }
        catch
        {
            return false;
        }
    }

    #region Commands

    [RelayCommand]
    private void Setup()
    {
        var dialog = new Views.Setup();
        if (dialog.ShowDialog() == true)
        {/*
            var model = (SetupModel)dialog.DataContext;
            var experiment = new Experiment(
                model.SelectedTrialCount,
                model.Amplitudes,
                model.Widths);
            experiment.Run();
            */
        }
    }

    #endregion
}
