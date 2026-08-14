using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FittsLaw.Views;

public partial class Main : Page
{
    internal Main(ViewModels.Main viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    #region UI event handlers

    private void Color_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Controls.ColorTextButton btn)
        {
            var currentColor = btn.Color;

            var dialog = new Egorozh.ColorPicker.Dialog.ColorPickerDialog() { Color = currentColor };
            if (dialog.ShowDialog() == true)
            {
                btn.Color = dialog.Color;
            }
        }
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        Keyboard.Focus(this);
    }

    #endregion
}
