using System.Globalization;

namespace BakeryPay.Mobile.Converters;

public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool booleanValue && !booleanValue;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool booleanValue && !booleanValue;
}
