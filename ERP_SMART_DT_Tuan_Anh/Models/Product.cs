using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP_SMART_DT_Tuan_Anh.Models
{
    public class Product
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal ImportPrice { get; set; }
        public decimal ExportPrice { get; set; }
        public int CurrentStock { get; set; }
        public int MinStock { get; set; }
        public int AlertThreshold { get; set; }
        public string Unit { get; set; } = "Chiếc";
        public byte[]? ProductImage { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }

        public Category? Category { get; set; }
        public ICollection<ImeiInventory> ImeiInventories { get; set; } = new List<ImeiInventory>();
        public ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();
        public ICollection<InventoryCheckDetail> InventoryCheckDetails { get; set; } = new List<InventoryCheckDetail>();
    }
}
