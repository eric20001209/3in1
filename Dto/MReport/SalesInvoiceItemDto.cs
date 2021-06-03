using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace FarroAPI.Models
{
    public class SalesInvoiceItemDto
    {
        public int ItemCode { get; set; }
        public string Description { get; set; }
        public decimal UnitPriceWithGST { get; set; }
        public double Quantity { get; set; }
    }
}
