using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Models
{
	public class PromotionList
	{
		public int PromoId { get; set; }
		public string PromoDesc { get; set; }
		public int PromoType { get; set; }
		public DateTime PromoStartDate { get; set; }
		public DateTime PromoEndDate { get; set; }
		public bool PromoActive { get; set; }
		public bool PromoMemberOnly { get; set; }
		public bool PromoDay1 { get; set; }
		public bool PromoDay2 { get; set; }
		public bool PromoDay3 { get; set; }
		public bool PromoDay4 { get; set; }
		public bool PromoDay5 { get; set; }
		public bool PromoDay6 { get; set; }
		public bool PromoDay7 { get; set; }
		public decimal SpecialPrice { get; set; }
		public double DiscountPercentage { get; set; }
		public double FreeQtyRequiredQty { get; set; }
		public double FreeQtyRewardQty { get; set; }
		public double VolumnDiscountQty { get; set; }
		public decimal VolumnDiscountPriceTotal { get; set; }
		public double FreeItemRequiredQty { get; set; }
		public string FreeItemRequiredItemCode { get; set; }
		public double FreeItemRewardQty { get; set; }
		public DateTime PromoCreateDate { get; set; }
		public string PromoCreateBy { get; set; }
		public int PromoBranchId { get; set; }
		public double Limit { get; set; }
	}
}
