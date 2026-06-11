using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FittsLaw.Converters;

[ValueConversion(typeof(Helpers.LayoutType), typeof(Visibility))]
internal class LayoutTypeToVisibility : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => 
        (Helpers.LayoutType)value == (Helpers.LayoutType)parameter
            ? Visibility.Visible
            : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => 
        throw new NotImplementedException();
}