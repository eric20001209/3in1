using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Models
{
	public class Catalog
	{

		[Key]

		public string Seq { get; set; }
		public string Cat { get; set; }
		public string SCat { get; set; }
		public string SSCat { get; set; }
	}
}
