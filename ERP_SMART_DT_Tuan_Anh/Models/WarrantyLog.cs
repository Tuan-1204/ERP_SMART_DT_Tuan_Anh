using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP_SMART_DT_Tuan_Anh.Models
{
    public class WarrantyLog
    {
        public int Id { get; set; }
        public string? Imei { get; set; }
        public int? UserId { get; set; }
        public DateTime ReceiveDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string? Description { get; set; }
        public string? Result { get; set; }
        public decimal Cost { get; set; }
        public string? Status { get; set; }

        public ImeiInventory? ImeiInventory { get; set; }
        public User? User { get; set; }
    }
}
