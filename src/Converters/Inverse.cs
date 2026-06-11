using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FittsLaw.Converters;

[ValueConversion(typeof(SolidColorBrush), typeof(Brush))]
internal class Inverse : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => 
        !(bool)value;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => 
        !(bool)value;
}
