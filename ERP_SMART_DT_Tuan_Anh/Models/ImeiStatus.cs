using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP_SMART_DT_Tuan_Anh.Models
{
    public class ImeiStatus
    {
        public int Id { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? ColorCode { get; set; }

        public ICollection<ImeiInventory> ImeiInventories { get; set; } = new List<ImeiInventory>();
    }
}
