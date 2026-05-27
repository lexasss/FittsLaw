using System.Windows.Media;

namespace FittsLaw.Models;

internal record class UiSettings(Brush Background, Brush Foreground, Brush Target)
{
    public static UiSettings From(ViewModels.Main main)
    {
        return new UiSettings(main.Background, main.Foreground, main.Target);
    }

    public static UiSettings From(Properties.Settings settings)
    {
        return new UiSettings(
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.Background)),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.Foreground)),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.Target))
        );
    }

    public void Save()
    {
        var props = Properties.Settings.Default;
        props.Background = (Background as SolidColorBrush)?.Color.ToString() ?? "#FFFFFFFF";
        props.Foreground = (Foreground as SolidColorBrush)?.Color.ToString() ?? "#FFFFFFFF";
        props.Target = (Target as SolidColorBrush)?.Color.ToString() ?? "#FFFFFFFF";

        props.Save();
    }
}
