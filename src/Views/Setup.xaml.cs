using System.Windows;

namespace FittsLaw.Views;

public partial class Setup : Window
{
    public Setup()
    {
        InitializeComponent();
    }

    private void Start_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
