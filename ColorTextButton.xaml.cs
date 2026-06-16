using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FittsLaw.Views;

public partial class ColorTextButton : Button
{
    public static readonly DependencyProperty ColorProperty =
        DependencyProperty.Register(
            nameof(Color),
            typeof(Brush),
            typeof(ColorTextButton),
            new PropertyMetadata(Brushes.Transparent)
        );

    public Brush Color
    {
        get => (Brush)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(ColorTextButton),
            new PropertyMetadata(string.Empty));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public ColorTextButton()
    {
        InitializeComponent();
    }
}
