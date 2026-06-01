using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;

namespace FittsLaw.ViewModels;

internal partial class Experiment : ObservableObject, IDisposable
{
    public ObservableCollection<Views.Target> Targets { get; } = [];

    [ObservableProperty]
    public partial Visibility InstructionVisibility { get; set; } = Visibility.Collapsed;

    public double ParentSize { get; }


    public event EventHandler? ExperimentStopped;


    public Experiment()
    {
        _experiment.BlockStarted += Experiment_BlockStarted;
        _experiment.BlockFinished += Experiment_BlockFinished;
        _experiment.TargetChanged += Experiment_TargetChanged;
        _experiment.Finished += Experiment_Finished;

        InstructionVisibility = _experiment.Setup?.ContinuedManually == true ? Visibility.Visible : Visibility.Collapsed;
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

    [RelayCommand]
    private void Continue()
    {
        _experiment.ResumeAfterBlock();
    }

    #endregion

    #region Internal

    readonly Services.Experiment _experiment = App.ServiceProvider.GetService<Services.Experiment>() ?? throw new Exception("Service is missing");
    readonly Services.Statistics _statistics = App.ServiceProvider.GetService<Services.Statistics>() ?? throw new Exception("Service is missing");

    Target[] _targetViewModels = [];

    private void Experiment_BlockStarted(object? sender, Models.Block block)
    {
        InstructionVisibility = Visibility.Collapsed;

        var targets = Helpers.BlockUiCreator.Create(block, _experiment.Setup!.TrialCount, ParentSize);
        _targetViewModels = targets.Select(t => (Target)t.DataContext).ToArray();
        _experiment.SetTargets(_targetViewModels.Select(vm => vm.Data));

        foreach (var target in targets)
        {
            Targets.Add(target);
        }
    }

    private void Experiment_BlockFinished(object? sender, bool hasNextBlock)
    {
        Targets.Clear();
        _targetViewModels = [];

        if (_experiment.Setup?.ContinuedManually == false || !hasNextBlock)
        {
            var delay = hasNextBlock ? 1000 : 10;
            Task.Delay(delay).ContinueWith(_ =>
            {
                _experiment.ResumeAfterBlock();
            });
        }
        else
        {
            InstructionVisibility = Visibility.Visible;
        }
    }

    private void Experiment_TargetChanged(object? sender, int index)
    {
        foreach (var vm in _targetViewModels)
        {
            vm.IsActive = false;
        }

        _targetViewModels[index].IsActive = true;

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
            var statisticsData = _statistics.Compute(_experiment.Blocks);

            var dialog = new Views.Statistics(statisticsData);
            dialog.ShowDialog();
        }
    }

    #endregion
}
