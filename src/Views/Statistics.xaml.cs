using System.Windows;

namespace FittsLaw.Views;

public partial class Statistics : Window
{
    internal Statistics(ViewModels.Statistics viewModel)
    {
        Owner = Application.Current.MainWindow;

        InitializeComponent();
        DataContext = viewModel;
        viewModel.HideCopyToClipboardConfirmation += (s, e) =>
            Dispatcher.Invoke(() => viewModel.CopyToClipboardConfirmationVisibility = Visibility.Hidden);
    }

    public void SetStatisticsData(IReadOnlyDictionary<string, string[]> statisticsData)
    {
        ((ViewModels.Statistics)DataContext).Items = statisticsData;
    }
}
