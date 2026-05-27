using FittsLaw.Helpers;

namespace FittsLaw.Services;

internal class Experiment
{
    public event EventHandler? Started;
    public event EventHandler? Finished;
    public event EventHandler<Models.Block>? BlockStarted;
    public event EventHandler<Models.Block>? BlockFinished;
    public event EventHandler<int>? TargetChanged;

    public async Task Run(Models.ExperimentSetup setup)
    {
        if (_isRunning)
            return;

        _setup = setup;
        _blocks = CreateBlocks(setup);

        _isRunning = true;
        Started?.Invoke(this, EventArgs.Empty);

        await Task.Delay(100);

        foreach (var block in _blocks)
        {
            BlockStarted?.Invoke(this, block);

            for (int trial = 0; trial < _setup.TrialCount; trial++)
            {
                TargetChanged?.Invoke(this, trial);

                _isWaitingForInput = true;
                while (_isWaitingForInput && !_isInterrupted)
                {
                    await Task.Delay(10);
                }
                if (_isInterrupted)
                {
                    goto finalize;
                }
            }

            BlockFinished?.Invoke(this, block);

            _isPaused = true;
            while (_isPaused && !_isInterrupted)
            {
                await Task.Delay(10);
            }
            if (_isInterrupted)
            {
                goto finalize;
            }
        }

    finalize:
        _isRunning = false;
        Finished?.Invoke(this, EventArgs.Empty);
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
    }

    #region Internal

    Models.ExperimentSetup? _setup;
    Models.Block[]? _blocks;

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
