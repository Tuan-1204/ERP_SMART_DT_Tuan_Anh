using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ERP_SMART_DT_Tuan_Anh.Converters;

public class SidebarActiveBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var activeMenu = value?.ToString() ?? string.Empty;
        var rawParameter = parameter?.ToString() ?? string.Empty;
        var parts = rawParameter.Split(':');

        var menuKey = parts.Length > 0 ? parts[0] : string.Empty;
        var brushType = parts.Length > 1 ? parts[1] : "Background";
        var isActive = string.Equals(activeMenu, menuKey, StringComparison.OrdinalIgnoreCase);

        return brushType switch
        {
            "Foreground" => CreateBrush(isActive ? "#2F65B0" : "#1F2933"),
            "Border" => CreateBrush(isActive ? "#2F65B0" : "#555555"),
            _ => CreateBrush(isActive ? "#DCEBFF" : "#FFFFFF")
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static SolidColorBrush CreateBrush(string color)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        brush.Freeze();
        return brush;
    }
}
