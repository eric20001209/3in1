using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce_API_RST.Dto
{
	public class TransForSyncDto
	{
		public int CardId { get; set; }
		public int StaffId{ get; set; }
		public decimal TotalAmount { get; set; }
		public int InvoiceNumber { get; set; }
		public int StationId{ get; set; }
		public virtual IEnumerable<PaymentForSyncDto> Payments { get; set; }
	}
}
