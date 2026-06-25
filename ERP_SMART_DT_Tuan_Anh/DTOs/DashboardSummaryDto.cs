using System.Collections.Generic;

namespace ERP_SMART_DT_Tuan_Anh.DTOs
{
    public class DashboardSummaryDto
    {
        // 4 Chỉ số tài chính & kho lớn
        public decimal TotalRevenue { get; set; }
        public decimal TotalImport { get; set; }
        public int TotalStock { get; set; }
        public decimal TotalDebtToSupplier { get; set; }

        // 4 Chỉ số thống kê thực thể bổ sung
        public int LowStockCount { get; set; }
        public int TotalProduct { get; set; }
        public int TotalCustomer { get; set; }
        public int TotalSupplier { get; set; }

        // Danh sách mảng đối tượng chi tiết
        public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();
        public List<InventoryAlertDto> InventoryAlerts { get; set; } = new();
    }

    public class TopSellingProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int SoldCount { get; set; }
    }

    public class InventoryAlertDto
    {
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinStock { get; set; }
    }
}