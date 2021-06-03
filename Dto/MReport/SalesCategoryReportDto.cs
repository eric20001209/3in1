using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class SalesCategoryReportDto
    {
        public string Category { get; set; }
        public decimal TotalWithoutGST { get; set; }
        public decimal ProfitWithoutGST { get; set; }
        public decimal BasketSpendWithoutGST { get; set; }
     
       
        public decimal MarkDownWithoutGST { get; set; }
    }
}
