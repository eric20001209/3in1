using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Dtos
{
	public class ResetCardSyncDto
	{
		public int Id { get; set; }
		public string Email { get; set; }
		public string Barcode { get; set; }
		public byte[] TimeStamp { get; set; }
	}
}
