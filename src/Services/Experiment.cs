using FittsLaw.Extensions;
using FittsLaw.Helpers;
using System.Windows;

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

    public Experiment()
    {
        _soundPlayer.Open(new Uri(System.IO.Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Assets",
            "Sounds",
            "selection.mp3")));
        _errorSoundPlayer.Open(new Uri(System.IO.Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Assets",
            "Sounds",
            "error.mp3")));
    }

    public void SetTargets(IEnumerable<Models.Target> targets)
    {
        _targets = targets;
    }

    public Size GetCircularSize()
    {
        var size = Blocks.Max(b => b.Amplitude + 2 * b.Width);
        return new(size, size);
    }

    public async Task Run(Models.ExperimentSetup setup)
    {
        if (_isRunning)
            return;

        Setup = setup;
        Blocks = CreateBlocks(setup);

        _isRunning = true;
        _isPaused = setup.IsContinueManually;
        _isWaitingForInput = false;
        _isInterrupted = false;

        Started?.Invoke(this, EventArgs.Empty);
        if (!await WaitFor(() => _isPaused))
            goto finalize;

        await Task.Delay(100);

        _blockIndex = 0;
        foreach (var block in Blocks)
        {
            BlockStarted?.Invoke(this, block);

            block.Targets = _targets;

            _stopwatch.Restart();

            for (_trialIndex = 0; _trialIndex < _targets.Count(); _trialIndex++)
            {
                TargetChanged?.Invoke(this, _trialIndex);

                _isWaitingForInput = true;
                if (!await WaitFor(() => _isWaitingForInput))
                    goto finalize;
            }

            BlockFinished?.Invoke(this, _blockIndex < Blocks.Length - 1);

            _isPaused = true;
            if (!await WaitFor(() => _isPaused))
                goto finalize;

            _blockIndex++;
        }

    finalize:

        _isRunning = false;

        Finished?.Invoke(this, _isInterrupted);
    }

    public void ResumeAfterTrial(Point activationLocation)
    {
        if (!_isRunning || !_isWaitingForInput)
            return;

        var target = _targets.FirstOrDefault(target => target.IsActive);
        if (target == null)
            return;

        var dx = activationLocation.X;
        var dy = activationLocation.Y;
        if (_trialIndex == 0 && Math.Sqrt(dx * dx + dy * dy) > target.Size / 2)
            return; // ignore activations outside the target for the first trial of the block

        target.ActivationTimestamp = _stopwatch.ElapsedMilliseconds;
        target.ActivationOffset = new Point(dx, dy);

        if (Setup?.HasAudioFeedback == true)
        {
            var player = target.ActivationOffset.Amplitude() > target.Size / 2 &&
                Setup?.IsDistinctErrorAudioFeedback == true
                    ? _errorSoundPlayer 
                    : _soundPlayer;
            player.Position = TimeSpan.Zero;
            player.Play();
        }

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

    readonly System.Diagnostics.Stopwatch _stopwatch = new();
    readonly System.Windows.Media.MediaPlayer _soundPlayer = new();
    readonly System.Windows.Media.MediaPlayer _errorSoundPlayer = new();

    IEnumerable<Models.Target> _targets = [];

    bool _isRunning = false;
    bool _isPaused = false;
    bool _isWaitingForInput = false;
    bool _isInterrupted = false;
    int _blockIndex;
    int _trialIndex;

    private static Models.Block[] CreateBlocks(Models.ExperimentSetup setup)
    {
        var result = new List<Models.Block>();
        int i = 0;
        for (int session = 0; session < setup.SessionCount; session++)
        {
            if (setup.LayoutType == LayoutType.Circular)
            {
                foreach (var amplitude in setup.Amplitudes)
                {
                    foreach (var width in setup.Widths)
                    {
                        result.Add(new(i++, amplitude, width));
                    }
                }
            }
            else if (setup.LayoutType == LayoutType.Grid)
            {
                foreach (var width in setup.Widths)
                {
                    result.Add(new(i++, 0, width));
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        if (setup.IsRandomized)
        {
            Random.Shared.Shuffle(result);
        }

        return result.ToArray();
    }

    private async Task<bool> WaitFor(Func<bool> condition)
    {
        while (condition())
        {
            if (_isInterrupted)
                return false;
            await Task.Delay(10);
        }
        return true;
    }

    #endregion
}
