using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Models
{
	public class PromotionBranch
	{
		public int Id { get; set; }
		public int PromoId { get; set; }
		public int BranchId { get; set; }
	}
}
