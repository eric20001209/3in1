using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Dtos
{
	public class CardDeletedDto
	{
		public int CardId { get; set; }
		public string Barcode { get; set; }
		public string Email { get; set; }
		[Timestamp]
		public byte[] TimeStamp { get; set; }
	}
}
