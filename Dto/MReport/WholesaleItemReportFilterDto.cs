using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class WholesaleItemReportFilterDto : IFilter
    {
        public DateTime StartDateTime { get; set; } = new DateTime(1753, 1, 1);
        public DateTime EndDateTime { get; set; } = DateTime.MaxValue;
        public int? DealerId { get; set; } = null;
        public string Cat { get; set; } = null;
        public string SCat { get; set; } = null;
        public string SSCat { get; set; } = null;
        public string Keyword { get; set; } = null;
        public int? BranchId { get; set; } = null;
    }
}
