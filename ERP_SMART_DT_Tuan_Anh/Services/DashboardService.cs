using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient; // Thư viện kết nối SQL Server chuẩn của .NET Core / Entity Framework
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Dapper;
using ERP_SMART_DT_Tuan_Anh.DTOs;

namespace ERP_SMART_DT_Tuan_Anh.Services
{
    public class DashboardService
    {
        private readonly string _connectionString;

        public DashboardService()
        {
            // Đọc cấu hình chuỗi kết nối từ file appsettings.json theo đúng thiết lập dự án
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Không tìm thấy chuỗi kết nối 'DefaultConnection' trong appsettings.json.");
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            var summary = new DashboardSummaryDto();

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    if (db.State == ConnectionState.Closed)
                        db.Open();

                    // Thực thi Procedure sp_GetDashboardSummary bóc tách đồng thời 3 tập kết quả dữ liệu
                    using (var multi = await db.QueryMultipleAsync("sp_GetDashboardSummary", commandType: CommandType.StoredProcedure))
                    {
                        // 1. Đọc Result Set 1: Đọc tập hợp 8 chỉ số hệ thống cốt lõi
                        var mainStats = await multi.ReadSingleOrDefaultAsync<dynamic>();
                        if (mainStats != null)
                        {
                            summary.TotalRevenue = (decimal)(mainStats.TotalRevenue ?? 0);
                            summary.TotalImport = (decimal)(mainStats.TotalImport ?? 0);
                            summary.TotalStock = (int)(mainStats.TotalStock ?? 0);
                            summary.TotalDebtToSupplier = (decimal)(mainStats.TotalDebtToSupplier ?? 0);
                            summary.LowStockCount = (int)(mainStats.LowStockCount ?? 0);
                            summary.TotalProduct = (int)(mainStats.TotalProduct ?? 0);
                            summary.TotalCustomer = (int)(mainStats.TotalCustomer ?? 0);
                            summary.TotalSupplier = (int)(mainStats.TotalSupplier ?? 0);
                        }

                        // 2. Đọc Result Set 2: Danh sách 5 sản phẩm bán chạy nhất hệ thống
                        var topSelling = await multi.ReadAsync<TopSellingProductDto>();
                        summary.TopSellingProducts = topSelling.ToList();

                        // 3. Đọc Result Set 3: Danh sách sản phẩm chạm/dưới mức tồn kho tối thiểu
                        var alerts = await multi.ReadAsync<InventoryAlertDto>();
                        summary.InventoryAlerts = alerts.ToList();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi hệ thống khi bóc tách dữ liệu thống kê từ SQL Server.", ex);
                }
            }

            return summary;
        }
    }
}