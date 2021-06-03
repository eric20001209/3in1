using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce_API_RST.Dto
{
	public class PaymentForSyncDto
	{
		public int PaymentMethod { get; set; }
		public string PaymentRef { get; set; }
		public DateTime TransDate { get; set; }
		public decimal AmountApplied { get; set; }
		public string Note { get; set; }
	}
}
