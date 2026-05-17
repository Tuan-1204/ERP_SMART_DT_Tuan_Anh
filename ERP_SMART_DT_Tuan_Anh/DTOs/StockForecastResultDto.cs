using ERP_SMART_DT_Tuan_Anh.Enums;

namespace ERP_SMART_DT_Tuan_Anh.DTOs;

public class StockForecastResultDto
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;

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
}