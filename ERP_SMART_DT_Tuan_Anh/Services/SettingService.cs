using ERP_SMART_DT_Tuan_Anh.Data;
using ERP_SMART_DT_Tuan_Anh.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_SMART_DT_Tuan_Anh.Services;

public class SettingService
{
    public async Task<Setting?> GetAsync()
    {
        await using var db = DbContextFactory.Create();

        return await db.Settings.FirstOrDefaultAsync();
    }

    public async Task SaveAsync(Setting setting)
    {
        await using var db = DbContextFactory.Create();

        var entity = await db.Settings.FirstOrDefaultAsync();

        if (entity == null)
        {
            db.Settings.Add(setting);
        }
        else
        {
            entity.ShopName = setting.ShopName;
            entity.Address = setting.Address;
            entity.Phone = setting.Phone;
            entity.Logo = setting.Logo;
            entity.Theme = setting.Theme;
        }

        await db.SaveChangesAsync();
    }
}