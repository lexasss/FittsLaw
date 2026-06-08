using System.Windows;

namespace FittsLaw.Services;

internal class MouseInput : InputBase
{
    public override void Register(Experiment experiment, UIElement root, UIElement container, Func<IEnumerable<Models.Target>> targetProvider)
    {
        base.Register(experiment, root, container, targetProvider);

        _root?.PreviewMouseDown += Target_MouseDown;
    }

    #region Internal

    protected override void Experiment_Finished(object? sender, bool interrupted)
    {
        _root?.PreviewMouseDown -= Target_MouseDown;

        base.Experiment_Finished(sender, interrupted);
    }

    private void Target_MouseDown(object? sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        Point clickPoint = e.GetPosition(_root);
        SetClickPoint(clickPoint);
    }

    #endregion
}
