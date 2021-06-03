using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class SalesInvoiceEf
    {
        public int invoiceNumber { get; set; }
        public DateTime commitDate { get; set; }
        public IEnumerable<SalesItemEf> salesItems { get; set; }
    }
    public class SalesItemEf
    {
        public int promoType { get; set; }
        public double commitPrice { get; set; }
        public int qty { get; set; }
        public int promoDesc { get; set; }
        public int promoId { get; set; }
        public int isSpecial { get; set; }
        public double normalPrice { get; set; }


    }
    public class SalesInvoiceDto
    {
        public IEnumerable<SalesItemDto> SalesItems { get; set; }
    }
    public class SalesItemDto
    {
        public string promoType { get; set; }
        public double cost { get; set; }
        public double commitPrice  { get; set; }
        public int qty { get; set; }
        public double normalPrice { get; set; }
    }
}
