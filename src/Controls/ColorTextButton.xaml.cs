using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FittsLaw.Controls;

public partial class ColorTextButton : Button
{
    #region Color
    public static readonly DependencyProperty ColorProperty =
        DependencyProperty.Register(
            nameof(Color),
            typeof(Color),
            typeof(ColorTextButton),
            new PropertyMetadata(Colors.Transparent)
        );

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }
    #endregion

    #region Text
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
    #endregion

    static ColorTextButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorTextButton),
            new FrameworkPropertyMetadata(typeof(ColorTextButton)));
    }
}
