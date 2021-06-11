using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using eCommerce_API.Data;
using eCommerce_API.Dto;
using eCommerce_API.Services;

namespace eCommerce_API.Controllers
{
    [Route("{hostId}/ecom/api/freight")]
    [ApiController]
    public class FeightController : ControllerBase
    {
		private readonly FreightContext _context;// = new FreightContext();
		private readonly ISetting _isettings;
		public FeightController(ISetting isettings, FreightContext context)
		{
			_isettings = isettings;
			_context = context;
		}
		[HttpGet()]
		public IActionResult getFreightUnitPrice()
		{
			var freightUnitPrice = "0";
			var freight = _context.Settings.Where(s => s.Name == "freight_unit_price").FirstOrDefault();
			if (freight == null)
			{
				freightUnitPrice = "5";
			}
			else
			{
				freightUnitPrice = freight.Value;
			}
			if (freightUnitPrice == null || freightUnitPrice == "")
				freightUnitPrice = "5";
			return Ok(freightUnitPrice);
		}

		[HttpGet("settings")]
		public IActionResult getFreightSettings()
		{
			/********   Default Oversea Freight   ****************/
			var freight = _context.Settings.Where(s => s.Name == "freight_unit_price").FirstOrDefault().Value;
			var freightUnitPrice = decimal.Parse(freight ?? "5");
			//if (freight == null || freight == "")
			//	freightUnitPrice = 5;

			/*********   Free shipping & Domestic Freight Settings   ********************/
			var freeshippingEnabled = _context.Settings.Where(s => s.Cat == "Freight Options" && s.Name == "free_shipping_enabled").FirstOrDefault();
			var sfreeshippingEnabled = "0" ;
			if (freeshippingEnabled != null)
				sfreeshippingEnabled =freeshippingEnabled.Value; 

			var freeshippingInternational = _context.Settings.Where(s => s.Cat == "Freight Options" && s.Name == "free_shipping_international").FirstOrDefault();
			var sfreeshippingInternational = "0";
			if (freeshippingInternational != null)
				sfreeshippingInternational =  freeshippingInternational.Value;

			var free_shipping_domestic = _context.Settings.Where(s => s.Cat == "Freight Options" && s.Name == "free_shipping_domestic").FirstOrDefault();
			var sfree_shipping_domestic = "0";
			if (free_shipping_domestic != null)
				sfree_shipping_domestic = free_shipping_domestic.Value;

			var freeShippingActiveAmount = _context.Settings.Where(s => s.Name == "free_shipping_active_amount").FirstOrDefault();
			var sfreeShippingActiveAmount = "100";
			if (freeShippingActiveAmount != null)
				sfreeShippingActiveAmount = freeShippingActiveAmount.Value;

			var domesticFreightOption = _context.Settings.Where(s => s.Cat == "Freight Options" && s.Name == "number_of_freight_options").FirstOrDefault();
			var sdomesticFreightOption = "0";
			if (domesticFreightOption != null)
				sdomesticFreightOption = domesticFreightOption.Value;

			int i_domesticFreightOption = int.Parse(sdomesticFreightOption ?? "0");
			List<DomesticFreightOptionDto> domesticFrieghtList = new List<DomesticFreightOptionDto>();
			for (int i = 1; i <= i_domesticFreightOption; i++)
			{ 
				var name = _context.Settings.Where(s => s.Cat == "Freight Options" && s.Name == "freight_option_name"+i.ToString()).FirstOrDefault().Value;
				var price = _context.Settings.Where(s => s.Cat == "Freight Options" && s.Name == "freight_option_price" + i.ToString()).FirstOrDefault().Value;
				decimal dprice = decimal.Parse(price??"0");
				domesticFrieghtList.Add(new DomesticFreightOptionDto { Id = i, Description = name, Price = dprice }) ;
			}

			return Ok( new { OverseaFreight = freightUnitPrice ,
							DomesticFreight = domesticFrieghtList,
							Freeshipping = freeshippingEnabled.Value,
							FreeshppingActiveAmount = freeShippingActiveAmount.Value
			});
		}

		[HttpGet("fixed")]
		public IActionResult getFixedFreightSettings()
		{
			var freeshippingEnabled = "0";
			var freeshipping = _context.Settings.Where(s => s.Cat == "Freight Options" && s.Name == "free_shipping_enabled").FirstOrDefault();
			if(freeshipping != null)
				freeshippingEnabled = freeshipping.Value;
			var freeshippingSetting = _isettings.getFreightSetting();
			return Ok(
				new
				{
					freeshippingEnabled,
					freeshippingSetting
				}
			);
		}

		[HttpGet("shippingmethod")]
		public IActionResult getShippingMethod()
		{
			var list=  _isettings.getShippingMethod();
			return Ok(list);
		}
	}
}