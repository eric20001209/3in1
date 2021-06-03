using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Models
{
	public class Barcode
	{
        public int Id { get; set; }
        public int ItemCode { get; set; }
        public string Barcode1 { get; set; }
        public double ItemQty { get; set; }
        public double CartonQty { get; set; }
        public string CartonBarcode { get; set; }
        public double BoxQty { get; set; }
        public decimal PackagePrice { get; set; }
        public string SupplierCode { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal? VoucherAmount { get; set; }
        public bool Bcancelled { get; set; }
        public string CancelledNote { get; set; }
        public DateTime VoucherCreated { get; set; }
    }
}
