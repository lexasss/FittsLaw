using System.Windows.Media;

namespace FittsLaw.Models;

internal record class UiSettings(
    Brush Background, 
    Brush BorderBrush, 
    Brush ActiveTargetBrush, 
    Brush ActiveTargetBorderBrush,
    Brush CompletedTargetBorderBrush)
{
    public static UiSettings From(Properties.Settings settings)
    {
        return new UiSettings(
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.Background)),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.BorderColor)),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.ActiveTargetColor)),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.ActiveTargetBorderColor)),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.CompletedTargetBorderColor))
        );
    }

    public void Save()
    {
        var props = Properties.Settings.Default;
        props.Background = (Background as SolidColorBrush)?.Color.ToString() ?? "#FFFFFFFF";
        props.BorderColor = (BorderBrush as SolidColorBrush)?.Color.ToString() ?? "#FFFFFFFF";
        props.ActiveTargetColor = (ActiveTargetBrush as SolidColorBrush)?.Color.ToString() ?? "#FFFFFFFF";
        props.ActiveTargetBorderColor = (ActiveTargetBorderBrush as SolidColorBrush)?.Color.ToString() ?? "#FFFFFFFF";
        props.CompletedTargetBorderColor = (CompletedTargetBorderBrush as SolidColorBrush)?.Color.ToString() ?? "#FFFFFFFF";

        props.Save();
    }
}
