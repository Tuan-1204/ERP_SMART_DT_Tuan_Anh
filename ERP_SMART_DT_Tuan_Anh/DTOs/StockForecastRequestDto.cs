namespace ERP_SMART_DT_Tuan_Anh.DTOs;

public class StockForecastRequestDto
{
    public int AnalysisDays { get; set; } = 30;
    public int LeadTimeDays { get; set; } = 5;
    public int SafetyDays { get; set; } = 3;
}