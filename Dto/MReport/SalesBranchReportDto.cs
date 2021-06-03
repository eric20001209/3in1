using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class SalesBranchReportDto : ISalesBranchReportDto
    {
        public string BranchName { get; set; }
        public decimal TotalWithGST { get; set; }
        public decimal ProfitWithGST { get; set; }
        public int InvoiceQuantity { get; set; }
    }
}
