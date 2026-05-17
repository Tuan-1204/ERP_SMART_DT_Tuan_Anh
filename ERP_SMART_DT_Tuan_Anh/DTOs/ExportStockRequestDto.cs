namespace ERP_SMART_DT_Tuan_Anh.DTOs;

public class ExportStockRequestDto
{
    public string BillId { get; set; } = string.Empty;
    public int ObjectId { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public List<string> ImeiList { get; set; } = new();

    public string ImeiListText => string.Join(",", ImeiList);
}