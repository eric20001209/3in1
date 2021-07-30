using Sync.Dtos;
using Sync.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Sync.Services
{
	public interface IItem
	{
		StoreSpecialDto SpecialSetting(int code, int branch);
		IEnumerable<string> getBarcodes(int code);
		string getItemDetail(int code);
		itemDetailsDto getItemDetails(int code);
		decimal getLevelPrice(int code, int level);
		string getCat(string level, int code);
		decimal getOnlineShopPrice(int branch_id, int code);
		double? getWeight(int code);
		double getItemStork(int branch_id, int code);
//		bool freeDelevery(int code);
		string getBarcode(int code);
		IEnumerable<BarcodeDto> getBarcodesWithQtyAndPrice(int code);
		IEnumerable<object> getItemsByPromotionId(int promo_id,int promo_type);
	}
	public class ItemRepository : IItem
	{
		private readonly Sync.Data.AppDbContext _context;
		private readonly IConfiguration _config;

		public ItemRepository(Sync.Data.AppDbContext context, IConfiguration config)
		{
			_context = context;
			_config = config;
		}
		public IEnumerable<string> getBarcodes(int code)
		{
			var barcodes = _context.Barcode.Where(b => b.ItemCode == code)
				.Select(b => b.Barcode1);
			return barcodes;
		}

		public string getCat(string level, int code)
		{
			if (level == "cat")
			{
				var item = _context.CodeRelations.Where(c => c.Code == code).Select(c => c.Cat).FirstOrDefault();
				return item;
			}

			else if (level == "scat")
				return _context.CodeRelations.Where(c => c.Code == code).Select(c => c.SCat).FirstOrDefault();
			else if (level == "sscat")
				return _context.CodeRelations.Where(c => c.Code == code).Select(c => c.SsCat).FirstOrDefault();
			return "";
		}

		public string getItemDetail(int code)
		{
			var item = _context.ProductDetails.Where(pd => pd.Code == code).FirstOrDefault();
			if (item != null)
				return item.Details;
			else
				return null;
		}
		public itemDetailsDto getItemDetails(int code)
		{
			return _context.ProductDetails.Where(pd => pd.Code == code)
					.Select(pd => new itemDetailsDto
					{
						Highlight = pd.Highlight,
						Specification = pd.Spec,
						Manufacture = pd.Manufacture,
						Picture = "http://" + _context.Url + "/pi/" + code + ".jpg",
						//				Rev = pd.Rev,
						Warranty = pd.Warranty,
						Details = pd.Details,
						Ingredients = pd.Ingredients,
						Directions = pd.Directions,
						Advice = pd.Advice,
						Shipping = pd.Shipping
					
					}).FirstOrDefault();

		}

		public double getItemStork(int branch_id, int code)
		{
			var result = _context.StockQty.Where(sq => sq.Code == code && sq.BranchId == branch_id).FirstOrDefault();
			if (result == null)
				return 0;
			return result.Qty ?? 0;
		}

		public decimal getLevelPrice(int code, int level)
		{
			var items = _context.CodeRelations.Where(cr => cr.Code == code)
				.Select(cr => new {
					cr.Price1,
					cr.LevelPrice1,
					cr.LevelPrice2,
					cr.LevelPrice3,
					cr.LevelPrice4,
					cr.LevelPrice5,
					cr.LevelPrice6,
					cr.LevelPrice7,
					cr.LevelPrice8,
					cr.LevelPrice9
				});

			var count = items.Count();
			if (items == null)
				return 0;
			var item = items.FirstOrDefault();
			if (item != null)
			{
				switch (level)
				{
					case 1:
						return item.LevelPrice1;
					case 2:
						return item.LevelPrice2;
					case 3:
						return item.LevelPrice3;
					case 4:
						return item.LevelPrice4;
					case 5:
						return item.LevelPrice5;
					case 6:
						return item.LevelPrice6;
					case 7:
						return item.LevelPrice7;
					case 8:
						return item.LevelPrice8;
					case 9:
						return item.LevelPrice9;
					default:
						return item.Price1;
				}
			}
			return item.Price1;
		}

		public decimal getOnlineShopPrice(int branch_id, int code)
		{
			var result = _context.CodeBranch.Where(cb => cb.BranchId == branch_id && cb.Code == code).FirstOrDefault();
			if (result != null)
				return result.Price1 ?? 0;
			var item = _context.CodeRelations.Where(c => c.Code == code)
						.Select(c => new { c.Price1, c.Code }).FirstOrDefault();
			if (item != null)
				return item.Price1;
			return 0;
		}

		public double? getWeight(int code)
		{
			var item = _context.CodeRelations.Where(c => c.Code == code)
						.Select(c => new { c.Weight }).FirstOrDefault();
			if (item != null)
				return item.Weight;
			else
				return 0;
		}

		//public bool freeDelevery(int code)
		//{
		//	var item = _context.CodeRelations.Where(c => c.Code == code)
		//				.Select(c => new { c.FreeDelivery }).FirstOrDefault();
		//	if (item != null)
		//		return item.FreeDelivery;
		//	else
		//		return false;
		//}

		public StoreSpecialDto SpecialSetting(int code, int branch)
		{
			return _context.StoreSpecial.Where(s => s.Code == code && s.BranchId == branch)
			.Select(s => new StoreSpecialDto
			{
				Id = s.Id,
				Price = s.Price,
				Enabeld = s.Enabled,
				Start = s.PriceStartDate,
				End = s.PriceEndDate
			}).FirstOrDefault();
		}

		public string getBarcode(int code)
		{
			var barcode = _context.CodeRelations.Where(cr => cr.Code == code)
						.Select(c => c.Barcode).FirstOrDefault();
			if (barcode != null)
				return barcode;
			else
				return "";
		}

		public IEnumerable<BarcodeDto> getBarcodesWithQtyAndPrice(int code)
		{
			var barcodes = _context.Barcode.Where(b => b.ItemCode == code).ToList();
			if (barcodes != null)
			{
				var barcodesWithQtyAndPrice = barcodes.Select(b => new BarcodeDto
				{
					ItemQty = b.ItemQty,
					PackagePrice = b.PackagePrice,
					Barcode = b.Barcode1
				}).ToList();
				return barcodesWithQtyAndPrice;
			}
			else
				return null;
		}

		public IEnumerable<object> getItemsByPromotionId(int promo_id , int promo_type)
		{
			try
			{
				if (promo_type < 6 && promo_type > 0)
				{
					var itemsWithCode =
							(from c in _context.CodeRelations
							 where c.PromoId  == promo_id
							 select new PromotionItemDto { Code = c.Code}).ToList();
					return (itemsWithCode);
				}
				else if (promo_type == 6 || promo_type == 7)
				{
					var itemsWithBarcode = (from gp in _context.PromotionGroups
							 where gp.PromoId == promo_id
							 join b in _context.Barcode on gp.Barcode equals b.Barcode1
							 select new PromotionItemDto { Code=b.ItemCode, Barcode = gp.Barcode }).ToList();
					return (itemsWithBarcode);
				}
				else if (promo_type == 8)
				{
					return null;
				}

			}
			catch (Exception ex)
			{

				return null;
			}
			return null;

			//var final = from i in items
			//			group i.Barcode1 by i.Code into g
			//			select new PromotionItemDto { Code = g.Key, Barcodes = g };
			//into p
			//group p.Barcode1 by p.Code into g
			//select new PromotionItemDto { Code = g.Key, Barcodes = g };
			//_context.CodeRelations.Where(cr => cr.PromoId == promo_id)
			//	.Join(_context.Barcode, cr => cr.Code, b => b.ItemCode, (cr, b) => new { cr.Code, b.Barcode1 })
			//	.GroupBy(g => g.Code, g => g.Barcode1, (key, g) => new PromotionItemDto { Code = key, Barcodes = g.ToList() })

		}
	}
}
