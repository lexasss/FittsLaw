using System.Windows;

namespace FittsLaw.Services;

internal static class Message
{
    public static void Error(string text)
    {
        MessageBox.Show(text, Application.Current.MainWindow.Title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
