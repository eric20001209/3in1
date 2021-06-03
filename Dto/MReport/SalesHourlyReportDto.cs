using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class SalesHourlyReportDto
    {
        public int StartHour { get; set; }
        public int EndHour { get; set; }
        public decimal AverageTotalWithGST { get; set; }
        public decimal AverageProfitWithGST { get; set; }
        public double AverageInvoiceQuantity { get; set; }
    }
}
