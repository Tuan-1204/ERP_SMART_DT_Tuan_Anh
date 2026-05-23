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
            .Include(x => x.InventoryCheckDetails)
                .ThenInclude(x => x.Product)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CheckDate)
            .ToListAsync();
    }

    public async Task AddWithDetailsAsync(InventoryCheck check, IEnumerable<InventoryCheckDetail> details)
    {
        await using var db = DbContextFactory.Create();
        await using var transaction = await db.Database.BeginTransactionAsync();

        check.CheckDate = DateTime.Now;
        check.IsDeleted = false;

        db.InventoryChecks.Add(check);
        await db.SaveChangesAsync();

        foreach (var detail in details)
        {
            detail.CheckId = check.Id;
            db.InventoryCheckDetails.Add(detail);
        }

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task UpdateWithDetailsAsync(int checkId, string? note, IEnumerable<InventoryCheckDetail> details)
    {
        await using var db = DbContextFactory.Create();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var entity = await db.InventoryChecks
            .Include(x => x.InventoryCheckDetails)
            .FirstOrDefaultAsync(x => x.Id == checkId && !x.IsDeleted);

        if (entity == null)
            return;

        entity.Note = note;

        db.InventoryCheckDetails.RemoveRange(entity.InventoryCheckDetails);
        await db.SaveChangesAsync();

        foreach (var detail in details)
        {
            detail.CheckId = checkId;
            db.InventoryCheckDetails.Add(detail);
        }

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task HardDeleteAsync(int checkId)
    {
        await using var db = DbContextFactory.Create();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var entity = await db.InventoryChecks
            .Include(x => x.InventoryCheckDetails)
            .FirstOrDefaultAsync(x => x.Id == checkId);

        if (entity == null)
            return;

        db.InventoryCheckDetails.RemoveRange(entity.InventoryCheckDetails);
        db.InventoryChecks.Remove(entity);

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
