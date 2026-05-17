using ERP_SMART_DT_Tuan_Anh.Data;
using ERP_SMART_DT_Tuan_Anh.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_SMART_DT_Tuan_Anh.Services;

public class BillService
{
    public async Task<List<Bill>> GetAllAsync()
    {
        await using var db = DbContextFactory.Create();

        return await db.Bills
            .Include(x => x.Object)
            .Include(x => x.User)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.BillDate)
            .ToListAsync();
    }

    public async Task<Bill?> GetByIdAsync(string id)
    {
        await using var db = DbContextFactory.Create();

        return await db.Bills
            .Include(x => x.Object)
            .Include(x => x.User)
            .Include(x => x.BillDetails)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    }

    public async Task SoftDeleteAsync(string id)
    {
        await using var db = DbContextFactory.Create();

        var bill = await db.Bills.FirstOrDefaultAsync(x => x.Id == id);
        if (bill == null)
            return;

        bill.IsDeleted = true;
        await db.SaveChangesAsync();
    }
}