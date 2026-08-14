using System.Windows;

namespace FittsLaw.Views;

public partial class MainWindow : Window
{
    internal MainWindow(ViewModels.MainWindow viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
