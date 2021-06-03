using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Dtos
{
	public class SyncStatusDto
	{
		public bool HasUpdates { get; set; } = false;
		public bool CurrentProcessed{ get; set; }
		public bool AllProcessed { get; set; }
	}
}
