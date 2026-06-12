using FittsLaw.Helpers;
using FittsLaw.Models;
using FittsLaw.Services;
using System.Windows;
using Target = FittsLaw.Models.Target;

namespace FittsLaw.Tests;

public class ExperimentTests
{
    /* These tests cannot be executed anymore as they require manual input
    [Fact]
    public async Task InterruptProducesExactlyOneInterruptedFinishedEvent()
    {
        var experiment = new Experiment();
        var results = new List<bool>();
        experiment.Finished += (_, wasInterrupted) => results.Add(wasInterrupted);

        var runTask = experiment.Run(CreateSetup());
        await WaitUntil(() => experiment.Blocks.Length > 0);

        experiment.Interrupt();
        await runTask;

        Assert.Equal([true], results);
    }

    [Fact]
    public async Task RunProducesNormalFinishedEvent()
    {
        var experiment = new Experiment();
        var results = new List<bool>();

        experiment.BlockStarted += (_, _) =>
        {
            experiment.SetTargets(
            [
                new Target
                {
                    Size = 20,
                    IsActive = true
                }
            ]);
        };
        experiment.TargetChanged += (_, _) => ResumeAfterTrialOnNextTick(experiment, new Point(10, 10));
        experiment.BlockFinished += (_, _) => ResumeAfterBlockOnNextTick(experiment);
        experiment.Finished += (_, wasInterrupted) => results.Add(wasInterrupted);

        await experiment.Run(CreateSetup(trialCount: 1));

        Assert.Equal([false], results);
    }

    [Fact]
    public async Task RunRaisesBlockTargetAndFinishedEventsInOrder()
    {
        var experiment = new Experiment();
        var events = new List<string>();

        experiment.BlockStarted += (_, _) =>
        {
            events.Add("block-started");
            experiment.SetTargets(
            [
                new Target
                {
                    Size = 20,
                    IsActive = true
                }
            ]);
        };
        experiment.TargetChanged += (_, _) =>
        {
            events.Add("target-changed");
            ResumeAfterTrialOnNextTick(experiment, new Point(10, 10));
        };
        experiment.BlockFinished += (_, _) =>
        {
            events.Add("block-finished");
            ResumeAfterBlockOnNextTick(experiment);
        };
        experiment.Finished += (_, _) => events.Add("finished");

        await experiment.Run(CreateSetup(trialCount: 1));

        Assert.Equal(
            ["block-started", "target-changed", "block-finished", "finished"],
            events);
    }

    [Fact]
    public async Task FirstTrialActivationOutsideTargetIsIgnored()
    {
        var experiment = new Experiment();
        var target = new Target
        {
            Size = 20,
            IsActive = true
        };
        var targetChangedCount = 0;

        experiment.BlockStarted += (_, _) => experiment.SetTargets([target]);
        experiment.TargetChanged += (_, _) =>
        {
            targetChangedCount++;
            if (targetChangedCount == 1)
            {
                ResumeAfterTrialOnNextTick(experiment, new Point(100, 100));
                ResumeAfterTrialOnNextTick(experiment, new Point(10, 10), delayMs: 30);
            }
        };
        experiment.BlockFinished += (_, _) => ResumeAfterBlockOnNextTick(experiment);

        await experiment.Run(CreateSetup(trialCount: 1));

        Assert.True(target.ActivationTimestamp > 0);
        Assert.Equal(new Point(0, 0), target.ActivationOffset);
    }
    */
    private static ExperimentSetup CreateSetup(int trialCount = 1) =>
        new()
        {
            SessionCount = 1,
            TrialCount = trialCount,
            LayoutType = LayoutType.Circular,
            Amplitudes = [100],
            Widths = [20],
            GridSize = new Models.Size() { Width = 3, Height = 2 },
            IsRandomized = false,
            HasAudioFeedback = false,
            IsDistinctErrorAudioFeedback = false,
            IsContinueManually = false,
            InputType = "Mouse",
            ScreenIndex = 0
        };

    private static void ResumeAfterTrialOnNextTick(
        Experiment experiment,
        Point activationLocation,
        int delayMs = 10)
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(delayMs);
            experiment.ResumeAfterTrial(activationLocation);
        });
    }

    private static void ResumeAfterBlockOnNextTick(Experiment experiment)
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(10);
            experiment.ResumeAfterBlock();
        });
    }

    private static async Task WaitUntil(Func<bool> predicate)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        while (!predicate())
        {
            if (cts.IsCancellationRequested)
                throw new TimeoutException("Timed out waiting for experiment state.");

            await Task.Delay(10);
        }
    }
}
