using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP_SMART_DT_Tuan_Anh.Models
{
    public class ImeiInventory
    {
        public string Imei { get; set; } = string.Empty;
        public int? ProductId { get; set; }
        public int? StatusId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }

        public Product? Product { get; set; }
        public ImeiStatus? Status { get; set; }
        public ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();
        public ICollection<WarrantyLog> WarrantyLogs { get; set; } = new List<WarrantyLog>();
    }
}
