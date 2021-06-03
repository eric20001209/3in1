using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public interface ISalesBranchReportDto
    {
        string BranchName { get; set; }
        decimal TotalWithGST { get; set; }
        decimal ProfitWithGST { get; set; }
        int InvoiceQuantity { get; set; }
    }
}
