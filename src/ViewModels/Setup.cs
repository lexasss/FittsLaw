using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.RegularExpressions;

namespace FittsLaw.ViewModels;

internal partial class Setup : ObservableObject
{
    public string[] InputTypes { get; init; }

    public Models.ExperimentSetup Model { get; private set; }

    [ObservableProperty]
    public partial string Amplitudes { get; set; }

    [ObservableProperty]
    public partial string Widths { get; set; }

    public Setup()
    {
        InputTypes = typeof(Services.IInput)
            .Assembly
            .GetTypes()
            .Where(type =>
                type.Namespace == typeof(Services.IInput).Namespace &&
                typeof(Services.IInput).IsAssignableFrom(type) &&
                !type.IsAbstract &&
                type.IsClass)
            .Select(type => _wordSeparationRegex
                    .Replace(type.Name, " $1")      // this affects service provider functionality
                    .Split(' ',  StringSplitOptions.RemoveEmptyEntries)[0])
            .Order()
            .ToArray();

        Model = Models.ExperimentSetup.Load();

        if (!InputTypes.Contains(Model.InputType))
        {
            Model = Model with { InputType = InputTypes.FirstOrDefault() ?? string.Empty };
        }

        Amplitudes = Models.ExperimentSetup.ToString(Model.Amplitudes);
        Widths = Models.ExperimentSetup.ToString(Model.Widths);
    }

    #region Commands

    [RelayCommand]
    private void Start()
    {
        Model.Save();
    }

    #endregion

    #region Internals

    public static Regex _wordSeparationRegex = new(@"([A-Z])", RegexOptions.Compiled);

    partial void OnAmplitudesChanged(string value)
    {
        Model = Model with { Amplitudes = Models.ExperimentSetup.ToNumbers(value) };
    }

    partial void OnWidthsChanged(string value)
    {
        Model = Model with { Widths = Models.ExperimentSetup.ToNumbers(value) };
    }

    #endregion
}
