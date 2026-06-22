using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.RegularExpressions;

namespace FittsLaw.ViewModels;

internal partial class Setup : ObservableObject
{
    public string[] InputTypes { get; init; }

    public Models.ExperimentSetup Model { get; private set; }

    [ObservableProperty]
    public partial string Amplitudes { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Widths { get; set; } = string.Empty;

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
            .Select(type => WordSeparationRegex()
                    .Replace(type.Name, " $1")      // this affects service provider functionality
                    .Split(' ',  StringSplitOptions.RemoveEmptyEntries)[0])
            .Order()
            .ToArray();

        Model = Models.ExperimentSetup.Load();

        ValidateModelValues(Model);
        UpdateModelDependentProperties(Model);
    }

    #region Commands

    [RelayCommand]
    private void Start()
    {
        Model.Save();
    }

    [RelayCommand]
    private void Save()
    {
        _setupFileStorage.Save(filename =>
        {
            if (!Model.SaveToFile(filename))
            {
                Helpers.Message.Error("Failed to save the setup file");
            }
        });
    }

    [RelayCommand]
    private void Load()
    {
        _setupFileStorage.Save(filename =>
        {
            var newModel = Models.ExperimentSetup.LoadFromFile(filename);
            if (newModel != null)
            {
                ValidateModelValues(newModel);
                UpdateModelDependentProperties(newModel);

                Model = newModel;
                OnPropertyChanged(""); // updates all fields
            }
            else
            {
                Helpers.Message.Error("Failed to load the setup file");
            }
        });
    }

    #endregion

    #region Property setters

    partial void OnAmplitudesChanged(string value)
    {
        Model.Amplitudes = Models.ExperimentSetup.ToNumbers<double>(value);
    }

    partial void OnWidthsChanged(string value)
    {
        Model.Widths = Models.ExperimentSetup.ToNumbers<double>(value);
    }

    #endregion

    #region Internals

    const string STORAGE_FILTER = "Setup files (*.fls)|*.fls";
    static readonly string STORAGE_FOLDER = Helpers.Storage.GetFolder(Helpers.Storage.Folders.Setups);

    readonly Helpers.Storage _setupFileStorage = Helpers.Storage.For(STORAGE_FILTER, STORAGE_FOLDER);

    [GeneratedRegex(@"([A-Z])", RegexOptions.Compiled)]
    private static partial Regex WordSeparationRegex();

    private void ValidateModelValues(Models.ExperimentSetup model)
    {
        if (!InputTypes.Contains(model.InputType))
        {
            model.InputType = InputTypes.FirstOrDefault() ?? string.Empty;
        }
    }

    private void UpdateModelDependentProperties(Models.ExperimentSetup model)
    {
        Amplitudes = Models.ExperimentSetup.ToString(model.Amplitudes);
        Widths = Models.ExperimentSetup.ToString(model.Widths);
    }

    #endregion
}
