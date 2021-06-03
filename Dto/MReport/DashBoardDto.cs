using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class DashboardDto
    {
        public decimal TotalWithoutGSTThisYear { get; set; }
        public decimal ProfitWithoutGSTThisYear { get; set; }
        public decimal BasketSpendWithoutGSTThisYear { get; set; }
        public decimal BudgetWihoutGSTThisYear { get; set; }
        public decimal WasteWithoutGSTThisYear { get; set; }
        public decimal MarkDownWithoutGSTThisYear { get; set; }
        public decimal TotalWithoutGSTLastYear { get; set; }
        public decimal ProfitWithoutGSTLastYear { get; set; }
        public decimal BasketSpendWithoutGSTLastYear { get; set; }
        public decimal BudgetWihoutGSTLastYear { get; set; }
        public decimal WasteWithoutGSTLastYear { get; set; }
        public decimal MarkDownWithoutGSTLastYear { get; set; }
    }
    public class DailyDashboardDto
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public decimal TotalWithoutGST { get; set; }
        public decimal ProfitWithoutGST { get; set; }
        public decimal BasketSpendWithoutGST { get; set; }
        public int InvoiceQuantity { get; set; }
    }
}
