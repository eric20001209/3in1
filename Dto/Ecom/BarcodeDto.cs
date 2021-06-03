using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce_API.Dto
{
    public class BarcodeDto
    {
        public string Barcode { get; set; }
        public double ItemQty{ get; set; }
        public decimal PackagePrice { get; set; }
    }
}
