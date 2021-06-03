using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Models
{
	public class Branch
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public bool Activated{ get; set; }
		public bool ApiSync{ get; set; }
		public string AuthCode { get; set; }
	}
}
