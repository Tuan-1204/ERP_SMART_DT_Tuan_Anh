namespace ERP_SMART_DT_Tuan_Anh.DTOs;

public class InventoryCheckMasterDto
{
    public int UserId { get; set; }
    public string? Note { get; set; }
    public List<InventoryCheckDetailDto> Details { get; set; } = new();
}
