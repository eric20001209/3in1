using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce_API.Dto
{
	public class FreightDto
	{
		public string Id{ get; set; }
		public bool Active { get; set; }
		public string Region { get; set; }
		public decimal? Freight { get; set; }
		public decimal? FreeshippingActiveAmount { get; set; }
		public decimal RangeStart1{ get; set; }
		public decimal RangeStart2 { get; set; }
		public decimal RangeStart3 { get; set; }
		public decimal RangeEnd1 { get; set; }
		public decimal RangeEnd2 { get; set; }
		public decimal RangeEnd3 { get; set; }

	}
}
