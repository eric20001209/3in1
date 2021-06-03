using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class SalesCategoryBranchReportDto
    {
        public int BranchId { get; set; }
        public decimal AmountWithGST { get; set; }
        public decimal ProfitWithGST { get; set; }
        public decimal BudgetWithGST { get; set; }
    }
}
