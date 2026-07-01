using System.Windows;

namespace FittsLaw.Views;

public partial class Setup : Window
{
    public Setup()
    {
        Owner = Application.Current.MainWindow;

        InitializeComponent();
    }

    #region UI event handlers

    private void Start_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    #endregion
}
