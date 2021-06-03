using FarroAPI.Entities;
using FarroAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Controllers
{
    [Route("{hostId}/api/suppliers")]
    public class SupplierController : Controller
    {
        private readonly farroContext context;

        public SupplierController(farroContext context)
        {
            this.context = context;
        }
        [HttpGet()]
        public async Task<IActionResult> GetSuppliers()
        {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                var supplierList = await context.Card
                    .Where(c => c.Type == 3)
                    .Select(c => new { c.Id, c.TradingName, c.Company })
                    .ToListAsync();

                var listToReturn = new List<SupplierDto>();

                supplierList.ForEach(s =>
                {
                    listToReturn.Add(new SupplierDto
                    {
                        SupplierId = s.Id,
                        TradingName = s.TradingName,
                        LegalName = s.Company
                    });
                });

                return Ok(listToReturn);
            
        }
    }
}
