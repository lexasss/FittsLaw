using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace FittsLaw.Controls;

[TemplatePart(Name = "PART_TextBlock", Type = typeof(TextBlock))]
public partial class Hyperlink : Control
{
    public static readonly DependencyProperty UriProperty =
        DependencyProperty.Register("Uri",
            typeof(string),
            typeof(Hyperlink),
            new PropertyMetadata(null));

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Text",
            typeof(string),
            typeof(Hyperlink),
            new PropertyMetadata(string.Empty));

    public string Uri
    {
        get => (string)GetValue(UriProperty);
        set => SetValue(UriProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        var link = GetTemplateChild("PART_Link") as System.Windows.Documents.Hyperlink;
        link?.Click += Link_Click;
    }

    #region Internal

    static Hyperlink()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Hyperlink),
            new FrameworkPropertyMetadata(typeof(Hyperlink)));
    }

    private void Link_Click(object sender, RoutedEventArgs e)
    {
        if (System.IO.Directory.Exists(Uri))
        {
            Process.Start("explorer.exe", Uri);
        }
    }

    #endregion
}