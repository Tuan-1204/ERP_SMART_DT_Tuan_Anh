using ERP_SMART_DT_Tuan_Anh.Enums;

namespace ERP_SMART_DT_Tuan_Anh.Helpers;

public static class BillCodeGenerator
{
    public static string GenerateImportCode(int sequence)
    {
        return Generate(BillTypeValues.Import, sequence);
    }

    public static string GenerateExportCode(int sequence)
    {
        return Generate(BillTypeValues.Export, sequence);
    }

    public static string Generate(string billType, int sequence)
    {
        var prefix = billType == BillTypeValues.Import ? "PN" : "PX";
        return $"{prefix}-{DateTime.Now:yyyyMMdd}-{sequence:000}";
    }
}