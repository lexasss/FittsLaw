using Microsoft.Win32;

namespace FittsLaw.Helpers;

internal class Storage
{
    public static string GetFolder(string? internalFolder = null)
    {
        var result = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FittsLaw", internalFolder ?? string.Empty);
        if (!System.IO.Directory.Exists(result))
        {
            System.IO.Directory.CreateDirectory(result);
        }
        return result;
    }

    public static Storage For(string filter, string? initialFolder = null) =>
        new Storage(filter, initialFolder);

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

    string? _filter = null;
    string? _initialFolder = null;

    Storage(string? filter, string? initialFolder)
    {
        _filter = filter;
        _initialFolder = initialFolder;
    }

    #endregion
}
