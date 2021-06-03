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
    [Route("{hostId}/api/SalesItemBranchReport")]
    public class SalesItemBranchReportController : Controller
    {
        private readonly farroContext _context;

        public SalesItemBranchReportController(farroContext context)
        {
            this._context = context;
        }
        [HttpGet("Today")]
        public IActionResult GetSalesItemBranchReportToday([FromQuery] int? branchId, [FromQuery] int? code)
        {
            // input validation
            if (code == null)
            {
                return BadRequest("Code must not be null; you must input a code");
            }
            // Set the filter to "Today"
            var filter = new SalesItemBranchReportFilterDto();
            filter.SetToday();
            filter.Code = code;

            // Return to response
            var listToReturn = GetSalesItemBranchReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("Yesterday")]
        public IActionResult GetSalesItemBranchReportYesterday([FromQuery] int? branchId, [FromQuery] int? code)
        {
            // input validation
            if (code == null)
            {
                return BadRequest("Code must not be null; you must input a code");
            }

            // Set the filter to "Yesterday"
            var filter = new SalesItemBranchReportFilterDto();
            filter.SetYesterday();
            filter.Code = code;

            // Return to response
            var listToReturn = GetSalesItemBranchReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("ThisWeek")]
        public IActionResult GetSalesItemBranchReportThisWeek([FromQuery] int? branchId, [FromQuery] int? code)
        {
            // input validation
            if (code == null)
            {
                return BadRequest("Code must not be null; you must input a code");
            }

            // Set the filter to "ThisWeek"
            var filter = new SalesItemBranchReportFilterDto();
            filter.SetThisWeek();
            filter.Code = code;

            // Return to response
            var listToReturn = GetSalesItemBranchReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastWeek")]
        public IActionResult GetSalesItemBranchReportLastWeek([FromQuery] int? branchId, [FromQuery] int? code)
        {
            // input validation
            if (code == null)
            {
                return BadRequest("Code must not be null; you must input a code");
            }

            // Set the filter to "LastWeek"
            var filter = new SalesItemBranchReportFilterDto();
            filter.SetLastWeek();
            filter.Code = code;

            // Return to response
            var listToReturn = GetSalesItemBranchReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("ThisMonth")]
        public IActionResult GetSalesItemBranchReportThisMonth([FromQuery] int? branchId, [FromQuery] int? code)
        {
            // input validation
            if (code == null)
            {
                return BadRequest("Code must not be null; you must input a code");
            }

            // Set the filter to "ThisMonth"
            var filter = new SalesItemBranchReportFilterDto();
            filter.SetThisMonth();
            filter.Code = code;

            // Return to response
            var listToReturn = GetSalesItemBranchReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastMonth")]
        public IActionResult GetSalesItemBranchReportLastMonth([FromQuery] int? branchId, [FromQuery] int? code)
        {
            // input validation
            if (code == null)
            {
                return BadRequest("Code must not be null; you must input a code");
            }

            // Set the filter to "LastMonth"
            var filter = new SalesItemBranchReportFilterDto();
            filter.SetLastMonth();
            filter.Code = code;

            // Return to response
            var listToReturn = GetSalesItemBranchReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastThreeMonths")]
        public IActionResult GetSalesItemBranchReportLastThreeMonths([FromQuery] int? branchId, [FromQuery] int? code)
        {
            // input validation
            if (code == null)
            {
                return BadRequest("Code must not be null; you must input a code");
            }

            // Set the filter to "LastThreeMonths"
            var filter = new SalesItemBranchReportFilterDto();
            filter.SetLastThreeMonths();
            filter.Code = code;

            // Return to response
            var listToReturn = GetSalesItemBranchReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("DateRange")]
        public IActionResult GetSalesItemBranchReportDateRange([FromQuery] DateTime startDateTime,
            [FromQuery] DateTime endDateTime, [FromQuery] int? branchId, [FromQuery] int? code)
        {
            // input validation
            if (code == null)
            {
                return BadRequest("Code must not be null; you must input a code");
            }

            if (startDateTime == null || endDateTime == null)
            {
                return BadRequest();
            }

            // Set the filter to "DateRange"
            var filter = new SalesItemBranchReportFilterDto
            {
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                Code = code
            };

            // Return to response
            var listToReturn = GetSalesItemBranchReport(filter);
            return Ok(listToReturn);
        }

        private List<SalesItemBranchReportDto> GetSalesItemBranchReport(SalesItemBranchReportFilterDto filter)
        {
            // Set NoTracking for ChangeTracker
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Join invoice, and sales tables
            var invoiceGroups = _context.Invoice
                .Where(i => i.CommitDate >= filter.StartDateTime)
                .Where(i => i.CommitDate < filter.EndDateTime)
                .Select(i => new { i.InvoiceNumber, BranchId = i.Branch })
                .Join(
                    _context.Branch
                        .Where(b => b.Fax.ToLower() != "hidden4mreport")
                        //.Where(b => b.Activated == true)
                        .Select(b => new { b.Id, b.Name }),
                    i => i.BranchId,
                    b => b.Id,
                    (i, b) => new { i.InvoiceNumber, BranchName = b.Name }
                )
                .Join(
                    _context.Sales
                        .Where(s => s.Code == filter.Code)
                        .Select(s => new { s.InvoiceNumber, s.Quantity, s.CommitPrice, s.SupplierPrice, s.Name }),
                    i => i.InvoiceNumber,
                    s => s.InvoiceNumber,
                    (i, s) => new { i.BranchName, s.Quantity, s.CommitPrice, s.SupplierPrice, s.Name }
                )
                .GroupBy(i => i.BranchName);

            // Apply sales Daily report logic
            var resultList = new List<SalesItemBranchReportDto>();

            foreach (var group in invoiceGroups)
            {
                resultList.Add(new SalesItemBranchReportDto()
                {
                    BranchName = group.Key,
                    AmountWithGST = Math.Round(group
                        .Sum(i => (decimal)(i.CommitPrice * (decimal)i.Quantity * 1.15m))
                        , 2),
                    ProfitWithGST = Math.Round(group
                        .Sum(i => (decimal)((i.CommitPrice - i.SupplierPrice) * (decimal)i.Quantity * 1.15m))
                        , 2),
                    ItemQuantity = Math.Round(group
                        .Where(i => !i.Name.Contains("Discount Given"))
                        .Sum(i => i.Quantity)
                        , 2)
                });
            }

            return resultList;
        }
    }
}
