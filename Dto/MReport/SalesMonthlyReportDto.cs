using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class SalesMonthlyReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalWithGST { get; set; }
        public decimal ProfitWithGST { get; set; }
        public int InvoiceQuantity { get; set; }
    }
}
