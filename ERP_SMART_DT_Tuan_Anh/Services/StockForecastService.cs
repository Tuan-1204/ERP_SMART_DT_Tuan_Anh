using ERP_SMART_DT_Tuan_Anh.Data;
using ERP_SMART_DT_Tuan_Anh.DTOs;
using ERP_SMART_DT_Tuan_Anh.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_SMART_DT_Tuan_Anh.Services;

public class StockForecastService
{
    public async Task<List<StockForecastResultDto>> ForecastAsync(StockForecastRequestDto request)
    {
        await using var db = DbContextFactory.Create();

        var fromDate = DateTime.Now.Date.AddDays(-request.AnalysisDays);

        var products = await db.Products
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.ProductName)
            .ToListAsync();

        var soldData = await db.BillDetails
            .Where(x => x.Bill != null
                        && x.Bill.BillType == "EXPORT"
                        && !x.Bill.IsDeleted
                        && x.Bill.BillDate >= fromDate)
            .GroupBy(x => x.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                SoldQuantity = g.Count()
            })
            .ToListAsync();

        var results = new List<StockForecastResultDto>();

        foreach (var product in products)
        {
            var totalSold = soldData.FirstOrDefault(x => x.ProductId == product.Id)?.SoldQuantity ?? 0;
            var avgDailySales = request.AnalysisDays > 0 ? (decimal)totalSold / request.AnalysisDays : 0;
            var safetyStock = avgDailySales * request.SafetyDays;
            var predictedMinStock = avgDailySales > 0
                ? (int)Math.Ceiling(avgDailySales * request.LeadTimeDays + safetyStock)
                : product.MinStock;

            var daysUntilOut = avgDailySales > 0
                ? product.CurrentStock / avgDailySales
                : 999;

            var suggestedImport = product.CurrentStock < predictedMinStock
                ? predictedMinStock - product.CurrentStock
                : 0;

            var risk = GetRiskLevel(product.CurrentStock, predictedMinStock, daysUntilOut, request.LeadTimeDays);

            results.Add(new StockForecastResultDto
            {
                ProductId = product.Id,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                CurrentStock = product.CurrentStock,
                CurrentMinStock = product.MinStock,
                TotalSoldQuantity = totalSold,
                AnalysisDays = request.AnalysisDays,
                LeadTimeDays = request.LeadTimeDays,
                SafetyDays = request.SafetyDays,
                AverageDailySales = Math.Round(avgDailySales, 2),
                SafetyStock = Math.Round(safetyStock, 2),
                PredictedMinStock = predictedMinStock,
                DaysUntilOutOfStock = Math.Round(daysUntilOut, 1),
                SuggestedImportQuantity = suggestedImport,
                RiskLevel = risk,
                RiskLevelName = GetRiskText(risk)
            });
        }

        return results.OrderByDescending(x => x.RiskLevel).ThenByDescending(x => x.SuggestedImportQuantity).ToList();
    }

    private static RiskLevel GetRiskLevel(int currentStock, int predictedMinStock, decimal daysUntilOut, int leadTimeDays)
    {
        if (currentStock <= 0)
            return RiskLevel.HetHang;

        if (daysUntilOut <= leadTimeDays)
            return RiskLevel.SapHetHang;

        if (currentStock <= predictedMinStock)
            return RiskLevel.CanTheoDoi;

        return RiskLevel.BinhThuong;
    }

    private static string GetRiskText(RiskLevel riskLevel)
    {
        return riskLevel switch
        {
            RiskLevel.HetHang => RiskLevelText.HetHang,
            RiskLevel.SapHetHang => RiskLevelText.SapHetHang,
            RiskLevel.CanTheoDoi => RiskLevelText.CanTheoDoi,
            _ => RiskLevelText.BinhThuong
        };
    }
}