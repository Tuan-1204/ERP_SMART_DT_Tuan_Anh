namespace ERP_SMART_DT_Tuan_Anh.Helpers;

public static class DateTimeHelper
{
    public static DateTime StartOfDay(DateTime date)
    {
        return date.Date;
    }

    public static DateTime EndOfDay(DateTime date)
    {
        return date.Date.AddDays(1).AddTicks(-1);
    }

    public static DateTime GetFromDateByDays(int days)
    {
        if (days <= 0)
            days = 30;

        return DateTime.Now.Date.AddDays(-days);
    }

    public static string ToVietnameseDateTime(DateTime? dateTime)
    {
        return dateTime.HasValue ? dateTime.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty;
    }

    public static string ToVietnameseDate(DateTime? dateTime)
    {
        return dateTime.HasValue ? dateTime.Value.ToString("dd/MM/yyyy") : string.Empty;
    }
}