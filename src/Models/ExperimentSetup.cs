using CommunityToolkit.Mvvm.ComponentModel;

namespace FittsLaw.Models;

internal partial class ExperimentSetup : ObservableObject
{
    [ObservableProperty]
    public partial int SessionCount { get; set; }
    [ObservableProperty]
    public partial int TrialCount { get; set; }
    [ObservableProperty]
    public partial Helpers.LayoutType LayoutType { get; set; }
    public double[] Amplitudes { get; set; } = [];
    public double[] Widths { get; set; } = [];
    public System.Windows.Size GridSize { get; set; }
    [ObservableProperty]
    public partial bool IsRandomized { get; set; }
    [ObservableProperty]
    public partial bool HasAudioFeedback { get; set; }
    [ObservableProperty]
    public partial bool IsDistinctErrorAudioFeedback { get; set; }
    [ObservableProperty]
    public partial bool IsContinueManually { get; set; }
    [ObservableProperty]
    public partial string InputType { get; set; }
    [ObservableProperty]
    public partial int ScreenIndex { get; set; }

    public void Save()
    {
        var props = Properties.Settings.Default;

        props.SessionCount = SessionCount;
        props.TrialCount = TrialCount;
        props.LayoutType = (int)LayoutType;
        props.Amplitudes = ToString(Amplitudes);
        props.Widths = ToString(Widths);
        props.GridSize = ToString([GridSize.Width, GridSize.Height]);
        props.IsRandomized = IsRandomized;
        props.HasAudioFeedback = HasAudioFeedback;
        props.IsDistinctErrorAudioFeedback = IsDistinctErrorAudioFeedback;
        props.IsContinueManually = IsContinueManually;
        props.InputType = InputType;
        props.DisplayId = ScreenIndex;

        props.Save();
    }

    public static ExperimentSetup Load()
    {
        var props = Properties.Settings.Default;
        var gridSize = ToNumbers(props.GridSize);

        return new()
        {
            SessionCount = props.SessionCount,
            TrialCount = props.TrialCount,
            LayoutType = (Helpers.LayoutType)props.LayoutType,
            Amplitudes = ToNumbers(props.Amplitudes),
            Widths = ToNumbers(props.Widths),
            GridSize = new System.Windows.Size(gridSize[0], gridSize[1]),
            IsRandomized = props.IsRandomized,
            HasAudioFeedback = props.HasAudioFeedback,
            IsDistinctErrorAudioFeedback = props.IsDistinctErrorAudioFeedback,
            IsContinueManually = props.IsContinueManually,
            InputType = props.InputType,
            ScreenIndex = Math.Min(props.DisplayId, Helpers.Displays.Count - 1)
        };
    }

    public static string ToString(double[] Array) =>
        string.Join(' ', Array);

    public static double[] ToNumbers(string input, uint? exactCount = null)
    {
        var parts = input.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
        var numbers = new double[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (double.TryParse(parts[i], out double num) && num > 0)
            {
                numbers[i] = num;
            }
            else
            {
                throw new ArgumentException($"Invalid number: {parts[i]}");
            }
        }

        if (numbers.Length == 0)
            throw new ArgumentException("At least one number is required.");
        
        if (exactCount != null && exactCount != numbers.Length)
            throw new ArgumentException($"Exactly {exactCount} numbers must be specified.");

        return numbers;
    }
};
