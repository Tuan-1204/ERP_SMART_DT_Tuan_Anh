using ERP_SMART_DT_Tuan_Anh.Data;
using ERP_SMART_DT_Tuan_Anh.DTOs;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ERP_SMART_DT_Tuan_Anh.Services;

public class StockService
{
    public async Task<string> ImportStockAsync(ImportStockRequestDto request)
    {
        if (request.ImeiList.Count == 0)
            return "Danh sách IMEI không được rỗng.";

        await using var db = DbContextFactory.Create();

        var result = await db.Database.SqlQueryRaw<string>(
            $"""
            EXEC {StoredProcedureNames.ImportStock}
                @BillId,
                @ObjectId,
                @UserId,
                @ProductId,
                @UnitPrice,
                @TotalAmount,
                @PaidAmount,
                @Note,
                @ImeiList
            """,
            new SqlParameter("@BillId", request.BillId),
            new SqlParameter("@ObjectId", request.ObjectId),
            new SqlParameter("@UserId", request.UserId),
            new SqlParameter("@ProductId", request.ProductId),
            new SqlParameter("@UnitPrice", request.UnitPrice),
            new SqlParameter("@TotalAmount", request.TotalAmount),
            new SqlParameter("@PaidAmount", request.PaidAmount),
            new SqlParameter("@Note", request.Note ?? string.Empty),
            new SqlParameter("@ImeiList", ImeiHelper.JoinImeiList(request.ImeiList))
        ).ToListAsync();

        return result.FirstOrDefault() ?? "SUCCESS";
    }

    public async Task<string> ExportStockAsync(ExportStockRequestDto request)
    {
        if (request.ImeiList.Count == 0)
            return "Danh sách IMEI không được rỗng.";

        await using var db = DbContextFactory.Create();

        var result = await db.Database.SqlQueryRaw<string>(
            $"""
            EXEC {StoredProcedureNames.ExportStock}
                @BillId,
                @ObjectId,
                @UserId,
                @ProductId,
                @UnitPrice,
                @TotalAmount,
                @PaidAmount,
                @ImeiList
            """,
            new SqlParameter("@BillId", request.BillId),
            new SqlParameter("@ObjectId", request.ObjectId),
            new SqlParameter("@UserId", request.UserId),
            new SqlParameter("@ProductId", request.ProductId),
            new SqlParameter("@UnitPrice", request.UnitPrice),
            new SqlParameter("@TotalAmount", request.TotalAmount),
            new SqlParameter("@PaidAmount", request.PaidAmount),
            new SqlParameter("@ImeiList", ImeiHelper.JoinImeiList(request.ImeiList))
        ).ToListAsync();

        return result.FirstOrDefault() ?? "SUCCESS";
    }
}