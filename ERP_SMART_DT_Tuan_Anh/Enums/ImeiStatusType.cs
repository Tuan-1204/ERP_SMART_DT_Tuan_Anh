namespace ERP_SMART_DT_Tuan_Anh.Enums;

public enum ImeiStatusType
{
    TrongKho = 1,
    DaBan = 2,
    BaoHanh = 3,
    Loi = 4,
    TraHang = 5
}

public static class ImeiStatusNames
{
    public const string TrongKho = "Trong kho";
    public const string DaBan = "Đã bán";
    public const string BaoHanh = "Bảo hành";
    public const string Loi = "Lỗi";
    public const string TraHang = "Trả hàng";
}