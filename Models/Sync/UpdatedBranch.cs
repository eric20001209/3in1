using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Models
{
	public class UpdatedBranch
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id{ get; set; }
		public int BranchId{ get; set; }
		public bool hasUpdated{ get; set; }
		public bool HasCreated { get; set; }
		public bool HasProcessed { get; set; }
		//public bool HasItemUpdated{ get; set; }
		//public bool HasCardUpdated { get; set; }
		//public bool HasPromotionUpdated { get; set; }

	}
}
