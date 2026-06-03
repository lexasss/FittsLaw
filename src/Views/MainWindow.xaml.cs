using FittsLaw.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FittsLaw.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        _input = FittsLaw.App.ServiceProvider.GetService<Services.MouseInput>() 
            ?? throw new InvalidOperationException("MouseInput service not available");
        _experiment = FittsLaw.App.ServiceProvider.GetService<Services.Experiment>() 
            ?? throw new InvalidOperationException("Experiment service not available");

        Content = _mainView;

        ((ViewModels.Main)_mainView.DataContext).ExperimentStarted += MainVm_ExperimentStarted;
    }

    #region Internal

    readonly Services.MouseInput _input;
    readonly Services.Experiment _experiment;

    readonly Main _mainView = new();

    ViewModels.Experiment? _experimentViewModel;
    WindowState _originalWindowState;
    int _originalScreenIndex = 0;

    private void MainVm_ExperimentStarted(object? sender, Models.ExperimentSetup setup)
    {
        _originalScreenIndex = Helpers.Displays.GetScreenIndex(this);
        Helpers.Displays.MoveToScreen(this, setup.ScreenIndex);

        //Topmost = true;
        WindowStyle = WindowStyle.None;

        _originalWindowState = WindowState;
        WindowState = WindowState.Maximized;

        var experimentView = new Experiment();

        _experimentViewModel = (ViewModels.Experiment)experimentView.DataContext;
        _experimentViewModel.ExperimentStopped += ExpVm_ExperimentStopped;

        _input.Register(this, experimentView.TargetContainer.ItemContainerGenerator.Items, _experiment);

        Content = experimentView;
    }

    private void ExpVm_ExperimentStopped(object? sender, Models.ExperimentSetup setup)
    {
        //Topmost = false;
        WindowStyle = WindowStyle.SingleBorderWindow;

        if (setup.ScreenIndex != _originalScreenIndex)
        {
            WindowState = WindowState.Normal;
            Helpers.Displays.MoveToScreen(this, _originalScreenIndex);
        }

        WindowState = _originalWindowState;

        Content = _mainView;

        if (_experimentViewModel != null)
        {
            _experimentViewModel.ExperimentStopped -= ExpVm_ExperimentStopped;
            _experimentViewModel.Dispose();

            _experimentViewModel = null;
        }
    }

    #endregion
}