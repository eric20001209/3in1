using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce_API_RST.Models
{
	public class MessageBoard
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		[MaxLength(50) ]
		[Required]
		public string Name{ get; set; }
		[Required]
		[MaxLength(50)]
		public string Subject{ get; set; }
		[Required]
		public string Content{ get; set; }
		[EmailAddress]
		[Required]
		public string Email{ get; set; }
	}
}
