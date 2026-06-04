using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.RegularExpressions;

namespace FittsLaw.ViewModels;

internal partial class Setup : ObservableObject
{
    public int[] TargetCounts { get; init; }
    public string[] InputTypes { get; init; }

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
    public partial string InputType { get; set; }

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
        InputTypes = typeof(Services.IInput)
            .Assembly
            .GetTypes()
            .Where(type =>
                type.Namespace == typeof(Services.IInput).Namespace &&
                typeof(Services.IInput).IsAssignableFrom(type) &&
                !type.IsAbstract &&
                type.IsClass)
            .Select(type => _wordSeparationRegex
                    .Replace(type.Name, " $1")
                    .Split(' ',  StringSplitOptions.RemoveEmptyEntries)[0])
            .Order()
            .ToArray();

        var props = Properties.Settings.Default;

        TargetCount = props.TrialCount;
        Amplitudes = props.Amplitudes;
        Widths = props.Widths;
        IsRandomized = props.IsRandomized;
        HasAudioFeedback = props.HasAudioFeedback;
        ContinuedManually = props.ContinuedManually;
        InputType = InputTypes.Contains(props.InputType) ? props.InputType : InputTypes.FirstOrDefault() ?? string.Empty;
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
        props.InputType = InputType;
        props.DisplayId = DisplayId;

        props.Save();
    }

    #endregion

    #region Internals

    public static Regex _wordSeparationRegex = new(@"([A-Z])", RegexOptions.Compiled);

    #endregion
}
