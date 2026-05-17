using ERP_SMART_DT_Tuan_Anh.Data;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_SMART_DT_Tuan_Anh.Services;

public class UserService
{
    public async Task<List<User>> GetAllAsync()
    {
        await using var db = DbContextFactory.Create();

        return await db.Users
            .Include(x => x.Role)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Username)
            .ToListAsync();
    }

    public async Task AddAsync(User user, string rawPassword)
    {
        await using var db = DbContextFactory.Create();

        user.PasswordHash = PasswordHasher.Hash(rawPassword);
        user.CreatedDate = DateTime.Now;
        user.IsActive = true;
        user.IsDeleted = false;

        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        await using var db = DbContextFactory.Create();

        var entity = await db.Users.FirstOrDefaultAsync(x => x.Id == user.Id);
        if (entity == null)
            return;

        entity.FullName = user.FullName;
        entity.Email = user.Email;
        entity.Phone = user.Phone;
        entity.RoleId = user.RoleId;
        entity.IsActive = user.IsActive;
        entity.UpdatedDate = DateTime.Now;

        await db.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        await using var db = DbContextFactory.Create();

        var entity = await db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
            return;

        entity.IsDeleted = true;
        await db.SaveChangesAsync();
    }
}