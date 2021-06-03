using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class WholesaleItemDailyReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal AmountWithGST { get; set; }
        public decimal ProfitWithGST { get; set; }
        public double ItemQuantity { get; set; }
    }
}
