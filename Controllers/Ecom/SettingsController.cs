using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eCommerce_API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eCommerce_API_RST.Controllers
{
	[Route("{hostId}/ecom/api/ecom")]
	[ApiController]
	public class SettingsController : ControllerBase
	{
		private readonly rst374_cloud12Context _context;
		public SettingsController(rst374_cloud12Context context)
		{
			_context = context;
		}

		[HttpGet("banner")]
		public async Task<IActionResult> getEcomBanner()
		{
			var ecomBanners = await _context.EcomBanner.ToListAsync();
			return Ok(ecomBanners);
		}

		[HttpGet("setting")]
		public async Task<IActionResult> getEcomSetting()
		{
			var ecomSetting = await _context.EcomSetting.ToListAsync();
			return Ok(ecomSetting);
		}

		[HttpGet("topmenu")]
		public async Task<IActionResult> getEcomTopMenu()
		{
			var ecomTopMenu = await _context.EcomTopMenu.ToListAsync();
			return Ok(ecomTopMenu);
		}
	}
}
