using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce_API_RST.Dto
{
	public class StoreSpecialDto
	{
		public int Id { get; set; }
		public bool Enabeld { get; set; }
		public decimal? Price { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
	}
}
