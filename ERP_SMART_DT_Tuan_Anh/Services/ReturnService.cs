using ERP_SMART_DT_Tuan_Anh.Data;
using Microsoft.EntityFrameworkCore;
using ReturnModel = ERP_SMART_DT_Tuan_Anh.Models.Return;

namespace ERP_SMART_DT_Tuan_Anh.Services;

public class ReturnService
{
    public async Task<List<ReturnModel>> GetAllAsync()
    {
        await using var db = DbContextFactory.Create();

        return await db.Returns
            .Include(x => x.Bill)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync();
    }

    public async Task AddAsync(ReturnModel item)
    {
        await using var db = DbContextFactory.Create();

        item.CreatedDate = DateTime.Now;
        db.Returns.Add(item);
        await db.SaveChangesAsync();
    }
}