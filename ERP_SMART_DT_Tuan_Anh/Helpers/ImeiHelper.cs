namespace ERP_SMART_DT_Tuan_Anh.Helpers;

public static class ImeiHelper
{
    public static List<string> ParseImeiList(string? rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return new List<string>();

        return rawText
            .Split(new[] { ',', ';', '\n', '\r', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();
    }

    public static bool IsValid(string? imei)
    {
        if (string.IsNullOrWhiteSpace(imei))
            return false;

        imei = imei.Trim();

        return imei.Length >= 10 && imei.Length <= 20;
    }

    public static string Normalize(string imei)
    {
        return imei.Trim();
    }

    public static string JoinImeiList(IEnumerable<string> imeiList)
    {
        return string.Join(",", imeiList.Select(Normalize).Where(IsValid).Distinct());
    }
}