using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP_SMART_DT_Tuan_Anh.Models
{
    public class DebtTransaction
    {
        public int Id { get; set; }
        public int? ObjectId { get; set; }
        public string? BillId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal? Amount { get; set; }
        public string? Type { get; set; }
        public string? Note { get; set; }

        public Partner? Object { get; set; }
        public Bill? Bill { get; set; }
    }
}
