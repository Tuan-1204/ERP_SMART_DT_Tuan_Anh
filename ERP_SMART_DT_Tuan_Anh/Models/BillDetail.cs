using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP_SMART_DT_Tuan_Anh.Models
{
    public class BillDetail
    {
        public int Id { get; set; }
        public string? BillId { get; set; }
        public int? ProductId { get; set; }
        public string? Imei { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public Bill? Bill { get; set; }
        public Product? Product { get; set; }
        public ImeiInventory? ImeiInventory { get; set; }
    }
}
