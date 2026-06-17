using System.Windows;

namespace FittsLaw.Helpers;

internal static class Message
{
    public static void Error(string text)
    {
        MessageBox.Show(text, App.Current.MainWindow.Title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
