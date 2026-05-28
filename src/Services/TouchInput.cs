using System.Windows;

namespace FittsLaw.Services;

internal class TouchInput : IInput
{
    public void Register(Window window, IReadOnlyCollection<object> items, Experiment experiment)
    {
        _experiment = experiment;
        _experiment.Finished += Experiment_Finished;

        _window = window;
        _window.TouchDown += Target_TouchDown;
    }

    #region Internal

    Window? _window;
    Experiment? _experiment;

    private void Experiment_Finished(object? sender, bool interrupted)
    {
        if (_experiment != null)
        {
            _experiment.Finished -= Experiment_Finished;
            _experiment = null;
        }

        if (_window != null)
        {
            _window.TouchDown -= Target_TouchDown;
            _window = null;
        }
    }

    private void Target_TouchDown(object? sender, System.Windows.Input.TouchEventArgs e)
    {
        _experiment?.ResumeAfterTrial(e.GetTouchPoint(_window).Position);
    }

    #endregion
}
