using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FittsLaw.Converters;

[ValueConversion(typeof(string), typeof(Thickness))]
public class StatKeyToMargin : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        (string)value == Services.Statistics.Fields[4]
            ? new Thickness(8, 8, 8, 2)
            : new Thickness(8, 2, 8, 2);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}