using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Models
{
	public class ButtonItem
	{
		public int Id { get; set; }	
		public int Code { get; set; }
		public string Name { get; set; }
		public string NameEn { get; set; }
		public string Location { get; set; }

		[ForeignKey("Button")]
		public int ButtonId { get; set; }
		public virtual Button Button { get; set; }
	}
}
