using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Dtos
{
	public class TransResetDto
	{
		public string Msg { get; set; }
		public int TranId { get; set; }
		public int InvoiceNumber { get; set; }
	}
}
