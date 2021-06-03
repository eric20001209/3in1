using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Dtos
{
	public class PromotionListDto
	{
		public List<List<PromotionDto>> Promotions { get; set; }   //different types of promotions will be seperate
	}
}
