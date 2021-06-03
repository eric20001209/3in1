using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Dtos
{
	public class ButtonItemDto
	{
		public int Id { get; set; }
		public int ButtonId { get; set; }
		public int Code { get; set; }
		public string Name { get; set; }
		public string NameEn { get; set; }
		public string Location { get; set; }
	}
}
