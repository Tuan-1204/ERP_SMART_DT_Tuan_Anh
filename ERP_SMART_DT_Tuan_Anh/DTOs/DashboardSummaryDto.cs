namespace ERP_SMART_DT_Tuan_Anh.DTOs;

public class DashboardSummaryDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalImport { get; set; }
    public int TotalStock { get; set; }
    public decimal TotalDebtToSupplier { get; set; }

    public int LowStockCount { get; set; }
    public int TotalProduct { get; set; }
    public int TotalCustomer { get; set; }
    public int TotalSupplier { get; set; }

    public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();
    public List<InventoryAlertDto> InventoryAlerts { get; set; } = new();
}