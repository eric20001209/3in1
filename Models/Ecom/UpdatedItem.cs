using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce_API_RST.Models
{
	public class UpdatedItem
	{
		public int Id { get; set; }
		public int BranchId { get; set; }
		public int ItemCode { get; set; }
		public DateTime DateUpdated { get; set; }
		public byte[] TimeStamp { get; set; }
	}
}
