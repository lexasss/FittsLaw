namespace FittsLaw.Models;

internal record class ExperimentSetup(
    int SessionCount,
    int TrialCount,
    double[] Amplitudes,
    double[] Widths,
    bool IsRandomized,
    bool HasAudioFeedback,
    bool IsDistinctErrorAudioFeedback,
    bool IsContinueManually,
    string InputType,
    int ScreenIndex
)
{
    public void Save()
    {
        var props = Properties.Settings.Default;

        props.SessionCount = SessionCount;
        props.TrialCount = TrialCount;
        props.Amplitudes = ToString(Amplitudes);
        props.Widths = ToString(Widths);
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

        return new(
            props.SessionCount,
            props.TrialCount,
            ToNumbers(props.Amplitudes),
            ToNumbers(props.Widths),
            props.IsRandomized,
            props.HasAudioFeedback,
            props.IsDistinctErrorAudioFeedback,
            props.IsContinueManually,
            props.InputType,
            Math.Min(props.DisplayId, Helpers.Displays.Count - 1));
    }

    public static string ToString(double[] Array) =>
        string.Join(' ', Array);

    public static double[] ToNumbers(string input)
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

        return numbers;
    }
};
