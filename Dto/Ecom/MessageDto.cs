using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce_API_RST.Dto
{
	public class MessageDto
	{
		[Required]
		[MaxLength(50)]
		public string Name{ get; set; }
		[Required]
		[MaxLength(50)]
		public string Subject{ get; set; }
		[Required]
		[MaxLength(1000)]
		public string Content{ get; set; }
		[Required]
		[MaxLength(250)]
		public string Email{ get; set; }
	}
}
