using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Dtos
{
	public class ButtonWithItemDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string NameEn { get; set; }
		public bool IsIndivisual { get; set; }
		public string Barcode { get; set; }
		public int? Code { get; set; }
		public byte[] Pic { get; set; }
		public ICollection<ButtonItemDto> ButtonItems { get; set; }
	}
}
