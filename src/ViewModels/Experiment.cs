using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace FittsLaw.ViewModels;

internal partial class Experiment : ObservableObject, IDisposable
{
    public ObservableCollection<Views.Target> Targets { get; } = [];

    public double ParentSize { get; }

    public event EventHandler? ExperimentStopped;

    public Experiment()
    {
        _experiment.BlockStarted += Experiment_BlockStarted;
        _experiment.BlockFinished += Experiment_BlockFinished;
        _experiment.TargetChanged += Experiment_TargetChanged;
        _experiment.Finished += Experiment_Finished;

        ParentSize = _experiment.Blocks.Max(b => b.Amplitude + 2 * b.Width);
    }

    public void Dispose()
    {
        _experiment.BlockStarted -= Experiment_BlockStarted;
        _experiment.BlockFinished -= Experiment_BlockFinished;
        _experiment.TargetChanged -= Experiment_TargetChanged;
        _experiment.Finished -= Experiment_Finished;

        GC.SuppressFinalize(this);
    }

    #region Commands

    [RelayCommand]
    private void Interrupt()
    {
        _experiment.Interrupt();
    }

    #endregion

    #region Internal

    Services.Experiment _experiment = App.ServiceProvider.GetService<Services.Experiment>()!;

    private void Experiment_BlockStarted(object? sender, Models.Block block)
    {
        var targets = Helpers.BlockUiCreator.Create(block, _experiment.Setup!.TrialCount, ParentSize);
        _experiment.SetTargets(targets.Select(t => (t.DataContext as Target)!.Data).ToArray());

        foreach (var target in targets)
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
        foreach (var target in Targets)
        {
            var vm = (target.DataContext as Target)!;
            vm.IsActive = false;
        }

        var activeTargetVm = (Targets[index].DataContext as Target)!;
        activeTargetVm.IsActive = true;

        /*/ input replacement while developing
        Task.Delay(500).ContinueWith(_ =>
        {
            _experiment.ResumeAfterTrial();
        });
        //*/
    }

    private void Experiment_Finished(object? sender, bool wasInterrupted)
    {
        ExperimentStopped?.Invoke(this, EventArgs.Empty);

        if (!wasInterrupted)
        {
            foreach (var block in _experiment.Blocks)
            {
                foreach (var target in block.Targets)
                {
                    System.Diagnostics.Debug.WriteLine($"Target {target.Id}: Size={target.Size}, Click=({target.ActivationLocation.X}, {target.ActivationLocation.Y}), Time={target.ActivationTimestamp}ms");
                }
            }
        }
    }

    #endregion
}
