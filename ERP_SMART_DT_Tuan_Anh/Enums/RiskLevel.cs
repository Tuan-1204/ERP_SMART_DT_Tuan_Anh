namespace ERP_SMART_DT_Tuan_Anh.Enums;

public enum RiskLevel
{
    BinhThuong = 1,
    CanTheoDoi = 2,
    SapHetHang = 3,
    HetHang = 4
}

public static class RiskLevelText
{
    public const string BinhThuong = "Bình thường";
    public const string CanTheoDoi = "Cần theo dõi";
    public const string SapHetHang = "Sắp hết hàng";
    public const string HetHang = "Hết hàng";
}