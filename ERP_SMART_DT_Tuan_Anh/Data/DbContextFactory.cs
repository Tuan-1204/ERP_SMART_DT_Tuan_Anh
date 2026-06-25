using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ERP_SMART_DT_Tuan_Anh.Data;

public static class DbContextFactory
{
    private const string DefaultConnectionString =
        "Server=.\\SQLEXPRESS;Database=ERP_Quan_Ly_Kho_Thong_Minh_DB;Trusted_Connection=True;TrustServerCertificate=True;";

    public static AppDbContext Create()
    {
        var connectionString = GetConnectionString();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    private static string GetConnectionString()
    {
        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();

            var configuredConnection = configuration.GetConnectionString("DefaultConnection");
            return string.IsNullOrWhiteSpace(configuredConnection)
                ? DefaultConnectionString
                : configuredConnection;
        }
        catch
        {
            return DefaultConnectionString;
        }
    }
}
