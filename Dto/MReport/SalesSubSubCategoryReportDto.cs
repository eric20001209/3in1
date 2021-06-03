using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class SalesSubSubCategoryReportDto
    {
        public int BranchId { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string SubSubCategory { get; set; }
        public decimal AmountWithoutGST { get; set; }
        public decimal ProfitWithoutGST { get; set; }
        public decimal BudgetWithoutGST { get; set; }
    }
}
