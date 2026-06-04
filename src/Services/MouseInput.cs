using System.Windows;

namespace FittsLaw.Services;

internal class MouseInput : IInput
{
    public void Register(Window window, IReadOnlyCollection<object> items, Experiment experiment)
    {
        _experiment = experiment;
        _experiment.Finished += Experiment_Finished;

        _window = window;
        _window.PreviewMouseDown += Target_MouseDown;

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
            _window.PreviewMouseDown -= Target_MouseDown;
            _window = null;
        }

        _items = [];
    }

    private void Target_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var target = _items
            .OfType<Views.Target>()
            .FirstOrDefault(t => ((ViewModels.Target)t.DataContext).IsActive);
        if (target != null)
            _experiment?.ResumeAfterTrial(e.GetPosition(target));
    }

    #endregion
}
