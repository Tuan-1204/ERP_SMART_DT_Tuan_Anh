using ERP_SMART_DT_Tuan_Anh.Data;
using ERP_SMART_DT_Tuan_Anh.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_SMART_DT_Tuan_Anh.Services;

public class InventoryCheckService
{
    public async Task<List<InventoryCheck>> GetAllAsync()
    {
        await using var db = DbContextFactory.Create();

        return await db.InventoryChecks
            .Include(x => x.User)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CheckDate)
            .ToListAsync();
    }

    public async Task AddAsync(InventoryCheck check)
    {
        await using var db = DbContextFactory.Create();

        check.CheckDate = DateTime.Now;
        db.InventoryChecks.Add(check);
        await db.SaveChangesAsync();
    }
}