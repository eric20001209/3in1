using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace FarroAPI.Models
{
    public class SalesInvoiceReportDto
    {
        public string BranchName { get; set; }
        public int? TillNumber { get; set; }
        public int InvoiceNumber { get; set; }
        public string LocalInvoiceNumber { get; set; }
        public DateTime InvoiceDateTime { get; set; }
        public string Customer { get; set; }
        public string SalesPerson { get; set; }
        public decimal AmountWithGST { get; set; }
        public decimal ProfitWithGST { get; set; }
    }
}
