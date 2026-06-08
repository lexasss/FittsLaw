using System.Windows;

namespace FittsLaw.Services;

internal abstract class InputBase : IInput
{
    public virtual void Register(Experiment experiment, UIElement root, UIElement container, Func<IEnumerable<Models.Target>> targetProvider)
    {
        _experiment = experiment;
        _experiment.Finished += Experiment_Finished;

        _root = root;

        _container = container;

        _targetProvider = targetProvider;
    }

    #region Shared

    protected UIElement? _root;

    // To be called by derived classes when a click is detected, to set the click point for the experiment.
    protected bool SetClickPoint(Point point)
    {
        var activeTarget = _targetProvider != null ?
            _targetProvider().FirstOrDefault(t => t.IsActive) :
            null;

        if (activeTarget != null)
        {
            var offset = _container!.TranslatePoint(ZeroPoint, _root);
            _experiment?.ResumeAfterTrial(new Point(
                point.X - offset.X - activeTarget.Position.X,
                point.Y - offset.Y - activeTarget.Position.Y));
            return true;
        }
        return false;
    }

    #endregion

    #region Internal

    readonly Point ZeroPoint;

    UIElement? _container;
    Func<IEnumerable<Models.Target>>? _targetProvider;
    Experiment? _experiment;

    protected virtual void Experiment_Finished(object? sender, bool interrupted)
    {
        if (_experiment != null)
        {
            _experiment.Finished -= Experiment_Finished;
            _experiment = null;
        }

        _root = null;
    }

    #endregion
}
