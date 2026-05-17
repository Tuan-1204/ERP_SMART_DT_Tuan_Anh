using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP_SMART_DT_Tuan_Anh.Models
{
    public class AuditLog
    {
        public long Id { get; set; }
        public int? UserId { get; set; }
        public string? Action { get; set; }
        public string? TableName { get; set; }
        public string? RecordId { get; set; }
        public string? OldData { get; set; }
        public string? NewData { get; set; }
        public DateTime Timestamp { get; set; }
    }

}
