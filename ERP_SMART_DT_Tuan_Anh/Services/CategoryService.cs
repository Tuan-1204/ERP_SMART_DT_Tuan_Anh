using ERP_SMART_DT_Tuan_Anh.Data;
using ERP_SMART_DT_Tuan_Anh.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_SMART_DT_Tuan_Anh.Services;

public class CategoryService
{
    public async Task<List<Category>> GetAllAsync()
    {
        await using var db = DbContextFactory.Create();

        return await db.Categories
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.CategoryName)
            .ToListAsync();
    }

    public async Task AddAsync(Category category)
    {
        await using var db = DbContextFactory.Create();

        category.CreatedDate = DateTime.Now;
        db.Categories.Add(category);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        await using var db = DbContextFactory.Create();

        var entity = await db.Categories.FirstOrDefaultAsync(x => x.Id == category.Id);
        if (entity == null)
            return;

        entity.CategoryName = category.CategoryName;
        entity.Description = category.Description;
        entity.UpdatedDate = DateTime.Now;

        await db.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        await using var db = DbContextFactory.Create();

        var entity = await db.Categories.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
            return;

        entity.IsDeleted = true;
        await db.SaveChangesAsync();
    }
}