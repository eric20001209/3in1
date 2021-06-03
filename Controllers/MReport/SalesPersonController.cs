using Microsoft.AspNetCore.Mvc;
using FarroAPI.Entities;
using FarroAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Controllers
{
    [Route("{hostId}/api/salespeople")]
    public class SalesPersonController : Controller
    {
        private readonly farroContext _context;
        public SalesPersonController(farroContext context)
        {
            this._context = context;
        }
        [HttpGet()]
        public IActionResult GetSalesPeople()
        {
            

            var salesPeople = _context.Invoice.Select(i => new { i.Branch, i.Sales }).Distinct();

            var resultList = new List<SalesPersonDto>();

            foreach (var salesPerson in salesPeople)
            {
                resultList.Add(new SalesPersonDto
                {
                    BranchId = salesPerson.Branch.GetValueOrDefault(),
                    Name = salesPerson.Sales
                });
            }

            return Ok(resultList.OrderBy(o => o.BranchId).ToList());
        }
    }
}
