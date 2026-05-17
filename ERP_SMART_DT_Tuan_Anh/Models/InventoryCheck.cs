using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP_SMART_DT_Tuan_Anh.Models
{
    public class InventoryCheck
    {
        public int Id { get; set; }
        public DateTime CheckDate { get; set; }
        public int? UserId { get; set; }
        public string? Note { get; set; }
        public bool IsDeleted { get; set; }

        public User? User { get; set; }
        public ICollection<InventoryCheckDetail> InventoryCheckDetails { get; set; } = new List<InventoryCheckDetail>();
    }
}
