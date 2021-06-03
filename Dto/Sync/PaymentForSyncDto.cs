using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Dtos
{
	public class PaymentForSyncDto
	{
		public int TranId { get; set; }
		public int PaymentMethod { get; set; }
		public string PaymentRef { get; set; }
		public DateTime TransDate { get; set; }
		public decimal AmountApplied { get; set; }
		public string Note { get; set; }
	}
}
