using eCommerce_API.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce_API_RST.Dto
{
	public class ItemListDto
	{
		public IQueryable<ItemDto> Items;
		public int CurrentPage{ get; set; }
		public int PageSize{ get; set; }
		public int ItemCount{ get; set; }
		public int PageCount{ get; set; }
	}
}
