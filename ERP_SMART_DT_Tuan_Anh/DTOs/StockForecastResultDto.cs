using ERP_SMART_DT_Tuan_Anh.Enums;
using System;

namespace ERP_SMART_DT_Tuan_Anh.DTOs
{
    public class StockForecastResultDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        // Các thuộc tính mà Service yêu cầu (đã thêm vào để hết lỗi)
        public int CurrentStock { get; set; }
        public int CurrentMinStock { get; set; }
        public int TotalSoldQuantity { get; set; }

        public int AnalysisDays { get; set; }
        public int LeadTimeDays { get; set; }
        public int SafetyDays { get; set; }

        public decimal AverageDailySales { get; set; }
        public decimal SafetyStock { get; set; }
        public int PredictedMinStock { get; set; }
        public decimal DaysUntilOutOfStock { get; set; }
        public int SuggestedImportQuantity { get; set; }

        public RiskLevel RiskLevel { get; set; }
        public string RiskLevelName { get; set; } = string.Empty;

        // THUỘC TÍNH MỚI: Tính ngày nhập hàng Real-time (Fix lỗi số 999)
        public string ExpectedRestockDateText
        {
            get
            {
                if (CurrentStock <= 0)
                    return $"🔥 Nhập ngay ({DateTime.Now.AddDays(LeadTimeDays):dd/MM/yyyy})";

                if (AverageDailySales <= 0)
                    return "∞ (Vô hạn)";

                double daysUntilEmpty = (double)CurrentStock / (double)AverageDailySales;
                return DateTime.Now.AddDays(daysUntilEmpty).ToString("dd/MM/yyyy");
            }
        }
    }
}