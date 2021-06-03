using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Models
{
	public class UpdatedCard
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public int BranchId { get; set; }
		public int CardId { get; set; }
		public string Email { get; set; }
		public string Barcode { get; set; }
		public bool Delete{ get; set; }
		public DateTime DateUpdated { get; set; }
		[Timestamp]
		public byte[] TimeStamp { get; set; }
	}
}
