using eCommerce_API_RST.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eCommerce_API.Data;
using eCommerce_API.Dto;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using eCommerce_API.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace eCommerce_API.Services
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
		bool freeDelevery(int code);
		string getBarcode(int code);
		IEnumerable<BarcodeDto> getBarcodesWithQtyAndPrice(int code);
	}
	public class ItemRepository : IItem
	{
		private readonly rst374_cloud12Context _context;
		private readonly FreightContext _contextf;
		public ItemRepository(rst374_cloud12Context context, FreightContext contextf)
		{
			_context = context;
			_contextf = contextf;
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
				var item =  _context.CodeRelations.Where(c => c.Code == code).Select(c=>c.Cat).FirstOrDefault();
				return item;
			}

			else if (level == "scat")
				return _context.CodeRelations.Where(c => c.Code == code).Select(c=>c.SCat).FirstOrDefault();
			else if (level == "sscat")
				return _context.CodeRelations.Where(c => c.Code == code).Select(c=>c.SsCat).FirstOrDefault();
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
						Picure = pd.Pic,
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
				.Select(cr=> new {
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
			var item = _context.CodeRelations.Where(c=>c.Code == code)
						.Select(c=>new { c.Price1,c.Code}).FirstOrDefault();
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

		public bool freeDelevery(int code)
		{
			var item = _context.CodeRelations.Where(c => c.Code == code)
						.Select(c => new { c.FreeDelivery }).FirstOrDefault();
			if (item != null)
				return item.FreeDelivery;
			else
				return false;
		}

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
			var barcode = _context.CodeRelations.Where(cr=>cr.Code == code)
						.Select(c=>c.Barcode).FirstOrDefault();
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
				var barcodesWithQtyAndPrice = barcodes.Select(b => new BarcodeDto { 
				ItemQty = b.ItemQty,
				PackagePrice = b.PackagePrice,
				Barcode = b.Barcode1
				}).ToList();
				return barcodesWithQtyAndPrice;
			}
			else
				return null;
		}
	}
}
