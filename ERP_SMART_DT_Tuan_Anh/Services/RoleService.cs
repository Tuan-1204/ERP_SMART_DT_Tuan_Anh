using ERP_SMART_DT_Tuan_Anh.Data;
using ERP_SMART_DT_Tuan_Anh.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_SMART_DT_Tuan_Anh.Services;

public class RoleService
{
    public async Task<List<Role>> GetAllAsync()
    {
        await using var db = DbContextFactory.Create();

        return await db.Roles
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.RoleName)
            .ToListAsync();
    }

    public async Task AddAsync(Role role)
    {
        await using var db = DbContextFactory.Create();

        role.CreatedDate = DateTime.Now;
        db.Roles.Add(role);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Role role)
    {
        await using var db = DbContextFactory.Create();

        var entity = await db.Roles.FirstOrDefaultAsync(x => x.Id == role.Id);
        if (entity == null)
            return;

        entity.RoleName = role.RoleName;
        entity.Description = role.Description;
        entity.UpdatedDate = DateTime.Now;

        await db.SaveChangesAsync();
    }
}