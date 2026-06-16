using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FittsLaw.Converters;

[ValueConversion(typeof(Color), typeof(Brush))]
internal class ColorToBrush : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => 
        new SolidColorBrush((Color)value);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => 
        ((SolidColorBrush)value).Color;
}
