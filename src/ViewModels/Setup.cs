using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.RegularExpressions;
using System.Windows.Forms;

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
            .Select(type => _wordSeparationRegex
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
        var ofd = new SaveFileDialog()
        {
            Filter = "Setup file (*.fls)|*.fls",
            InitialDirectory = GetCacheFolder(),
        };
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            if (!Model.SaveToFile(ofd.FileName))
            {
                MessageBox.Show("Failed to save the setup file", App.Current.MainWindow.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    [RelayCommand]
    private void Load()
    {
        var ofd = new OpenFileDialog()
        {
            Filter = "Setup file (*.fls)|*.fls",
            InitialDirectory = GetCacheFolder(),
        };
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            var newModel = Models.ExperimentSetup.LoadFromFile(ofd.FileName);
            if (newModel != null)
            {
                ValidateModelValues(newModel);
                UpdateModelDependentProperties(newModel);

                Model = newModel;
                OnPropertyChanged("");
            }
            else
            {
                MessageBox.Show("Failed to load the setup file", App.Current.MainWindow.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    #endregion

    #region Internals

    static Regex _wordSeparationRegex = new(@"([A-Z])", RegexOptions.Compiled);

    private string GetCacheFolder()
    {
        var result = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FittsLaw");
        if (!System.IO.Directory.Exists(result))
        {
            System.IO.Directory.CreateDirectory(result);
        }
        return result;
    }

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

    partial void OnAmplitudesChanged(string value)
    {
        Model.Amplitudes = Models.ExperimentSetup.ToNumbers<double>(value);
    }

    partial void OnWidthsChanged(string value)
    {
        Model.Widths = Models.ExperimentSetup.ToNumbers<double>(value);
    }

    #endregion
}
