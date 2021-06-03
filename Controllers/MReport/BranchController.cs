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
    [Route("{hostId}/api/branches")]
    public class BranchController : Controller
    {
        private readonly farroContext _context;

        public BranchController(farroContext context)
        {
            this._context = context;
        }
        /// <summary>
        /// Get Branches
        /// </summary>
        /// <returns></returns>
        /// GET api/branches
        [HttpGet()]
        public async Task<IActionResult> GetBranches()
        {
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var listToReturn = await _context.Branch
                .Where(b => b.Fax.ToLower() != "hidden4mreport")
                .Select(b => new BranchDto
                {
                    Id = b.Id,
                    Name = b.Name
                })
                .ToListAsync();

            return Ok(listToReturn);
        }
    }
}
