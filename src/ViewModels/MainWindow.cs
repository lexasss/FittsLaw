using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace FittsLaw.ViewModels;

internal partial class MainWindow : ObservableObject
{
    [ObservableProperty]
    public partial UIElement Page { get; private set; }
    [ObservableProperty]
    public partial WindowState State { get; private set; } = WindowState.Normal;
    [ObservableProperty]
    public partial WindowStyle Style { get; private set; } = WindowStyle.SingleBorderWindow;

    public MainWindow(
        Services.Experiment experiment,
        Func<string, Services.IInput> inputFactory,
        Views.Main mainView,
        Func<Views.Experiment> experimentViewFactory)
    {
        _experiment = experiment;
        _inputFactory = inputFactory;
        _mainView = mainView;
        _experimentViewFactory = experimentViewFactory;

        Page = _mainView;

        ((Main)_mainView.DataContext).ExperimentStarted += MainVm_ExperimentStarted;
    }


    #region Internal

    readonly Func<string, Services.IInput> _inputFactory;
    readonly Services.Experiment _experiment;

    readonly Views.Main _mainView;
    readonly Func<Views.Experiment> _experimentViewFactory;

    Experiment? _experimentViewModel;
    WindowState _originalWindowState;
    int _originalScreenIndex = 0;

    private void MainVm_ExperimentStarted(object? sender, Models.ExperimentSetup setup)
    {
        var window = Application.Current.MainWindow;

        _originalWindowState = State;
        _originalScreenIndex = Services.Display.GetScreenIndex(window);

        if (setup.ScreenIndex != _originalScreenIndex && State == WindowState.Maximized)
        {
            State = WindowState.Normal;
        }

        Services.Display.MoveToScreen(window, setup.ScreenIndex);

        //Topmost = true;
        Style = WindowStyle.None;
        State = WindowState.Maximized;

        var experimentView = _experimentViewFactory();

        _experimentViewModel = (Experiment)experimentView.DataContext;
        _experimentViewModel.ExperimentStopped += ExpVm_ExperimentStopped;

        var input = _inputFactory(setup.InputType);
        input.Register(_experiment, window, experimentView.itemsControl,
            () => _experimentViewModel.Targets);

        Page = experimentView;
    }

    private void ExpVm_ExperimentStopped(object? sender, Models.ExperimentSetup setup)
    {
        //Topmost = false;
        Style = WindowStyle.SingleBorderWindow;

        if (setup.ScreenIndex != _originalScreenIndex)
        {
            State = WindowState.Normal;
            Services.Display.MoveToScreen(Application.Current.MainWindow, _originalScreenIndex);
        }

        State = _originalWindowState;

        Page = _mainView;

        if (_experimentViewModel != null)
        {
            _experimentViewModel.ExperimentStopped -= ExpVm_ExperimentStopped;
            _experimentViewModel.Dispose();

            _experimentViewModel = null;
        }
    }

    #endregion
}
