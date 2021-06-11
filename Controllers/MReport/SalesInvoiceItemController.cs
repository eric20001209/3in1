using FarroAPI.Entities;
using FarroAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Controllers
{
    [Route("{hostId}/mreport/api/SalesInvoiceItems")]
    public class SalesInvoiceItemController : Controller
    {
        
        private readonly farroContext _context;
        public SalesInvoiceItemController(farroContext context)
        {
            this._context = context;
        }
        [HttpGet()]
        public IActionResult GetSalesInvoiceItems([FromQuery] int invoiceNumber)
        {
            

            var invoiceItemList = _context.Sales
                .Where(s => s.InvoiceNumber == invoiceNumber)
                .Select(s => new { s.Code, s.Name, s.Quantity, s.CommitPrice });

            var resultList = new List<SalesInvoiceItemDto>();

            foreach (var item in invoiceItemList)
            {
                resultList.Add(new SalesInvoiceItemDto
                {
                    ItemCode = item.Code,
                    Description = item.Name,
                    Quantity = item.Quantity,
                    UnitPriceWithGST = (decimal)Math.Round(item.CommitPrice * 1.15m, 2)
                });
            }

            return Ok(resultList);
        }
    }
}
