namespace ERP_SMART_DT_Tuan_Anh.DTOs;

public class DebtPaymentRequestDto
{
    public int ObjectId { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}