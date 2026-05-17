namespace ERP_SMART_DT_Tuan_Anh.DTOs;

public class InventoryAlertDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinStock { get; set; }
    public int MissingQuantity { get; set; }
}