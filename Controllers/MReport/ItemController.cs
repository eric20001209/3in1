using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FarroAPI.Entities;
using FarroAPI.Models;

namespace FarroAPI.Controllers
{
    [Route("{hostId}/api/items")]
    public class ItemController : Controller
    {
        private readonly farroContext _context;

        public ItemController(farroContext context)
        {
            this._context = context;
        }
        [HttpGet()]
        public async Task<IActionResult> GetItems([FromQuery] int? supplierId, [FromQuery] string category,
            [FromQuery] string subCategory, [FromQuery] string subSubCategory, [FromQuery] string keyword)
        {
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var listToReturn = await _context.CodeRelations
                .Where(c => c.Code > 1020 || c.Code < 1001)
                .Where(c => supplierId.HasValue ? c.Supplier == supplierId.ToString() : true )
                .Where(c => category != null ? c.Cat == category : true)
                .Where(c => subCategory != null ? c.Cat == subCategory : true)
                .Where(c => subSubCategory != null ? c.Cat == subSubCategory : true)
                .Where(c => keyword != null ? (c.Name.Contains(keyword) || c.NameCn.Contains(keyword)) : true)
                .Select(c => new ItemDto {
                    Code = c.Code,
                    Description = c.Name,
                    Name = c.NameCn,
                    Category = c.Cat })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(listToReturn);
        }
    }
}
