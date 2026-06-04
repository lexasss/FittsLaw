using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FittsLaw.ViewModels;

internal partial class Setup : ObservableObject
{
    public int[] TargetCounts { get; init; }

    [ObservableProperty]
    public partial int TargetCount { get; set; }

    [ObservableProperty]
    public partial string Amplitudes { get; set; }

    [ObservableProperty]
    public partial string Widths { get; set; }

    [ObservableProperty]
    public partial bool IsRandomized { get; set; }

    [ObservableProperty]
    public partial bool HasAudioFeedback { get; set; } = false;

    [ObservableProperty]
    public partial bool ContinuedManually { get; set; } = false;

    [ObservableProperty]
    public partial int DisplayId { get; set; } = 0;

    public Setup()
    {
        var counts = new List<int>();
        for (int i = 3; i < 50; i += 2)
        {
            counts.Add(i);
        }

        TargetCounts = counts.ToArray();

        var props = Properties.Settings.Default;

        TargetCount = props.TrialCount;
        Amplitudes = props.Amplitudes;
        Widths = props.Widths;
        IsRandomized = props.IsRandomized;
        HasAudioFeedback = props.HasAudioFeedback;
        ContinuedManually = props.ContinuedManually;
        DisplayId = Math.Min(props.DisplayId, Helpers.Displays.Count - 1);
    }

    #region Commands

    [RelayCommand]
    private void Start()
    {
        var props = Properties.Settings.Default;

        props.TrialCount = TargetCount;
        props.Amplitudes = Amplitudes;
        props.Widths = Widths;
        props.IsRandomized = IsRandomized;
        props.HasAudioFeedback = HasAudioFeedback;
        props.ContinuedManually = ContinuedManually;
        props.DisplayId = DisplayId;

        props.Save();
    }

    #endregion

    #region Internals

    #endregion
}
