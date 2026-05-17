using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP_SMART_DT_Tuan_Anh.Models
{
    public class Return
    {
        public int Id { get; set; }
        public string? BillId { get; set; }
        public string? ReturnType { get; set; }
        public string? Reason { get; set; }
        public decimal? TotalRefund { get; set; }
        public DateTime CreatedDate { get; set; }

        public Bill? Bill { get; set; }
    }
}
