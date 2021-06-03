using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Dtos
{
	public class ItemForSyncDto
	{
		// from code_relations
		public string Id { get; set; }
		public int Code { get; set; }
		public string SupplierCode { get; set; }
		public string Name { get; set; }
		public string NameCn { get; set; }
		public string Brand { get; set; }
		public string Cat { get; set; }
		public string SCat { get; set; }
		public string SsCat { get; set; }
		public decimal Price1 { get; set; }
		public decimal AverageCost { get; set; }
		public int? PromoId { get; set; }
		public bool? HasScale { get; set; }
		public bool? IsBarcodeprice { get; set; }
		public bool? IsIdCheck { get; set; }
		public decimal LevelPrice0 { get; set; }
		public decimal LevelPrice1 { get; set; }
		public decimal LevelPrice2 { get; set; }
		public decimal LevelPrice3 { get; set; }
		public decimal LevelPrice4 { get; set; }
		public decimal LevelPrice5 { get; set; }
		public decimal LevelPrice6 { get; set; }
		public decimal LevelPrice7 { get; set; }
		public decimal LevelPrice8 { get; set; }
		public decimal LevelPrice9 { get; set; }
		//from barcode
		public virtual IEnumerable<BarcodeDto> Barcodes { get; set; } = new List<BarcodeDto>();
		//from code_branch
		public int BranchId { get; set; }
		public bool? Inactive { get; set; }
		public decimal BranchPrice { get; set; }
		// from store_special
		public StoreSpecialDto StoreSpecial { get; set; }
		public double? Stock { get; set; }
		// from product details
		public itemDetailsDto ItemDetails { get; set; }
//		public bool Delete { get; set; }
		[Timestamp]
		public byte[] TimeStamp { get; set; }
	}
}
