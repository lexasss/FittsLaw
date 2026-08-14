using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FittsLaw.Views;

public partial class Experiment : Page
{
    internal Experiment(ViewModels.Experiment viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    #region UI event handlers

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        Keyboard.Focus(this);
    }

    #endregion
}
