using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FittsLaw.ViewModels;

internal partial class SetupModel : ObservableObject
{
    public int[] TrialCounts { get; init; }

    [ObservableProperty]
    public partial string Amplitudes { get; set; }

    [ObservableProperty]
    public partial string Widths { get; set; }

    [ObservableProperty]
    public partial int SelectedTrialCount { get; set; }

    public SetupModel()
    {
        var counts = new List<int>();
        for (int i = 3; i < 50; i += 2)
        {
            counts.Add(i);
        }

        TrialCounts = counts.ToArray();

        var props = Properties.Settings.Default;

        Amplitudes = props.Amplitudes;
        Widths = props.Widths;
        SelectedTrialCount = props.TrialCount;
    }

    #region RelayCommand

    [RelayCommand]
    private void Start()
    {
        var props = Properties.Settings.Default;

        props.Amplitudes = props.Amplitudes;
        props.Widths = props.Widths;
        props.TrialCount = SelectedTrialCount;

        props.Save();
    }

    [RelayCommand]
    private void Cancel()
    {
    }

    #endregion

    #region Internals
    /*
    const char Separator = ' ';

    private static int[] ToIntegers(string str)
    {
        var parts = str.Split(Separator);
        var integers = new List<int>();
        foreach (var part in parts)
        {
            if (int.TryParse(part.Trim(), out int value))
            {
                integers.Add(value);
            }
        }
        return integers.ToArray();
    }
    */
    #endregion
}
