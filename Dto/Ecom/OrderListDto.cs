using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eCommerce_API.Dto;
using eCommerce_API_RST.Dto;

namespace eCommerce_API_RST.Dto
{
	public class OrderListDto
	{
		public IQueryable<OrderDto> Orders;
		public int CurrentPage { get; set; }
		public int PageSize { get; set; }
		public int ItemCount { get; set; }
		public int PageCount { get; set; }
	}
}
