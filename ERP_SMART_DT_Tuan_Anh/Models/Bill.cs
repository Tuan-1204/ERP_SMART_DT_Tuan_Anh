using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP_SMART_DT_Tuan_Anh.Models
{
    public class Bill
    {
        public string Id { get; set; } = string.Empty;
        public string BillType { get; set; } = string.Empty;
        public int? ObjectId { get; set; }
        public int? UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; private set; }
        public DateTime BillDate { get; set; }
        public string? Note { get; set; }
        public bool IsDeleted { get; set; }

        public Partner? Object { get; set; }
        public User? User { get; set; }
        public ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();
        public ICollection<DebtTransaction> DebtTransactions { get; set; } = new List<DebtTransaction>();
        public ICollection<Return> Returns { get; set; } = new List<Return>();
    }
}
