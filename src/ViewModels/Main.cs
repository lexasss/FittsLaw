using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Media;

namespace FittsLaw.ViewModels;

internal partial class Main : ObservableObject
{
    [ObservableProperty]
    public partial bool IsExperimentRunning { get; set; } = false;

    public Brush Background
    {
        get => _uiSettings.Background;
        set
        {
            _uiSettings = _uiSettings with { Background = value };
            _uiSettings.Save();
            OnPropertyChanged(nameof(Background));
        }
    }

    public Brush Foreground
    {
        get => _uiSettings.Foreground;
        set
        {
            _uiSettings = _uiSettings with { Foreground = value };
            _uiSettings.Save();
            OnPropertyChanged(nameof(Foreground));
        }
    }

    public Brush Target
    {
        get => _uiSettings.Target;
        set
        {
            _uiSettings = _uiSettings with { Target = value };
            _uiSettings.Save();
            OnPropertyChanged(nameof(Target));
        }
    }

    public event EventHandler? ExperimentStarted;

    #region Commands

    [RelayCommand]
    private void Setup()
    {
        var dialog = new Views.Setup();
        if (dialog.ShowDialog() == true)
        {
            var model = (Setup)dialog.DataContext;
            try
            {
                var setup = Models.ExperimentSetup.From(model);
                var experiment = App.ServiceProvider.GetService<Services.Experiment>();
                if (experiment?.Run(setup) != null)
                {
                    ExperimentStarted?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Error starting experiment: {ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
    }

    #endregion

    #region Internal

    Models.UiSettings _uiSettings = Models.UiSettings.From(Properties.Settings.Default);

    #endregion
}
