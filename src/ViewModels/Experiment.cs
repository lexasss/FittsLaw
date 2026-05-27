using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows.Shapes;

namespace FittsLaw.ViewModels;

internal class Experiment
{
    public ObservableCollection<Ellipse> Targets { get; } = [];

    public int ParentSize { get; }

    public event EventHandler? ExperimentStopped;

    public Experiment()
    {
        _experiment.BlockStarted += Experiment_BlockStarted;
        _experiment.BlockFinished += Experiment_BlockFinished;
        _experiment.TargetChanged += Experiment_TargetChanged;
        _experiment.Finished += Experiment_Finished;

        ParentSize = _experiment.Blocks.Max(b => b.Amplitude + 2 * b.Width);
    }

    #region Internal

    Services.Experiment _experiment = App.ServiceProvider.GetService<Services.Experiment>()!;
    Models.UiSettings _uiSettings = Models.UiSettings.From(Properties.Settings.Default);
    Ellipse[] _targets = [];

    private void Experiment_BlockStarted(object? sender, Models.Block block)
    {
        _targets = Helpers.BlockUiCreator.Create(block, _experiment.Setup!.TrialCount, ParentSize);

        foreach (var target in _targets)
        {
            Targets.Add(target);
        }
    }

    private void Experiment_BlockFinished(object? sender, bool hasNextBlock)
    {
        Targets.Clear();

        // input replacement while developing
        Task.Delay(1000).ContinueWith(_ =>
        {
            _experiment.ResumeAfterBlock();
        });
    }

    private void Experiment_TargetChanged(object? sender, int index)
    {
        foreach (var target in _targets)
        {
            target.Fill = null;
        }
        _targets[index].Fill = _uiSettings.Target;

        // input replacement while developing
        Task.Delay(500).ContinueWith(_ =>
        {
            _experiment.ResumeAfterTrial();
        });
    }

    private void Experiment_Finished(object? sender, bool wasInterrupted)
    {
        ExperimentStopped?.Invoke(this, EventArgs.Empty);

        if (!wasInterrupted)
        {
            // show statistics
        }
    }

    #endregion
}
