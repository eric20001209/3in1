using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce_API_RST_Multi.Models.Sync
{
    public class Product
    {
        public int Code { get; set; }
        public string Name { get; set; }
        public string NameCn { get; set; }
        public string Brand { get; set; }
        public string Cat { get; set; }
        public string SCat { get; set; }
        public string SSCat { get; set; }
        public bool Hot { get; set; } = false;
        public decimal? Price { get; set; }
        public string Supplier { get; set; }
        public string SupplierCode { get; set; }
        public decimal SupplierPrice { get; set; } = 0;
    }
}
