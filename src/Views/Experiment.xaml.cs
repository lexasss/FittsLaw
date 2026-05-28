using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FittsLaw.Views;

public partial class Experiment : Page, IDisposable
{
    public ItemsControl TargetContainer => itemsControl;

    public Experiment()
    {
        InitializeComponent();
    }

    #region Internal

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        Keyboard.Focus(this);
    }

    public void Dispose()
    {
        (DataContext as ViewModels.Experiment)?.Dispose();

        GC.SuppressFinalize(this);
    }

    #endregion
}
