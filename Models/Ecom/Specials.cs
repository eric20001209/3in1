using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce_API.Models
{
	public class Specials
	{
		[Key]
		public int Code { get; set; }
		public decimal? price { get; set; }
	}
}
