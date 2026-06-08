using System.Windows;

namespace FittsLaw.Services;

internal class TouchInput : InputBase
{
    public override void Register(Experiment experiment, UIElement root, UIElement container, Func<IEnumerable<Models.Target>> targetProvider)
    {
        base.Register(experiment, root, container, targetProvider);

        _root?.TouchDown += Target_TouchDown;
    }

    #region Internal

    protected override void Experiment_Finished(object? sender, bool interrupted)
    {
        _root?.TouchDown += Target_TouchDown;

        base.Experiment_Finished(sender, interrupted);
    }

    private void Target_TouchDown(object? sender, System.Windows.Input.TouchEventArgs e)
    {
        var clickPoint = e.GetTouchPoint(_root);
        SetClickPoint(clickPoint.Position);
    }

    #endregion
}
