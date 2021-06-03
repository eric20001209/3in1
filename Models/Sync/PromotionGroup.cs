using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Models
{
	public class PromotionGroup
	{
		public int Id{ get; set; }
		public int PromoId { get; set; }
		public string Barcode { get; set; }
		public int PromoType { get; set; }
	}
}
