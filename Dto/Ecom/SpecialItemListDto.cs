using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eCommerce_API.Dto;

namespace eCommerce_API_RST.Dto
{
	public class SpecialItemListDto
	{
		public IEnumerable<ItemDto> Items;
		public int CurrentPage { get; set; }
		public int PageSize { get; set; }
		public int ItemCount { get; set; }
		public int PageCount { get; set; }
	}
}
