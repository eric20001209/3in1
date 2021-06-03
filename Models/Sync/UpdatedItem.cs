using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Models
{
	public class UpdatedItem
	{
		public int Id { get; set; }
		public int BranchId { get; set; }
		public int ItemCode { get; set; }
		public bool Delete { get; set; }
		public DateTime DateUpdated { get; set; }
		public byte[] TimeStamp { get; set; }
		//this attibute is for record item update time
		public string TimeStampS { get; set; }
	}
}
