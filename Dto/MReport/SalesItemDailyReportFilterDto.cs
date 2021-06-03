using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class SalesItemDailyReportFilterDto : IFilter
    {
        public DateTime StartDateTime { get; set; } = new DateTime(1753, 1, 1);
        public DateTime EndDateTime { get; set; } = DateTime.MaxValue;
        public List<int> BranchIds { get; set; } = new List<int>();
        public List<int> Codes { get; set; } = new List<int>();
    }
}
