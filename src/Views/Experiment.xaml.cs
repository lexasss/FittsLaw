using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FittsLaw.Views;

public partial class Experiment : Page
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

    #endregion
}
