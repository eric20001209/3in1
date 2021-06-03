using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Dtos
{
	public class MessageWithTimeStampDto
	{
		public string Code { get; set; }
		public string Msg { get; set; }
		public object Data { get; set; }
		public byte[] TimeStamp { get; set; }
	}
}
