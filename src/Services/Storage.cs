using Microsoft.Win32;

namespace FittsLaw.Services;

internal class Storage
{
    public enum Folders
    {
        Setups,
        Logs
    }

    public static string GetFolder(Folders? internalFolderId = null)
    {
        string internalFolder = internalFolderId?.ToString() ?? string.Empty;
        var result = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FittsLaw", internalFolder);
        if (!System.IO.Directory.Exists(result))
        {
            System.IO.Directory.CreateDirectory(result);
        }
        return result;
    }

    public static Storage For(string filter, string? initialFolder = null) =>
        new(filter, initialFolder);

    public bool Open(Action<string> action,
        string? filter = null,
        string? initialFolder = null)
    {
        var ofd = new OpenFileDialog()
        {
            Filter = filter ?? _filter,
            InitialDirectory = initialFolder ?? _initialFolder
        };

        if (ofd.ShowDialog() == true)
        {
            action(ofd.FileName);
            return true;
        }

        return false;
    }

    public bool Save(
        Action<string> action,
        string? filter = null,
        string? initialFolder = null)
    {
        var ofd = new SaveFileDialog()
        {
            Filter = filter ?? _filter,
            InitialDirectory = initialFolder ?? _initialFolder,
        };

        if (ofd.ShowDialog() == true)
        {
            action(ofd.FileName);
            return true;
        }

        return false;
    }

    #region Internal

    readonly string? _filter = null;
    readonly string? _initialFolder = null;

    Storage(string? filter, string? initialFolder)
    {
        _filter = filter;
        _initialFolder = initialFolder;
    }

    #endregion
}
