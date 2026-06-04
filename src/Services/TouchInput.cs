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

        _items = items;
    }

    #region Internal

    Window? _window;
    IReadOnlyCollection<object> _items = [];
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

        _items = [];
    }

    private void Target_TouchDown(object? sender, System.Windows.Input.TouchEventArgs e)
    {
        var target = _items
            .OfType<Views.Target>()
            .FirstOrDefault(t => ((ViewModels.Target)t.DataContext).IsActive);
        if (target != null)
            _experiment?.ResumeAfterTrial(e.GetTouchPoint(target).Position);
    }

    #endregion
}
