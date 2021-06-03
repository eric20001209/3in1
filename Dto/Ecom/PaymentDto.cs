using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce_API.Dto
{
	public class PaymentDto
	{
		public string payment_method { get; set; }
		public decimal amount { get; set; }
	}
}
