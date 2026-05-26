using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FittsLaw.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Closing += (s, e) =>
        {
            if (DataContext is ViewModels.MainWindowModel model)
            {
                if (!model.Save())
                {
                    MessageBox.Show("Failed to save settings.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        };
    }

    private void Color_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Content is Rectangle rect)
        {
            var currentColor = (rect.Fill as SolidColorBrush)?.Color ?? Colors.White;

            var dialog = new Egorozh.ColorPicker.Dialog.ColorPickerDialog() { Color = currentColor };
            if (dialog.ShowDialog() == true)
            {
                rect.Fill = new SolidColorBrush(dialog.Color);
            }
        }
    }
}