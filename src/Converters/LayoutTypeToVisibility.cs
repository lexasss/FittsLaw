using FittsLaw.Services;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FittsLaw.Converters;

[ValueConversion(typeof(LayoutType), typeof(Visibility))]
internal class LayoutTypeToVisibility : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => 
        (LayoutType)value == (LayoutType)parameter
            ? Visibility.Visible
            : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => 
        throw new NotImplementedException();
}