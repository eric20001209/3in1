using eCommerce_API.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce_API_RST.Dto
{
	public class CreateOrderByDealerIdDto
	{
		public decimal? freight { get; set; }
		public int shipping_method { get; set; }
		public string sales_note { get; set; }
		public double customer_gst { get; set; } = 0.15;
		public List<CartItemDto> cartItems = new List<CartItemDto>();
		public ShippingInfoDto ShippingInfo { get; set; }
	}
}
