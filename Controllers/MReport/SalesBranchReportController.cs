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
    [Route("{hostId}/api/SalesBranchReport")]
    public class SalesBranchReportController : Controller
    {
        private readonly farroContext _context;

        public SalesBranchReportController(farroContext context)
        {
            this._context = context;
        }
        [HttpGet("Today")]
        public IActionResult GetSalesBranchReportToday()
        {
            // Set the filter to "Today"
            var filter = new SalesBranchReportFilterDto();
            filter.SetToday();

            // Return to response
            var listToReturn = GetSalesBranchReport<SalesBranchReportDto>(filter);
            return Ok(listToReturn);
        }

        [HttpGet("Yesterday")]
        public IActionResult GetSalesBranchReportYesterday()
        {
            // Set the filter to "Yesterday"
            var filter = new SalesBranchReportFilterDto();
            filter.SetYesterday();

            // Return to response
            var listToReturn = GetSalesBranchReport<SalesBranchReportDto>(filter);
            return Ok(listToReturn);
        }

        [HttpGet("ThisWeek")]
        public IActionResult GetSalesBranchReportThisWeek()
        {
            // Set the filter to "ThisWeek"
            var filter = new SalesBranchReportFilterDto();
            filter.SetThisWeek();

            // Return to response
            var listToReturn = GetSalesBranchReport<SalesBranchReportDto>(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastWeek")]
        public IActionResult GetSalesBranchReportLastWeek()
        {
            // Set the filter to "LastWeek"
            var filter = new SalesBranchReportFilterDto();
            filter.SetLastWeek();

            // Return to response
            var listToReturn = GetSalesBranchReport<SalesBranchReportDto>(filter);
            return Ok(listToReturn);
        }

        [HttpGet("ThisMonth")]
        public IActionResult GetSalesBranchReportThisMonth()
        {
            // Set the filter to "ThisMonth"
            var filter = new SalesBranchReportFilterDto();
            filter.SetThisMonth();

            // Return to response
            var listToReturn = GetSalesBranchReport<SalesBranchReportDto>(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastMonth")]
        public IActionResult GetSalesBranchReportLastMonth()
        {
            // Set the filter to "LastMonth"
            var filter = new SalesBranchReportFilterDto();
            filter.SetLastMonth();

            // Return to response
            var listToReturn = GetSalesBranchReport<SalesBranchReportDto>(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastThreeMonths")]
        public IActionResult GetSalesBranchReportLastThreeMonths()
        {
            // Set the filter to "LastThreeMonths"
            var filter = new SalesBranchReportFilterDto();
            filter.SetLastThreeMonths();

            // Return to response
            var listToReturn = GetSalesBranchReport<SalesBranchReportDto>(filter);
            return Ok(listToReturn);
        }

        [HttpGet("DateRange")]
        public IActionResult GetSalesBranchReportDateRange([FromQuery] DateTime startDateTime, 
            [FromQuery] DateTime endDateTime)
        {
            // Validate user inpute
            if(startDateTime == null || endDateTime == null)
            {
                return BadRequest();
            }

            // Set the filter to "DateRange"
            var filter = new SalesBranchReportFilterDto
            {
                StartDateTime = startDateTime,
                EndDateTime = endDateTime
            };

            // Return to response
            var listToReturn = GetSalesBranchReport<SalesBranchReportDto>(filter);
            return Ok(listToReturn);
        }

        public List<T> GetSalesBranchReport<T>(IBranchFilter filter) where T : ISalesBranchReportDto, new()
        {
            // Set NoTracking for ChangeTracker
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Join invoice, branch, and sales tables
            var invoiceGroups = _context.Invoice
                .Where(i => filter.BranchId.HasValue ? filter.BranchId.GetValueOrDefault() == i.Branch : true)
                .Where(i => i.CommitDate >= filter.StartDateTime)
                .Where(i => i.CommitDate < filter.EndDateTime)
                .Select(i => new { i.Total, i.InvoiceNumber, BranchId = i.Branch })
                .Join(
                    _context.Branch
                        .Where(b => b.Fax.ToLower() != "hidden4mreport")
                        //.Where(b => b.Activated == true)
                        .Select(b => new { b.Id, b.Name }),
                    i => i.BranchId,
                    b => b.Id,
                    (i, b) => new { i.Total, i.InvoiceNumber, BranchName = b.Name }
                )
                .Join(
                    _context.Sales
                        .Select(s => new { s.InvoiceNumber, s.Code, s.Quantity, s.CommitPrice, s.SupplierPrice }),
                    i => i.InvoiceNumber,
                    s => s.InvoiceNumber,
                    (i, s) => new {i.InvoiceNumber, i.Total, i.BranchName, s.Code, s.Quantity, s.CommitPrice, s.SupplierPrice}
                )
                .GroupBy(i => i.BranchName);

            // Apply sales branch report logic 
            var resultList = new List<T>();

            foreach (var group in invoiceGroups)
            {
                resultList.Add(new T
                {
                    BranchName = group.Key,
                    TotalWithGST = Math.Round((decimal)group
                        .Select(i => new { i.InvoiceNumber, i.Total })
                        .Distinct()
                        .Sum(i => i.Total), 2),
                    ProfitWithGST = Math.Round(group
                        .Sum(i => (decimal)((i.CommitPrice - i.SupplierPrice) * (decimal)i.Quantity * 1.15m))
                        , 2),
                    InvoiceQuantity = group
                        .Select(i => new { i.InvoiceNumber, i.Total })
                        .Distinct()
                        .Count()
                });
            }

            return resultList;
        }
    }
}
