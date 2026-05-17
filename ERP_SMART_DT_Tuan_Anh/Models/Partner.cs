using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP_SMART_DT_Tuan_Anh.Models
{
    public class Partner
    {
        public int Id { get; set; }
        public string ObjectType { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? TaxCode { get; set; }
        public decimal TotalDebt { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<Bill> Bills { get; set; } = new List<Bill>();
        public ICollection<DebtTransaction> DebtTransactions { get; set; } = new List<DebtTransaction>();
    }
}
