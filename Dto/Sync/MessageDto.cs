using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Dtos
{
	public class MessageDto
	{
		public string Code { get; set; }
		public string Msg { get; set; }
		public object Data{ get; set; }
		public List<object> Skipped { get; set; } = new List<object>();
		public List<object> Processed { get; set; } = new List<object>();
	}
}
