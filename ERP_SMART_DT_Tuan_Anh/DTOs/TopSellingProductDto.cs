namespace ERP_SMART_DT_Tuan_Anh.DTOs;

public class TopSellingProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public int SoldCount { get; set; }
    public decimal TotalAmount { get; set; }
}