using System.Windows;

namespace FittsLaw.Views;

public partial class Statistics : Window
{
    public Statistics(Models.StatisticsData[] statisticsData)
    {
        Owner = Application.Current.MainWindow;

        InitializeComponent();

        var vm = (ViewModels.Statistics)DataContext;
        vm.Items = statisticsData;
        vm.HideCopyToClipboardConfirmation += (s, e) =>
            Dispatcher.Invoke(() => vm.CopyToClipboardConfirmationVisibility = Visibility.Hidden);
    }
}
