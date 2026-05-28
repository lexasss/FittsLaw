using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FittsLaw.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        _input = FittsLaw.App.ServiceProvider.GetService<Services.MouseInput>()!;
        _experiment = FittsLaw.App.ServiceProvider.GetService<Services.Experiment>()!;

        Content = _mainView;

        var mainVm = (_mainView.DataContext as ViewModels.Main)!;
        mainVm.ExperimentStarted += (s, e) =>
        {
            //Topmost = true;
            WindowStyle = WindowStyle.None;

            _originalWindowState = WindowState;
            WindowState = WindowState.Maximized;

            _experimentView = new Experiment();
            var expVm = (_experimentView.DataContext as ViewModels.Experiment)!;
            expVm.ExperimentStopped += ExpVm_ExperimentStopped;

            _input.Register(this, _experimentView.TargetContainer.ItemContainerGenerator.Items, _experiment);

            Content = _experimentView;
        };
    }

    private void ExpVm_ExperimentStopped(object? sender, EventArgs e)
    {
        //Topmost = false;
        WindowStyle = WindowStyle.SingleBorderWindow;
        WindowState = _originalWindowState;

        Content = _mainView;

        if (_experimentView != null)
        {
            var expVm = (_experimentView.DataContext as ViewModels.Experiment)!;
            expVm.ExperimentStopped -= ExpVm_ExperimentStopped;

            _experimentView?.Dispose();
            _experimentView = null;
        }
    }

    #region Internal

    readonly Services.MouseInput _input;
    readonly Services.Experiment _experiment;

    readonly Main _mainView = new();

    Experiment? _experimentView;
    WindowState _originalWindowState;

    #endregion
}