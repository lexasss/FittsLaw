namespace FittsLaw.Models;

internal record class ExperimentSetup(
    int TrialCount,
    int[] Amplitudes,
    int[] Widths,
    bool IsRandomized,
    bool HasAudioFeedback
)
{
    public static ExperimentSetup From(ViewModels.Setup vm) =>
        new(vm.TargetCount,
            ToIntegers(vm.Amplitudes),
            ToIntegers(vm.Widths),
            vm.IsRandomized,
            vm.HasAudioFeedback);

    private static int[] ToIntegers(string input)
    {
        var parts = input.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
        var numbers = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (int.TryParse(parts[i], out int num))
            {
                numbers[i] = num;
            }
            else
            {
                throw new FormatException($"Invalid number: {parts[i]}");
            }
        }

        return numbers;
    }
};
