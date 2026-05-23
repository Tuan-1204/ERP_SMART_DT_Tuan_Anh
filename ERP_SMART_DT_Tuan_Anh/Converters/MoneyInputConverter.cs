using System;
using System.Globalization;
using System.Windows.Data;
using ERP_SMART_DT_Tuan_Anh.Helpers;

namespace ERP_SMART_DT_Tuan_Anh.Converters;

public class MoneyInputConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return string.Empty;

        return decimal.TryParse(value.ToString(), out var money)
            ? MoneyHelper.FormatInput(money)
            : string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return MoneyHelper.ParseVnd(value?.ToString());
    }
}
