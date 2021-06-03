using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class SalesItemBranchReportDto
    {
        public string BranchName { get; set; }
        public decimal AmountWithGST { get; set; }
        public decimal ProfitWithGST { get; set; }
        public double ItemQuantity { get; set; }
    }
}
