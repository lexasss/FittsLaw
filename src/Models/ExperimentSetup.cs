namespace FittsLaw.Models;

internal record class ExperimentSetup(
    int TrialCount,
    double[] Amplitudes,
    double[] Widths,
    bool IsRandomized,
    bool HasAudioFeedback,
    bool ContinuedManually
)
{
    public static ExperimentSetup From(ViewModels.Setup vm) =>
        new(vm.TargetCount,
            ToNumbers(vm.Amplitudes),
            ToNumbers(vm.Widths),
            vm.IsRandomized,
            vm.HasAudioFeedback,
            vm.ContinuedManually);

    private static double[] ToNumbers(string input)
    {
        var parts = input.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
        var numbers = new double[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (double.TryParse(parts[i], out double num))
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
