using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace FittsLaw.ViewModels;

internal partial class Experiment : ObservableObject, IDisposable
{
    public ObservableCollection<Models.Target> Targets { get; } = [];

    [ObservableProperty]
    public partial Visibility InstructionVisibility { get; set; } = Visibility.Collapsed;

    public Size ParentSize { get; }
    public Brush Background { get; }
    public Brush Foreground { get; }

    public event EventHandler<Models.ExperimentSetup>? ExperimentStopped;

    public Experiment()
    {
        _experiment.BlockStarted += Experiment_BlockStarted;
        _experiment.BlockFinished += Experiment_BlockFinished;
        _experiment.TargetChanged += Experiment_TargetChanged;
        _experiment.Finished += Experiment_Finished;

        InstructionVisibility = _experiment.Setup?.IsContinueManually == true ? Visibility.Visible : Visibility.Collapsed;
        ParentSize = _experiment.Setup!.LayoutType switch
        {
            Services.LayoutType.Circular => _experiment.GetCircularSize(),
            Services.LayoutType.Grid => Services.Display.GetScreenSize(_experiment.Setup.ScreenIndex),
            _ => throw new NotImplementedException(),
        };

        var uiSettings = Models.UiSettings.From(Properties.Settings.Default);
        Background = uiSettings.Background;
        Foreground = uiSettings.BorderBrush;
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

    readonly Services.Experiment _experiment = App.ServiceProvider.GetService<Services.Experiment>() 
        ?? throw new InvalidOperationException("Experiment service not available");

    private void Experiment_BlockStarted(object? sender, Models.Block block)
    {
        InstructionVisibility = Visibility.Collapsed;
        
        var setup = _experiment.Setup!;
        var targets = setup.LayoutType switch
        {
            Services.LayoutType.Circular => Services.LayoutCreator.CreateCircular(
                block,
                setup.TrialCount,
                ParentSize.Width),
            Services.LayoutType.Grid => Services.LayoutCreator.CreateGrid(
                block,
                setup.GridSize,
                ParentSize),
            _ => throw new InvalidOperationException($"Layout type {setup.LayoutType} is not yet implemented")
        };
   
        _experiment.SetTargets(targets);

        foreach (var target in targets)
        {
            Targets.Add(target);
        }
    }

    private void Experiment_BlockFinished(object? sender, bool hasNextBlock)
    {
        Targets.Clear();

        if (_experiment.Setup?.IsContinueManually == false || !hasNextBlock)
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
        foreach (var target in Targets)
        {
            target.IsActive = false;
        }

        Targets[index].IsActive = true;
    }

    private void Experiment_Finished(object? sender, bool wasInterrupted)
    {
        ExperimentStopped?.Invoke(this, _experiment.Setup!);

        if (!wasInterrupted)
        {
            var statisticsData = Services.Statistics.Compute(_experiment.Blocks);
            if (statisticsData.First().Value.Length == 0)
            {
                Services.Message.Error("No valid blocks.");
            }
            else
            {
                var dialog = new Views.Statistics(statisticsData);
                dialog.ShowDialog();
            }
        }
    }

    #endregion
}
