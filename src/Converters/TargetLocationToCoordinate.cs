using System.Globalization;
using System.Windows.Data;

namespace FittsLaw.Converters;

[ValueConversion(typeof(double), typeof(double))]
internal class TargetLocationToCoordinate : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => 
        (double)values[0] - (double)values[1] / 2;

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => 
        throw new NotImplementedException();
}
