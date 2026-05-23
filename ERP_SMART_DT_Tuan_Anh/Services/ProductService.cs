using ERP_SMART_DT_Tuan_Anh.Data;
using ERP_SMART_DT_Tuan_Anh.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_SMART_DT_Tuan_Anh.Services;

public class ProductService
{
    public async Task<List<Product>> GetAllAsync()
    {
        await using var db = DbContextFactory.Create();

        return await db.Products
            .Include(x => x.Category)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.ProductName)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        await using var db = DbContextFactory.Create();

        return await db.Products
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    }

    public async Task<Product?> GetByCodeAsync(string productCode)
    {
        await using var db = DbContextFactory.Create();

        productCode = productCode.Trim();

        return await db.Products
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.ProductCode == productCode && !x.IsDeleted);
    }

    public async Task<bool> IsProductCodeExistsAsync(string productCode, int ignoreProductId = 0)
    {
        await using var db = DbContextFactory.Create();

        productCode = productCode.Trim();

        return await db.Products.AnyAsync(x =>
            x.ProductCode == productCode &&
            x.Id != ignoreProductId &&
            !x.IsDeleted);
    }

    public async Task AddAsync(Product product)
    {
        await using var db = DbContextFactory.Create();

        product.CreatedDate = DateTime.Now;
        product.IsDeleted = false;

        db.Products.Add(product);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        await using var db = DbContextFactory.Create();

        var entity = await db.Products.FirstOrDefaultAsync(x => x.Id == product.Id);
        if (entity == null)
            return;

        entity.CategoryId = product.CategoryId;
        entity.ProductCode = product.ProductCode;
        entity.ProductName = product.ProductName;
        entity.ImportPrice = product.ImportPrice;
        entity.ExportPrice = product.ExportPrice;
        entity.MinStock = product.MinStock;
        entity.AlertThreshold = product.AlertThreshold;
        entity.Unit = product.Unit;
        entity.ProductImage = product.ProductImage;
        entity.UpdatedDate = DateTime.Now;
        entity.UpdatedBy = product.UpdatedBy;

        await db.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        await using var db = DbContextFactory.Create();

        var entity = await db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
            return;

        entity.IsDeleted = true;
        entity.UpdatedDate = DateTime.Now;

        await db.SaveChangesAsync();
    }

    public async Task<string> HardDeleteAsync(int id)
    {
        await using var db = DbContextFactory.Create();

        var entity = await db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
            return "Không tìm thấy sản phẩm cần xóa.";

        var hasImei = await db.ImeiInventories.AnyAsync(x => x.ProductId == id);
        var hasBillDetail = await db.BillDetails.AnyAsync(x => x.ProductId == id);
        var hasInventoryCheck = await db.InventoryCheckDetails.AnyAsync(x => x.ProductId == id);

        if (hasImei || hasBillDetail || hasInventoryCheck)
        {
            return "Không thể xóa hẳn sản phẩm này vì đã có IMEI, hóa đơn hoặc kiểm kê liên quan. Để bảo toàn lịch sử kho, chỉ nên xóa sản phẩm chưa phát sinh giao dịch.";
        }

        db.Products.Remove(entity);
        await db.SaveChangesAsync();

        return "SUCCESS";
    }
}
