using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP_SMART_DT_Tuan_Anh.Models
{
    public class InventoryCheckDetail
    {
        public int Id { get; set; }
        public int? CheckId { get; set; }
        public int? ProductId { get; set; }
        public int? SystemStock { get; set; }
        public int? ActualStock { get; set; }
        public int? Difference { get; private set; }

        public InventoryCheck? InventoryCheck { get; set; }
        public Product? Product { get; set; }
    }
}
