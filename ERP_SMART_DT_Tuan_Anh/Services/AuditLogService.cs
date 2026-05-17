using ERP_SMART_DT_Tuan_Anh.Data;
using ERP_SMART_DT_Tuan_Anh.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_SMART_DT_Tuan_Anh.Services;

public class AuditLogService
{
    public async Task<List<AuditLog>> GetAllAsync()
    {
        await using var db = DbContextFactory.Create();

        return await db.AuditLogs
            .OrderByDescending(x => x.Timestamp)
            .Take(300)
            .ToListAsync();
    }
}