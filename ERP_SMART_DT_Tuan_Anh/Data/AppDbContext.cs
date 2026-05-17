using ERP_SMART_DT_Tuan_Anh.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_SMART_DT_Tuan_Anh.Data;

public class AppDbContext : DbContext
{
    private const string DefaultConnectionString =
    "Server=DESKTOP-4655K0P\\SQLEXPRESS;Database=ERP_Quan_Ly_Kho_Thong_Minh_DB;Trusted_Connection=True;TrustServerCertificate=True;";
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ImeiStatus> ImeiStatuses => Set<ImeiStatus>();
    public DbSet<ImeiInventory> ImeiInventories => Set<ImeiInventory>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<BillDetail> BillDetails => Set<BillDetail>();
    public DbSet<InventoryCheck> InventoryChecks => Set<InventoryCheck>();
    public DbSet<InventoryCheckDetail> InventoryCheckDetails => Set<InventoryCheckDetail>();
    public DbSet<DebtTransaction> DebtTransactions => Set<DebtTransaction>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<WarrantyLog> WarrantyLogs => Set<WarrantyLog>();
    public DbSet<Return> Returns => Set<Return>();
    public DbSet<Setting> Settings => Set<Setting>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(DefaultConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureRoles(modelBuilder);
        ConfigureUsers(modelBuilder);
        ConfigureCategories(modelBuilder);
        ConfigureProducts(modelBuilder);
        ConfigureImeiStatuses(modelBuilder);
        ConfigureImeiInventories(modelBuilder);
        ConfigurePartners(modelBuilder);
        ConfigureBills(modelBuilder);
        ConfigureBillDetails(modelBuilder);
        ConfigureInventoryChecks(modelBuilder);
        ConfigureInventoryCheckDetails(modelBuilder);
        ConfigureDebtTransactions(modelBuilder);
        ConfigureAuditLogs(modelBuilder);
        ConfigureWarrantyLogs(modelBuilder);
        ConfigureReturns(modelBuilder);
        ConfigureSettings(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private static void ConfigureRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.RoleName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Username).IsUnique();

            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.Role)
                .WithMany(e => e.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.CategoryName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
    }

    private static void ConfigureProducts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products", table =>
            {
                table.HasCheckConstraint("CHK_Product_Price", "ImportPrice >= 0 AND ExportPrice >= 0");
                table.HasCheckConstraint("CHK_Product_Stock", "CurrentStock >= 0");
            });
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.ProductName).HasDatabaseName("IX_Product_Name");
            entity.HasIndex(e => e.ProductCode).IsUnique();

            entity.Property(e => e.ProductCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ImportPrice).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            entity.Property(e => e.ExportPrice).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            entity.Property(e => e.CurrentStock).HasDefaultValue(0);
            entity.Property(e => e.MinStock).HasDefaultValue(5);
            entity.Property(e => e.AlertThreshold).HasDefaultValue(10);
            entity.Property(e => e.Unit).HasMaxLength(50).HasDefaultValue("Chiếc");
            entity.Property(e => e.ProductImage).HasColumnType("varbinary(max)");
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.Category)
                .WithMany(e => e.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureImeiStatuses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ImeiStatus>(entity =>
        {
            entity.ToTable("ImeiStatuses");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.StatusName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ColorCode).HasMaxLength(10);
        });
    }

    private static void ConfigureImeiInventories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ImeiInventory>(entity =>
        {
            entity.ToTable("ImeiInventories", table =>
            {
                table.HasCheckConstraint("CHK_Imei_Length", "LEN(Imei) >= 10");
            });
            entity.HasKey(e => e.Imei);

            entity.HasIndex(e => e.StatusId).HasDatabaseName("IX_Imei_Status");

            entity.Property(e => e.Imei).HasMaxLength(20).IsRequired();
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.Product)
                .WithMany(e => e.ImeiInventories)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Status)
                .WithMany(e => e.ImeiInventories)
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePartners(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Partner>(entity =>
        {
            entity.ToTable("Objects");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ObjectType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.TaxCode).HasMaxLength(50);
            entity.Property(e => e.TotalDebt).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
    }

    private static void ConfigureBills(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bill>(entity =>
        {
            entity.ToTable("Bills");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.BillDate).HasDatabaseName("IX_Bill_Date");

            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.BillType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            entity.Property(e => e.RemainingAmount)
                .HasColumnType("decimal(18,2)")
                .HasComputedColumnSql("(TotalAmount - PaidAmount)", stored: false);
            entity.Property(e => e.BillDate).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.Note).HasColumnType("nvarchar(max)");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.Object)
                .WithMany(e => e.Bills)
                .HasForeignKey(e => e.ObjectId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany(e => e.Bills)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureBillDetails(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BillDetail>(entity =>
        {
            entity.ToTable("BillDetails");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.BillId).HasMaxLength(50);
            entity.Property(e => e.Imei).HasMaxLength(20);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(e => e.Bill)
                .WithMany(e => e.BillDetails)
                .HasForeignKey(e => e.BillId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Product)
                .WithMany(e => e.BillDetails)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ImeiInventory)
                .WithMany(e => e.BillDetails)
                .HasForeignKey(e => e.Imei)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureInventoryChecks(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryCheck>(entity =>
        {
            entity.ToTable("InventoryChecks");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.CheckDate).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.Note).HasColumnType("nvarchar(max)");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.User)
                .WithMany(e => e.InventoryChecks)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureInventoryCheckDetails(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryCheckDetail>(entity =>
        {
            entity.ToTable("InventoryCheckDetails");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Difference)
                .HasComputedColumnSql("(ActualStock - SystemStock)", stored: false);

            entity.HasOne(e => e.InventoryCheck)
                .WithMany(e => e.InventoryCheckDetails)
                .HasForeignKey(e => e.CheckId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Product)
                .WithMany(e => e.InventoryCheckDetails)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureDebtTransactions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DebtTransaction>(entity =>
        {
            entity.ToTable("DebtTransactions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.BillId).HasMaxLength(50);
            entity.Property(e => e.TransactionDate).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Type).HasMaxLength(20);
            entity.Property(e => e.Note).HasColumnType("nvarchar(max)");

            entity.HasOne(e => e.Object)
                .WithMany(e => e.DebtTransactions)
                .HasForeignKey(e => e.ObjectId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Bill)
                .WithMany(e => e.DebtTransactions)
                .HasForeignKey(e => e.BillId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAuditLogs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.TableName).HasMaxLength(50);
            entity.Property(e => e.RecordId).HasMaxLength(50);
            entity.Property(e => e.OldData).HasColumnType("nvarchar(max)");
            entity.Property(e => e.NewData).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Timestamp).HasDefaultValueSql("GETDATE()");
        });
    }

    private static void ConfigureWarrantyLogs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WarrantyLog>(entity =>
        {
            entity.ToTable("WarrantyLogs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Imei).HasMaxLength(20);
            entity.Property(e => e.ReceiveDate).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Result).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Cost).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(e => e.ImeiInventory)
                .WithMany(e => e.WarrantyLogs)
                .HasForeignKey(e => e.Imei)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany(e => e.WarrantyLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureReturns(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Return>(entity =>
        {
            entity.ToTable("Returns");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.BillId).HasMaxLength(50);
            entity.Property(e => e.ReturnType).HasMaxLength(20);
            entity.Property(e => e.Reason).HasColumnType("nvarchar(max)");
            entity.Property(e => e.TotalRefund).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");

            entity.HasOne(e => e.Bill)
                .WithMany(e => e.Returns)
                .HasForeignKey(e => e.BillId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSettings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Setting>(entity =>
        {
            entity.ToTable("Settings");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ShopName).HasMaxLength(200);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Logo).HasColumnType("varbinary(max)");
            entity.Property(e => e.Theme).HasMaxLength(20).HasDefaultValue("Light");
        });
    }
}
