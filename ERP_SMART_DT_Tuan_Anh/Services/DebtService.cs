using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERP_SMART_DT_Tuan_Anh.Data;
using ERP_SMART_DT_Tuan_Anh.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ERP_SMART_DT_Tuan_Anh.Services
{
    public class DebtService
    {
        // Lấy danh sách lịch sử giao dịch công nợ theo ObjectId đối tác
        public async Task<List<Models.DebtTransaction>> GetDebtTransactionsAsync(int objectId)
        {
            await using var db = DbContextFactory.Create();

            return await db.DebtTransactions
                .Where(x => x.ObjectId == objectId)
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();
        }

        // Wrapper tương thích ngược được gọi trực tiếp từ DebtViewModel
        public async Task<List<Models.DebtTransaction>> GetTransactionsByPartnerAsync(int objectId)
        {
            return await GetDebtTransactionsAsync(objectId);
        }

        // Hàm xử lý lưu phiếu thu/chi trả nợ thông qua Stored Procedure 
        public async Task<string> PayDebtAsync(DebtPaymentRequestDto request)
        {
            await using var db = DbContextFactory.Create();

            var parameters = new[]
            {
                new SqlParameter("@ObjectId", request.ObjectId),
                new SqlParameter("@Amount", request.Amount),
                new SqlParameter("@Note", request.Note ?? (object)DBNull.Value)
            };

           
            var result = await db.Database.SqlQueryRaw<string>(
                $"EXEC {StoredProcedureNames.PayDebt} @ObjectId, @Amount, @Note",
                parameters
            ).ToListAsync();

            return result.FirstOrDefault() ?? "SUCCESS";
        }

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
}