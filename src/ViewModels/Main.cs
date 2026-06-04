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

    public Brush Border
    {
        get => _uiSettings.Border;
        set
        {
            _uiSettings = _uiSettings with { Border = value };
            _uiSettings.Save();
            OnPropertyChanged(nameof(Border));
        }
    }

    public Brush ActiveTarget
    {
        get => _uiSettings.ActiveTarget;
        set
        {
            _uiSettings = _uiSettings with { ActiveTarget = value };
            _uiSettings.Save();
            OnPropertyChanged(nameof(ActiveTarget));
        }
    }

    public event EventHandler<Models.ExperimentSetup>? ExperimentStarted;

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
                if (experiment != null)
                {
                    _ = experiment.Run(setup);  // runs asynchronously, never throws
                    ExperimentStarted?.Invoke(this, setup);
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
