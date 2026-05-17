using Microsoft.EntityFrameworkCore;

namespace ERP_SMART_DT_Tuan_Anh.Data;

public static class DbContextFactory
{
    private const string DefaultConnectionString =
    "Server=DESKTOP-4655K0P\\SQLEXPRESS;Database=ERP_Quan_Ly_Kho_Thong_Minh_DB;Trusted_Connection=True;TrustServerCertificate=True;";

    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(DefaultConnectionString)
            .Options;

        return new AppDbContext(options);
    }
}