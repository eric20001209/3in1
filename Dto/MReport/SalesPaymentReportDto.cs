using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class SalesPaymentReportDto
    {
        public string PaymentMethod { get; set; }
        public decimal Total { get; set; }
    }
}
