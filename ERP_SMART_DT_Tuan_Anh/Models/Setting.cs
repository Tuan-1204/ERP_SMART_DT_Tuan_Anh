using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP_SMART_DT_Tuan_Anh.Models
{
    public class Setting
    {
        public int Id { get; set; }
        public string? ShopName { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public byte[]? Logo { get; set; }
        public string Theme { get; set; } = "Light";
    }
}
