using System.Windows;

namespace FittsLaw.Views;

public partial class Setup : Window
{
    internal Setup(ViewModels.Setup viewModel)
    {
        Owner = Application.Current.MainWindow;

        InitializeComponent();
        DataContext = viewModel;
    }

    #region UI event handlers

    private void Start_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    #endregion
}
