using System.Windows;

namespace FittsLaw.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Content = _mainView;

        var mainVm = (_mainView.DataContext as ViewModels.Main)!;
        mainVm.ExperimentStarted += (s, e) =>
        {
            //Topmost = true;
            WindowStyle = WindowStyle.None;

            var state = WindowState;
            WindowState = WindowState.Maximized;

            var experimentView = new Experiment();
            var expVm = (experimentView.DataContext as ViewModels.Experiment)!;
            expVm.ExperimentStopped += (s, e) =>
            {
                //Topmost = false;
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = state;

                Content = _mainView;
            };

            Content = experimentView;
        };
    }

    #region Internal

    Main _mainView = new();

    #endregion
}