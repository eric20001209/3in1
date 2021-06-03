using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce_API_RST_Multi.Dto.Sync
{
    public class CardCreateDto
    {
		[Required(ErrorMessage = "email is required!")]
		[EmailAddress]
		public string Email { get; set; }
		//[Required]
		//public string Password { get; set; }
		[MaxLength(30, ErrorMessage = "user name length is no more than 30.")]
		public string Name { get; set; }
		[Phone]
		public string Phone { get; set; }
		public string Barcode { get; set; }
		public int Type { get; set; }
	}
}
