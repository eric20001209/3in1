using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using eCommerce_API.Data;

namespace eCommerce_API.Controllers
{
    [Route("{hostId}/ecom/api/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        rst374_cloud12Context _context;//= new rst374_cloud12Context();
        public CategoryController(rst374_cloud12Context context)
        {
            _context = context;
        }

        [HttpGet()]
        public IActionResult categoryList([FromQuery] string cat, [FromQuery] string scat, [FromQuery] string sscat)
        {
            var list = _context.CodeRelations
                .Where(c => c.IsWebsiteItem == true || c.OnLineRetail == true && c.Skip == false)
//              .Where(c=>c.Skip == false)
                .Where(c => !String.IsNullOrEmpty(c.Cat))
                .Where(c => cat != null ? c.Cat == cat : true)
                .Where(c => scat != null ? c.SCat == scat : true)
                .Where(c => sscat != null ? c.SsCat == scat : true)
                .Select(c => new
                {
                    c.Cat,
                    c.SCat,
                    c.SsCat
                }).Distinct().ToList();

            return Ok(list);
        }
    }
}