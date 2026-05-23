using System.Globalization;
using System.Text;

namespace ERP_SMART_DT_Tuan_Anh.Helpers;

public static class MoneyHelper
{
    public static decimal ParseVnd(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        var builder = new StringBuilder();
        foreach (var c in text)
        {
            if (char.IsDigit(c))
                builder.Append(c);
        }

        return decimal.TryParse(builder.ToString(), out var value) ? value : 0;
    }

    public static string FormatInput(decimal value)
    {
        return value <= 0 ? string.Empty : value.ToString("N0", CultureInfo.InvariantCulture);
    }

    public static string NormalizeInput(string? text)
    {
        return FormatInput(ParseVnd(text));
    }
}
