using ERP_SMART_DT_Tuan_Anh.Data;
using ERP_SMART_DT_Tuan_Anh.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_SMART_DT_Tuan_Anh.Services;

public class WarrantyService
{
    public async Task<List<WarrantyLog>> GetAllAsync()
    {
        await using var db = DbContextFactory.Create();

        return await db.WarrantyLogs
            .Include(x => x.ImeiInventory)
            .ThenInclude(x => x!.Product)
            .Include(x => x.User)
            .OrderByDescending(x => x.ReceiveDate)
            .ToListAsync();
    }

    public async Task AddAsync(WarrantyLog warranty)
    {
        await using var db = DbContextFactory.Create();

        warranty.ReceiveDate = DateTime.Now;
        db.WarrantyLogs.Add(warranty);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(WarrantyLog warranty)
    {
        await using var db = DbContextFactory.Create();

        var entity = await db.WarrantyLogs.FirstOrDefaultAsync(x => x.Id == warranty.Id);
        if (entity == null)
            return;

        entity.ReturnDate = warranty.ReturnDate;
        entity.Result = warranty.Result;
        entity.Cost = warranty.Cost;
        entity.Status = warranty.Status;

        await db.SaveChangesAsync();
    }
}