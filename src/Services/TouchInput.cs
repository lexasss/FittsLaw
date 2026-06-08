using System.Windows;

namespace FittsLaw.Services;

internal class TouchInput : IInput
{
    public void Register(Experiment experiment, UIElement root, UIElement container, Func<IEnumerable<Models.Target>> targetProvider)
    {
        _experiment = experiment;
        _experiment.Finished += Experiment_Finished;

        _root = root;
        _root.TouchDown += Target_TouchDown;

        _container = container;

        _targetProvider = targetProvider;
    }

    #region Internal

    UIElement? _root;
    UIElement? _container;
    Func<IEnumerable<Models.Target>>? _targetProvider;
    Experiment? _experiment;

    private void Experiment_Finished(object? sender, bool interrupted)
    {
        if (_experiment != null)
        {
            _experiment.Finished -= Experiment_Finished;
            _experiment = null;
        }

        if (_root != null)
        {
            _root.TouchDown -= Target_TouchDown;
            _root = null;
        }
    }

    private void Target_TouchDown(object? sender, System.Windows.Input.TouchEventArgs e)
    {
        var clickPoint = e.GetTouchPoint(_root);

        Models.Target? activeTarget = _targetProvider != null ?
            _targetProvider().FirstOrDefault(t => t.IsActive) :
            null;

        if (activeTarget != null)
        {
            var offset = _container!.TranslatePoint(new Point(0, 0), _root);
            _experiment?.ResumeAfterTrial(new Point(
                clickPoint.X - offset.X - activeTarget.Position.X,
                clickPoint.Y - offset.Y - activeTarget.Position.Y));
        }
    }

    #endregion
}
