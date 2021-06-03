using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Models
{
	public class WorkTime
	{
		public int Id { get; set; }
		public int CardId { get; set; }
		public DateTime RecordTime { get; set; }
		public bool IsCheckin { get; set; }
		public double Hours { get; set; }
		public string Name { get; set; }
		public string Barcode { get; set; }
		public int BranchId { get; set; }

	}
}
