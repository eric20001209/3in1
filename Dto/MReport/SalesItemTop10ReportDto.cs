using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class SalesItemTop10ReportDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal AmountWithGST { get; set; }
        public decimal ProfitWithGST { get; set; }
        public double ItemQuantity { get; set; }
    }
}
