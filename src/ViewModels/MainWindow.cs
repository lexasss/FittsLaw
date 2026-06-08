using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
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

    public MainWindow()
    {
        _inputFactory = App.ServiceProvider.GetService<Func<string, Services.IInput>>()
            ?? throw new InvalidOperationException("Input service factory not available");
        _experiment = App.ServiceProvider.GetService<Services.Experiment>()
            ?? throw new InvalidOperationException("Experiment service not available");

        Page = _mainView;

        ((Main)_mainView.DataContext).ExperimentStarted += MainVm_ExperimentStarted;
    }


    #region Internal

    readonly Func<string, Services.IInput> _inputFactory;
    readonly Services.Experiment _experiment;
    readonly Window _window = Application.Current.MainWindow;

    readonly Views.Main _mainView = new();

    Experiment? _experimentViewModel;
    WindowState _originalWindowState;
    int _originalScreenIndex = 0;

    private void MainVm_ExperimentStarted(object? sender, Models.ExperimentSetup setup)
    {
        _originalWindowState = State;
        _originalScreenIndex = Helpers.Displays.GetScreenIndex(_window);

        if (setup.ScreenIndex != _originalScreenIndex && State == WindowState.Maximized)
        {
            State = WindowState.Normal;
        }

        Helpers.Displays.MoveToScreen(_window, setup.ScreenIndex);

        //Topmost = true;
        Style = WindowStyle.None;
        State = WindowState.Maximized;

        var experimentView = new Views.Experiment();

        _experimentViewModel = (Experiment)experimentView.DataContext;
        _experimentViewModel.ExperimentStopped += ExpVm_ExperimentStopped;

        var input = _inputFactory(setup.InputType);
        input.Register(_experiment, _window, experimentView.itemsControl,
            () => ((Experiment)experimentView.DataContext).Targets);

        Page = experimentView;
    }

    private void ExpVm_ExperimentStopped(object? sender, Models.ExperimentSetup setup)
    {
        //Topmost = false;
        Style = WindowStyle.SingleBorderWindow;

        if (setup.ScreenIndex != _originalScreenIndex)
        {
            State = WindowState.Normal;
            Helpers.Displays.MoveToScreen(_window, _originalScreenIndex);
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
