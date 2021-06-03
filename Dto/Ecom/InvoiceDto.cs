using eCommerce_API.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce_API.Dto
{
	public class InvoiceDto
	{
        public int? inovice_number { get; set; }
        public string logo_url{ get; set; }
        public int order_id { get; set; }
        public decimal tax { get; set; }
        public DateTime commit_date { get; set; }
        public decimal total { get; set; }
        public decimal freight{ get; set; }
        public decimal sub_total{ get; set; }
        public string po_box { get; set; }
        public string gst{ get; set; } 
        public ShippingToDto shipto{ get; set; }

        public List<OrderItemDto> sales_items = new List<OrderItemDto>();
        public List<PaymentDto> payment = new List<PaymentDto>();
    }
}
