using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Models
{
	public class Image
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public int Code { get; set; }
		public string Url { get; set; }
		public bool MainPic { get; set; }
		public byte[] PicToBinary { get; set; }
		public string ExtensionName { get; set; }
	}
}
