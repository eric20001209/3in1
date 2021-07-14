using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Models
{
	public class Button
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string NameEn { get; set; }
		public bool IsIndivisual { get; set; }
		public virtual ICollection<Button> ButtonItems{ get; set; }
		public int? Code { get; set; }
        public byte[] Pic { get; set; }
    }
}
