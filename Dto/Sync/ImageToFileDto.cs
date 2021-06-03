using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Dtos
{
	public class ImageToFileDto
	{
		public int Code { get; set; }
		public byte[] BinaryOfImage { get; set; }
		public string ExtensionName { get; set; }
	}
}
