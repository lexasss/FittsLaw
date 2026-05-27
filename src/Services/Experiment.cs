using FittsLaw.Helpers;

namespace FittsLaw.Services;

internal class Experiment
{
    public Models.ExperimentSetup? Setup { get; private set; }
    public Models.Block[] Blocks { get; private set; } = [];

    public event EventHandler? Started;

    /// <summary>
    /// The argument is true if the experiment was interrupted, false if it finished normally.
    /// </summary>
    public event EventHandler<bool>? Finished;

    public event EventHandler<Models.Block>? BlockStarted;

    /// <summary>
    /// The argument is true if this was not the last block of the experiment, false otherwise.
    /// </summary>
    public event EventHandler<bool>? BlockFinished;

    /// <summary>
    /// Occurs when the target changes during the experiment. The argument is the current trial/target index within the block (starting from 0).
    /// </summary>
    public event EventHandler<int>? TargetChanged;

    public async Task Run(Models.ExperimentSetup setup)
    {
        if (_isRunning)
            return;

        Setup = setup;
        Blocks = CreateBlocks(setup);

        _isRunning = true;
        Started?.Invoke(this, EventArgs.Empty);

        await Task.Delay(100);

        int i = 0;
        foreach (var block in Blocks)
        {
            BlockStarted?.Invoke(this, block);
            System.Diagnostics.Debug.WriteLine($"Started block {block.Index} with amplitude {block.Amplitude} and width {block.Width}");

            for (int trial = 0; trial < Setup.TrialCount; trial++)
            {
                TargetChanged?.Invoke(this, trial);
                System.Diagnostics.Debug.WriteLine($"Target {trial} shown");

                _isWaitingForInput = true;
                while (_isWaitingForInput && !_isInterrupted)
                {
                    await Task.Delay(10);
                }
                if (_isInterrupted)
                {
                    goto finalize;
                }
                System.Diagnostics.Debug.WriteLine($"Target {trial} hidden");
            }

            BlockFinished?.Invoke(this, i < Blocks.Length - 1);
            System.Diagnostics.Debug.WriteLine($"Block finished");

            _isPaused = true;
            while (_isPaused && !_isInterrupted)
            {
                await Task.Delay(10);
            }
            if (_isInterrupted)
            {
                goto finalize;
            }

            System.Diagnostics.Debug.WriteLine($"Block done");
            i++;
        }

        System.Diagnostics.Debug.WriteLine($"All done");

        await Task.Delay(1000);

    finalize:
        _isRunning = false;
        Finished?.Invoke(this, false);
    }

    public void ResumeAfterTrial()
    {
        if (!_isRunning || !_isWaitingForInput)
            return;

        _isWaitingForInput = false;
    }

    public void ResumeAfterBlock()
    {
        if (!_isRunning || !_isPaused)
            return;

        _isPaused = false;
    }

    public void Interrupt()
    {
        if (!_isRunning)
            return;

        _isPaused = false;
        _isWaitingForInput = false;
        _isInterrupted = true;

        Finished?.Invoke(this, true);
    }

    #region Internal

    bool _isRunning = false;
    bool _isPaused = false;
    bool _isWaitingForInput = false;
    bool _isInterrupted = false;

    private static Models.Block[] CreateBlocks(Models.ExperimentSetup setup)
    {
        var result = new List<Models.Block>();
        int i = 0;
        foreach (var amplitude in setup.Amplitudes)
        {
            foreach (var width in setup.Widths)
            {
                result.Add(new(i++, amplitude, width));
            }
        }

        if (setup.IsRandomized)
        {
            Random.Shared.Shuffle(result);
        }

        return result.ToArray();
    }

    #endregion
}
