using ERP_SMART_DT_Tuan_Anh.Data;
using ERP_SMART_DT_Tuan_Anh.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ERP_SMART_DT_Tuan_Anh.Services;

public class DebtService
{
    public async Task<List<Models.DebtTransaction>> GetDebtTransactionsAsync(int objectId)
    {
        await using var db = DbContextFactory.Create();

        return await db.DebtTransactions
            .Where(x => x.ObjectId == objectId)
            .OrderByDescending(x => x.TransactionDate)
            .ToListAsync();
    }

    public async Task<string> PayDebtAsync(DebtPaymentRequestDto request)
    {
        await using var db = DbContextFactory.Create();

        var result = await db.Database.SqlQueryRaw<string>(
            $"""
            EXEC {StoredProcedureNames.PayDebt}
                @ObjectId,
                @Amount,
                @Note
            """,
            new SqlParameter("@ObjectId", request.ObjectId),
            new SqlParameter("@Amount", request.Amount),
            new SqlParameter("@Note", request.Note ?? string.Empty)
        ).ToListAsync();

        return result.FirstOrDefault() ?? "SUCCESS";
    }

    // Backwards-compatible wrapper used by ViewModels
    public async Task<string> ExecutePayDebtAsync(int objectId, decimal amount, string? note)
    {
        var dto = new DebtPaymentRequestDto
        {
            ObjectId = objectId,
            Amount = amount,
            Note = note
        };

        return await PayDebtAsync(dto);
    }
}