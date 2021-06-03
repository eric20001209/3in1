using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class SalesItemBranchReportFilterDto : IFilter
    {
        public DateTime StartDateTime { get; set; } = new DateTime(1753, 1, 1);
        public DateTime EndDateTime { get; set; } = DateTime.MaxValue;
        public int? Code { get; set; } = null;
        public int? BranchId { get; set; } = null;
    }
}
