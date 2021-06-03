using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Dtos
{
	public class ImageUploadDto
	{
		public int Code { get; set; }
		public string Url { get; set; }
		public bool MainPic { get; set; }
		public byte[] Pic { get; set; }
	}
}
