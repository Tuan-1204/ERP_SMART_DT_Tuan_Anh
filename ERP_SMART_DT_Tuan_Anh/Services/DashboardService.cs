using ERP_SMART_DT_Tuan_Anh.Data;
using ERP_SMART_DT_Tuan_Anh.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ERP_SMART_DT_Tuan_Anh.Services;

public class DashboardService
{
    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        await using var db = DbContextFactory.Create();

        var totalRevenue = await db.Bills
            .Where(x => x.BillType == "EXPORT" && !x.IsDeleted)
            .SumAsync(x => (decimal?)x.TotalAmount) ?? 0;

        var totalImport = await db.Bills
            .Where(x => x.BillType == "IMPORT" && !x.IsDeleted)
            .SumAsync(x => (decimal?)x.TotalAmount) ?? 0;

        var totalStock = await db.ImeiInventories
            .CountAsync(x => x.StatusId == 1 && !x.IsDeleted);

        var totalDebtToSupplier = await db.Partners
            .Where(x => x.ObjectType == "SUPPLIER" && !x.IsDeleted)
            .SumAsync(x => (decimal?)x.TotalDebt) ?? 0;

        var topSellingProducts = await db.BillDetails
            .Where(x => x.Bill != null && x.Bill.BillType == "EXPORT" && !x.Bill.IsDeleted)
            .GroupBy(x => new { x.ProductId, x.Product!.ProductName })
            .Select(g => new TopSellingProductDto
            {
                ProductName = g.Key.ProductName,
                SoldCount = g.Count(),
                TotalAmount = g.Sum(x => x.UnitPrice)
            })
            .OrderByDescending(x => x.SoldCount)
            .Take(5)
            .ToListAsync();

        var alerts = await db.Products
            .Where(x => !x.IsDeleted && x.CurrentStock <= x.MinStock)
            .Select(x => new InventoryAlertDto
            {
                ProductCode = x.ProductCode,
                ProductName = x.ProductName,
                CurrentStock = x.CurrentStock,
                MinStock = x.MinStock,
                MissingQuantity = x.MinStock - x.CurrentStock
            })
            .ToListAsync();

        return new DashboardSummaryDto
        {
            TotalRevenue = totalRevenue,
            TotalImport = totalImport,
            TotalStock = totalStock,
            TotalDebtToSupplier = totalDebtToSupplier,
            LowStockCount = alerts.Count,
            TotalProduct = await db.Products.CountAsync(x => !x.IsDeleted),
            TotalCustomer = await db.Partners.CountAsync(x => x.ObjectType == "CUSTOMER" && !x.IsDeleted),
            TotalSupplier = await db.Partners.CountAsync(x => x.ObjectType == "SUPPLIER" && !x.IsDeleted),
            TopSellingProducts = topSellingProducts,
            InventoryAlerts = alerts
        };
    }
}