using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Dtos
{
	public class ResetItemSyncDto
	{
		public int Code { get; set; }
		public byte[] TimeStamp { get; set; }
	}
}
