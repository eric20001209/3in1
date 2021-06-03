using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class SalesSubSubCategoryReportFilterDto : IFilter
    {
        public List<int> BranchIds { get; set; } = new List<int>();
        public List<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
        public DateTime StartDateTime { get; set; } = new DateTime(1753, 1, 1);
        public DateTime EndDateTime { get; set; } = DateTime.MaxValue;
        public bool OnlineOrder { get; set; } = false;
    }
}
