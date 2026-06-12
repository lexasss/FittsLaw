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

    public Brush BorderBrush
    {
        get => _uiSettings.BorderBrush;
        set
        {
            _uiSettings = _uiSettings with { BorderBrush = value };
            _uiSettings.Save();
            OnPropertyChanged(nameof(BorderBrush));
        }
    }

    public Brush ActiveTargetBrush
    {
        get => _uiSettings.ActiveTargetBrush;
        set
        {
            _uiSettings = _uiSettings with { ActiveTargetBrush = value };
            _uiSettings.Save();
            OnPropertyChanged(nameof(ActiveTargetBrush));
        }
    }

    public Brush ActiveTargetBorderBrush
    {
        get => _uiSettings.ActiveTargetBorderBrush;
        set
        {
            _uiSettings = _uiSettings with { ActiveTargetBorderBrush = value };
            _uiSettings.Save();
            OnPropertyChanged(nameof(ActiveTargetBorderBrush));
        }
    }

    public Brush CompletedTargetBorderBrush
    {
        get => _uiSettings.CompletedTargetBorderBrush;
        set
        {
            _uiSettings = _uiSettings with { CompletedTargetBorderBrush = value };
            _uiSettings.Save();
            OnPropertyChanged(nameof(CompletedTargetBorderBrush));
        }
    }

    public double CriticalErrorRate
    {
        get => _statisticsSettings.CriticalErrorRate;
        set
        {
            _statisticsSettings = _statisticsSettings with { CriticalErrorRate = value };
            _statisticsSettings.Save();
            OnPropertyChanged(nameof(CriticalErrorRate));
        }
    }

    public event EventHandler<Models.ExperimentSetup>? ExperimentStarted;

    #region Commands

    [RelayCommand]
    private void Setup()
    {
        var dialog = new Views.Setup();
        dialog.Owner = App.Current.MainWindow;
        if (dialog.ShowDialog() == true)
        {
            try
            {
                var setup = ((Setup)dialog.DataContext).Model;
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
    Models.StatisticsSettings _statisticsSettings = Models.StatisticsSettings.From(Properties.Settings.Default);

    #endregion
}
