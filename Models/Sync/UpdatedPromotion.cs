using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Models
{
	public class UpdatedPromotion
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public int BranchId { get; set; }
		public DateTime DateUpdated { get; set; }
		[Timestamp]
		public byte[] TimeStamp { get; set; }
	}
}
