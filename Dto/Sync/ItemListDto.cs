
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Dtos
{
	public class ItemListDto
	{
		public List<ItemForSyncDto> Items { get; set; }
		public int CurrentPage{ get; set; }
		public int PageSize{ get; set; }
		public int ItemCount{ get; set; }
		public int PageCount{ get; set; }

		public List<ItemDeletedDto> ItemsToDelete { get; set; }
	}
}
