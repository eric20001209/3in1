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
    [Route("{hostId}/api/SalesDailyReport")]
    public class SalesDailyReportController : Controller
    {
        private readonly farroContext _context;

        public SalesDailyReportController(farroContext context)
        {
            this._context = context;
        }
        [HttpGet("Today")]
        public IActionResult GetSalesDailyReportToday([FromQuery] int? branchId)
        {
            // Set the filter to "Today"
            var filter = new SalesDailyReportFilterDto();
            filter.SetToday();
            filter.BranchId = branchId;

            // Return to response
            var listToReturn = GetSalesDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("Yesterday")]
        public IActionResult GetSalesDailyReportYesterday([FromQuery] int? branchId)
        {
            // Set the filter to "Yesterday"
            var filter = new SalesDailyReportFilterDto();
            filter.SetYesterday();
            filter.BranchId = branchId;

            // Return to response
            var listToReturn = GetSalesDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("ThisWeek")]
        public IActionResult GetSalesDailyReportThisWeek([FromQuery] int? branchId)
        {
            // Set the filter to "ThisWeek"
            var filter = new SalesDailyReportFilterDto();
            filter.SetThisWeek();
            filter.BranchId = branchId;

            // Return to response
            var listToReturn = GetSalesDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastWeek")]
        public IActionResult GetSalesDailyReportLastWeek([FromQuery] int? branchId)
        {
            // Set the filter to "LastWeek"
            var filter = new SalesDailyReportFilterDto();
            filter.SetLastWeek();
            filter.BranchId = branchId;

            // Return to response
            var listToReturn = GetSalesDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("ThisMonth")]
        public IActionResult GetSalesDailyReportThisMonth([FromQuery] int? branchId)
        {
            // Set the filter to "ThisMonth"
            var filter = new SalesDailyReportFilterDto();
            filter.SetThisMonth();
            filter.BranchId = branchId;

            // Return to response
            var listToReturn = GetSalesDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastMonth")]
        public IActionResult GetSalesDailyReportLastMonth([FromQuery] int? branchId)
        {
            // Set the filter to "LastMonth"
            var filter = new SalesDailyReportFilterDto();
            filter.SetLastMonth();
            filter.BranchId = branchId;

            // Return to response
            var listToReturn = GetSalesDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastThreeMonths")]
        public IActionResult GetSalesDailyReportLastThreeMonths([FromQuery] int? branchId)
        {
            // Set the filter to "LastThreeMonths"
            var filter = new SalesDailyReportFilterDto();
            filter.SetLastThreeMonths();
            filter.BranchId = branchId;

            // Return to response
            var listToReturn = GetSalesDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("DateRange")]
        public IActionResult GetSalesDailyReportDateRange([FromQuery] DateTime startDateTime,
            [FromQuery] DateTime endDateTime, [FromQuery] int? branchId)
        {
            // Validate user inpute
            if (startDateTime == null || endDateTime == null)
            {
                return BadRequest();
            }

            // Set the filter to "DateRange"
            var filter = new SalesDailyReportFilterDto
            {
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                BranchId = branchId
            };

            // Return to response
            var listToReturn = GetSalesDailyReport(filter);
            return Ok(listToReturn);
        }

        private List<SalesDailyReportDto> GetSalesDailyReport(SalesDailyReportFilterDto filter)
        {
            // Set NoTracking for ChangeTracker
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Join invoice, and sales tables
            var invoiceGroups = _context.Invoice
                .Where(i => i.CommitDate >= filter.StartDateTime)
                .Where(i => i.CommitDate < filter.EndDateTime)
                // LINQ to SQL Leaky abstractions pitfull
                //.Where(i => filter.BranchId.HasValue ? (i.Daily == filter.BranchId.Value) : true)
                .Where(i => filter.BranchId.HasValue ? (i.Branch == filter.BranchId.GetValueOrDefault()) : true)
                .Select(i => new { i.InvoiceNumber, i.Total, i.CommitDate, i.Branch })
                .Join(
                    _context.Branch
                        .Where(b => b.Fax.ToLower() != "hidden4mreport")
                        //.Where(b => b.Activated == true)
                        .Select(b => new { b.Id }),
                    i => i.Branch,
                    b => b.Id,
                    (i, b) => new { i.InvoiceNumber, i.Total, i.CommitDate }
                )
                .Join(
                    _context.Sales
                    .Select(s => new { s.InvoiceNumber, s.Code, s.Quantity, s.CommitPrice, s.SupplierPrice }),
                    i => i.InvoiceNumber,
                    s => s.InvoiceNumber,
                    (i, s) => new { i.InvoiceNumber, i.Total, i.CommitDate, s.Code, s.Quantity, s.CommitPrice, s.SupplierPrice }
                )
                .GroupBy(i => i.CommitDate.Date);

            // Apply sales Daily report logic
            var resultList = new List<SalesDailyReportDto>();

            foreach (var group in invoiceGroups)
            {
                resultList.Add(new SalesDailyReportDto()
                {
                    StartDate = group.Key,
                    EndDate = group.Key.AddDays(1),
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
