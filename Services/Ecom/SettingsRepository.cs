using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eCommerce_API.Data;
using eCommerce_API.Dto;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Internal.Account;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce_API.Services
{
	public interface ISetting
	{
		int getOnlineShopId();
		IEnumerable<FreightDto> getFreightSetting();
		FreightDto getRegionFreightSetting(int id);
		IEnumerable<ShippingMethodDto> getShippingMethod();
		string getDealerRegion(int id);
		decimal getDealerFreight(decimal total, int dfid);
		string getOrderStatus(int id);
		List<FreightInfoDto> getFreightInfo(int invoice_number);
		List<FreightInfoDto> getFreightInfoByPo(string po_number);
		string getPaymentMethodById(int id);
		int getIdByPaymentMethod(string paymentMethod);
	}
	public class SettingsRepository : ISetting
	{
		private readonly FreightContext _context;
		private readonly rst374_cloud12Context _contextE;
		public SettingsRepository(FreightContext context, rst374_cloud12Context contextE)
		{
			_context = context;
			_contextE = contextE;
		}

		public int getOnlineShopId()
		{
			var onlineshop = _context.Settings.Where(s => s.Name == "online_shop_id").FirstOrDefault();
			if (onlineshop == null)
				return 1;
			var final = Convert.ToInt32(onlineshop.Value);
			return final;
		}
		public IEnumerable<FreightDto> getFreightSetting()
		{
//			return new List<FreightDto>();
			
				var result= _context.FreightSettings.Where(f => f.Active == true)
					.Select(i => new FreightDto
					{
						Id = i.Id.ToString(),
						Region = i.Region,
						Freight = i.Freight,
						Active = i.Active ?? false,
						FreeshippingActiveAmount = i.FreeshippingActiveAmount,
						RangeStart1 = i.FreightRangeStart1,
						RangeStart2 = i.FreightRangeStart2,
						RangeStart3 = i.FreightRangeStart3,
						RangeEnd1 = i.FreightRangeEnd1,
						RangeEnd2 = i.FreightRangeEnd2,
						RangeEnd3 = i.FreightRangeEnd3


					}).OrderBy(i=>i.Region);
			return result;	
		}
		public IEnumerable<ShippingMethodDto> getShippingMethod()
		{
			return _contextE.Enum.Where(e => e.Class == "shipping_method")
				.Select(e => new ShippingMethodDto
				{
					Id = e.Id,
					Name = e.Name
				}).OrderBy(e => e.Id);
		}

		public FreightDto getRegionFreightSetting(int id)
		{
			return _context.FreightSettings.Where(f => f.Id == id)
					.Select(i=>new FreightDto
					{ 
						Id = i.Id.ToString(),
						Active = i.Active ?? false,
						Region = i.Region,
						Freight = i.Freight,
						FreeshippingActiveAmount = i.FreeshippingActiveAmount,
						RangeStart1 = i.FreightRangeStart1,
						RangeStart2 = i.FreightRangeStart2,
						RangeStart3 = i.FreightRangeStart3,
						RangeEnd1 = i.FreightRangeEnd1,
						RangeEnd2 = i.FreightRangeEnd2,
						RangeEnd3 = i.FreightRangeEnd3,
					}).FirstOrDefault();
		}

		public decimal getDealerFreight(decimal total, int dfid)
		{

			decimal? regionUnitPrice = 0;
			decimal? regionFreeShippingActiveAmount = 0;
			decimal rangeStart1 = 0;
			decimal rangeStart2 = 0;
			decimal rangeStart3 = 0;
			decimal rangeEnd1 = 0;
			decimal rangeEnd2 = 0;
			decimal rangeEnd3 = 0;
			var regionShipping = this.getRegionFreightSetting(dfid);
			if (regionShipping != null)
			{
				regionUnitPrice = regionShipping.Freight;
				regionFreeShippingActiveAmount = regionShipping.FreeshippingActiveAmount;
				rangeStart1 = regionShipping.RangeStart1;
				rangeStart2 = regionShipping.RangeStart2;
				rangeStart3 = regionShipping.RangeStart3;
				rangeEnd1 = regionShipping.RangeEnd1;
				rangeEnd2 = regionShipping.RangeEnd2;
				rangeEnd3 = regionShipping.RangeEnd3;
			}

			var totalAmount = total ;
			var key = 0;
			if (totalAmount >= regionFreeShippingActiveAmount) //total amount greater than shipping active amount, freight = 0
				return 0;
			if (totalAmount >= rangeStart1 && totalAmount <= rangeEnd1)
				key = 1;
			if (totalAmount > rangeStart2 && totalAmount <= rangeEnd2)
				key = 2;
			if (totalAmount > rangeStart3 && totalAmount < rangeEnd3)
				key = 3;
			switch (key)
			{
				case 1:
					return regionUnitPrice ?? 0;
				case 2:
					return regionUnitPrice*2 ?? 0;
				case 3:
					return regionUnitPrice*3 ?? 0;
				default:
					break;
			}
			return 0;
		}

		public string getDealerRegion(int id)
		{
			var card = _context.FreightSettings.Where(fs => fs.Id == id).FirstOrDefault();
			if (card == null)
				return "none";
			var result = String.IsNullOrEmpty(card.Region) ? "Pickup" : card.Region;
			return result;

		}

		public string getOrderStatus(int id)
		{
			var result = _contextE.Enum.Where(e => e.Class == "order_item_status" && e.Id == id).FirstOrDefault();
			if (result != null)
			{
				return result.Name;
			}
			else
				return "Placed";
				
		}
		public string getPaymentMethodById(int id)
		{ 
			var payment = _contextE.Enum.Where(e => e.Class == "payment_method" && e.Id == id).FirstOrDefault();
			if (payment != null)
			{
				return payment.Name;
			}
			else
				return "others";
		}

		public int getIdByPaymentMethod(string paymentMethod)
		{
			var payment = _contextE.Enum.Where(e => e.Class == "payment_method" && e.Name == paymentMethod).FirstOrDefault();
			if (payment != null)
			{
				return payment.Id;
			}
			else
				return getIdByPaymentMethod("others");
		}

		public List<FreightInfoDto> getFreightInfo(int invoice_number)
		{

			List<FreightInfoDto> freightInfo = _contextE.InvoiceFreight.Where(i => i.InvoiceNumber == invoice_number)
				.Select(i => new FreightInfoDto
				{
					invoice_number = i.InvoiceNumber,
					ship_name = i.ShipName,
					ship_desc = i.ShipDesc,
					ship_id = i.ShipId.Value,
					ticket = i.Ticket,
					price = i.Price

				}).ToList();
			return freightInfo;

		}

		public List<FreightInfoDto> getFreightInfoByPo(string po_number)
		{
			var freightInfo = //new List<FreightInfoDto>();
			_contextE.Orders.Where(i => i.PoNumber == po_number)
			.Select(o => new { o.PoNumber, o.InvoiceNumber })
			.Join(_contextE.InvoiceFreight
			, (o => o.InvoiceNumber)
			, (i => i.InvoiceNumber)
			, (o, i) => new { o.PoNumber, i.InvoiceNumber, i.ShipName, i.ShipDesc, i.ShipId, i.Ticket, i.Price })
			 .Select(i => new FreightInfoDto
			 {
				 invoice_number = i.InvoiceNumber,
				 ship_name = i.ShipName,
				 ship_desc = i.ShipDesc,
				 ship_id = i.ShipId.Value,
				 ticket = i.Ticket,
				 price = i.Price

			 }).ToList();
			return freightInfo;
		}
	}
}
