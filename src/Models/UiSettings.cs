using System.Windows.Media;

namespace FittsLaw.Models;

internal record class UiSettings(Brush Background, Brush Border, Brush ActiveTarget)
{
    public static UiSettings From(ViewModels.Main main)
    {
        return new UiSettings(main.Background, main.Border, main.ActiveTarget);
    }

    public static UiSettings From(Properties.Settings settings)
    {
        return new UiSettings(
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.Background)),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.Border)),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.ActiveTarget))
        );
    }

    public void Save()
    {
        var props = Properties.Settings.Default;
        props.Background = (Background as SolidColorBrush)?.Color.ToString() ?? "#FFFFFFFF";
        props.Border = (Border as SolidColorBrush)?.Color.ToString() ?? "#FFFFFFFF";
        props.ActiveTarget = (ActiveTarget as SolidColorBrush)?.Color.ToString() ?? "#FFFFFFFF";

        props.Save();
    }
}
