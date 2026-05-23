using ERP_SMART_DT_Tuan_Anh.Data;
using ERP_SMART_DT_Tuan_Anh.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_SMART_DT_Tuan_Anh.Services;

public class ImeiService
{
    public async Task<List<ImeiInventory>> GetAllAsync()
    {
        await using var db = DbContextFactory.Create();

        return await db.ImeiInventories
            .Include(x => x.Product)
            .Include(x => x.Status)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync();
    }

    public async Task<List<ImeiInventory>> GetByProductAsync(int productId)
    {
        await using var db = DbContextFactory.Create();

        return await db.ImeiInventories
            .Include(x => x.Status)
            .Where(x => x.ProductId == productId && !x.IsDeleted)
            .ToListAsync();
    }

    public async Task<ImeiInventory?> GetByImeiAsync(string imei)
    {
        await using var db = DbContextFactory.Create();

        imei = imei.Trim();

        return await db.ImeiInventories
            .Include(x => x.Product)
            .Include(x => x.Status)
            .FirstOrDefaultAsync(x => x.Imei == imei && !x.IsDeleted);
    }

    public async Task<List<ImeiInventory>> GetByImeisAsync(IEnumerable<string> imeis)
    {
        await using var db = DbContextFactory.Create();

        var imeiList = imeis
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        return await db.ImeiInventories
            .Include(x => x.Product)
            .Include(x => x.Status)
            .Where(x => imeiList.Contains(x.Imei) && !x.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<string>> GetExistingImeisAsync(IEnumerable<string> imeis)
    {
        await using var db = DbContextFactory.Create();

        var imeiList = imeis
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        return await db.ImeiInventories
            .Where(x => imeiList.Contains(x.Imei) && !x.IsDeleted)
            .Select(x => x.Imei)
            .ToListAsync();
    }
}
