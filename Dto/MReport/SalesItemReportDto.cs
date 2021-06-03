using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class SalesItemReportDto
    {
        public int BranchId { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string SubSubCategory { get; set; }
        public int Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal AmountWithoutGST { get; set; }
        public decimal ProfitWithoutGST { get; set; }
        public double Quantity { get; set; }
    }
}
