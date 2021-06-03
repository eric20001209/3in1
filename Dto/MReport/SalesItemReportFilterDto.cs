using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class SalesItemReportFilterDto : IFilter
    {
        public List<int> BranchIds { get; set; } = new List<int>();
        public List<SubSubCategory> SubSubCategories { get; set; } = new List<SubSubCategory>();
        public List<int> SupplierIds { get; set; } = new List<int>();
        public DateTime StartDateTime { get; set; } = new DateTime(1753, 1, 1);
        public DateTime EndDateTime { get; set; } = DateTime.MaxValue;
        public bool OnlineOrder { get; set; }
    }
}
