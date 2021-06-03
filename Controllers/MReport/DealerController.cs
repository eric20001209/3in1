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
    [Route("{hostId}/api/dealers")]
    public class DealerController : Controller
    {
        private readonly farroContext _context;

        public DealerController(farroContext context)
        {
            _context = context;
        }
        [HttpGet()]
        public IActionResult GetDealers()
        {
            // Set NoTracking for ChangeTracker
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var resultList = new List<DealerDto>();

            _context.Card
            .Where(c => c.Type == 3)
            .Select(c => new { c.Id, c.TradingName, c.Company })
            .ToList()
            .ForEach(c => resultList.Add(new DealerDto
                {
                    DealerId = c.Id,
                    TradingName = c.TradingName,
                    LegalName = c.Company
                })
            );

            return Ok(resultList);
        }
    }
}
