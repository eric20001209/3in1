using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Dtos
{
	public class ItemDeletedDto
	{
		public int Code { get; set; }
		[Timestamp]
		public byte[] TimeStamp { get; set; }
	}
}
