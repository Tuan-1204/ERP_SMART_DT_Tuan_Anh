using ERP_SMART_DT_Tuan_Anh.Data;
using ERP_SMART_DT_Tuan_Anh.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_SMART_DT_Tuan_Anh.Services;

public class PartnerService
{
    public async Task<List<Partner>> GetCustomersAsync()
    {
        return await GetByTypeAsync("CUSTOMER");
    }

    public async Task<List<Partner>> GetSuppliersAsync()
    {
        return await GetByTypeAsync("SUPPLIER");
    }

    public async Task<List<Partner>> GetByTypeAsync(string objectType)
    {
        await using var db = DbContextFactory.Create();

        return await db.Partners
            .Where(x => x.ObjectType == objectType && !x.IsDeleted)
            .OrderBy(x => x.FullName)
            .ToListAsync();
    }

    public async Task AddAsync(Partner partner)
    {
        await using var db = DbContextFactory.Create();

        partner.CreatedDate = DateTime.Now;
        db.Partners.Add(partner);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Partner partner)
    {
        await using var db = DbContextFactory.Create();

        var entity = await db.Partners.FirstOrDefaultAsync(x => x.Id == partner.Id);
        if (entity == null)
            return;

        entity.FullName = partner.FullName;
        entity.Phone = partner.Phone;
        entity.Address = partner.Address;
        entity.Email = partner.Email;
        entity.TaxCode = partner.TaxCode;
        entity.UpdatedDate = DateTime.Now;

        await db.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        await using var db = DbContextFactory.Create();

        var entity = await db.Partners.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
            return;

        entity.IsDeleted = true;
        await db.SaveChangesAsync();
    }
}