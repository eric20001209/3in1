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
    [Route("{hostId}/mreport/api/SalesItemTop10Report")]
    public class SalesItemTop10ReportController : Controller
    {
        private readonly farroContext _context;

        public SalesItemTop10ReportController(farroContext context)
        {
            this._context = context;
        }
        [HttpGet("Today")]
        public IActionResult GetSalesItemTop10ReportToday([FromQuery] int? branchId, [FromQuery] string category, [FromQuery] string sortBy)
        {
            // Set the filter to "Today"
            var filter = new SalesItemTop10ReportFilterDto();
            filter.SetToday();
            filter.BranchId = branchId;
            filter.SortBy = sortBy;
            filter.Category = category;

            // Return to response
            var listToReturn = GetSalesItemTop10Report(filter);
            return Ok(listToReturn);
        }

        [HttpGet("Yesterday")]
        public IActionResult GetSalesItemTop10ReportYesterday([FromQuery] int? branchId, [FromQuery] string category, [FromQuery] string sortBy)
        {
            // Set the filter to "Yesterday"
            var filter = new SalesItemTop10ReportFilterDto();
            filter.SetYesterday();
            filter.BranchId = branchId;
            filter.SortBy = sortBy;
            filter.Category = category;

            // Return to response
            var listToReturn = GetSalesItemTop10Report(filter);
            return Ok(listToReturn);
        }

        [HttpGet("ThisWeek")]
        public IActionResult GetSalesItemTop10ReportThisWeek([FromQuery] int? branchId, [FromQuery] string category, [FromQuery] string sortBy)
        {
            // Set the filter to "ThisWeek"
            var filter = new SalesItemTop10ReportFilterDto();
            filter.SetThisWeek();
            filter.BranchId = branchId;
            filter.SortBy = sortBy;
            filter.Category = category;

            // Return to response
            var listToReturn = GetSalesItemTop10Report(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastWeek")]
        public IActionResult GetSalesItemTop10ReportLastWeek([FromQuery] int? branchId, [FromQuery] string category, [FromQuery] string sortBy)
        {
            // Set the filter to "LastWeek"
            var filter = new SalesItemTop10ReportFilterDto();
            filter.SetLastWeek();
            filter.BranchId = branchId;
            filter.SortBy = sortBy;
            filter.Category = category;

            // Return to response
            var listToReturn = GetSalesItemTop10Report(filter);
            return Ok(listToReturn);
        }

        [HttpGet("ThisMonth")]
        public IActionResult GetSalesItemTop10ReportThisMonth([FromQuery] int? branchId, [FromQuery] string category, [FromQuery] string sortBy)
        {
            // Set the filter to "ThisMonth"
            var filter = new SalesItemTop10ReportFilterDto();
            filter.SetThisMonth();
            filter.BranchId = branchId;
            filter.SortBy = sortBy;
            filter.Category = category;

            // Return to response
            var listToReturn = GetSalesItemTop10Report(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastMonth")]
        public IActionResult GetSalesItemTop10ReportLastMonth([FromQuery] int? branchId, [FromQuery] string category, [FromQuery] string sortBy)
        {
            // Set the filter to "LastMonth"
            var filter = new SalesItemTop10ReportFilterDto();
            filter.SetLastMonth();
            filter.BranchId = branchId;
            filter.SortBy = sortBy;
            filter.Category = category;

            // Return to response
            var listToReturn = GetSalesItemTop10Report(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastThreeMonths")]
        public IActionResult GetSalesItemTop10ReportLastThreeMonths([FromQuery] int? branchId, [FromQuery] string category, [FromQuery] string sortBy)
        {
            // Set the filter to "LastThreeMonths"
            var filter = new SalesItemTop10ReportFilterDto();
            filter.SetLastThreeMonths();
            filter.BranchId = branchId;
            filter.SortBy = sortBy;
            filter.Category = category;

            // Return to response
            var listToReturn = GetSalesItemTop10Report(filter);
            return Ok(listToReturn);
        }

        [HttpGet("DateRange")]
        public IActionResult GetSalesItemTop10ReportDateRange([FromQuery] DateTime startDateTime,
            [FromQuery] DateTime endDateTime, [FromQuery] int? branchId, [FromQuery] string category, [FromQuery] string sortBy)
        {
            // Validate user inpute
            if (startDateTime == null || endDateTime == null)
            {
                return BadRequest();
            }

            // Set the filter to "DateRange"
            var filter = new SalesItemTop10ReportFilterDto
            {
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                BranchId = branchId,
                SortBy = sortBy,
                Category = category
            };

            // Return to response
            var listToReturn = GetSalesItemTop10Report(filter);
            return Ok(listToReturn);
        }

        private List<SalesItemTop10ReportDto> GetSalesItemTop10Report(SalesItemTop10ReportFilterDto filter)
        {
            // Set NoTracking for ChangeTracker
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Join invoice, sales, and code_relations tables
            var invoiceGroups = _context.Invoice
                .Where(i => i.CommitDate >= filter.StartDateTime)
                .Where(i => i.CommitDate < filter.EndDateTime)
                // LINQ to SQL Leaky abstractions pitfull
                //.Where(i => filter.BranchId.HasValue ? (i.Daily == filter.BranchId.Value) : true)
                .Where(i => filter.BranchId.HasValue ? (i.Branch == filter.BranchId.GetValueOrDefault()) : true)
                .Select(i => new { i.InvoiceNumber, i.Branch})
                .Join(
                    _context.Branch
                        .Where(b => b.Fax.ToLower() != "hidden4mreport")
                        //.Where(b => b.Activated == true)
                        .Select(b => new { b.Id }),
                    i => i.Branch,
                    b => b.Id,
                    (i, b) => new { i.InvoiceNumber}
                )
                .Join(
                    _context.Sales
                        .Select(s => new { s.InvoiceNumber, s.Code, s.Quantity, s.CommitPrice, s.SupplierPrice, s.Name }),
                    i => i.InvoiceNumber,
                    s => s.InvoiceNumber,
                    (i, s) => new { s.Code, s.Quantity, s.CommitPrice, s.SupplierPrice, s.Name, SalesName = s.Name }
                )
                //.Join(
                //    _context.CodeRelations
                //        .Where(c => c.Code > 1020 || c.Code < 1001)
                //        .Where(c => filter.Category == null ? true : (c.Cat == filter.Category))
                //        .Select(c => new { c.Code, c.NameCn, c.Name })
                //        .ToList(),
                //    i => i.Code,
                //    c => c.Code,
                //    (i, c) => new { i.Quantity, i.CommitPrice, i.SupplierPrice, Name = c.NameCn, Description = c.Name, SalesName = i.Name}
                //)
                .GroupBy(i => i.Name);

            // Apply sales Daily report logic
            var resultList = new List<SalesItemTop10ReportDto>();

            foreach (var group in invoiceGroups)
            {
                resultList.Add(new SalesItemTop10ReportDto()
                {
                    Description = group.Key,
                    Name = group.FirstOrDefault().Name,
                    AmountWithGST = Math.Round(group
                        .Sum(i => (decimal)(i.CommitPrice * (decimal)i.Quantity * 1.15m))
                        , 2),
                    ProfitWithGST = Math.Round(group
                        .Sum(i => (decimal)((i.CommitPrice - i.SupplierPrice) * (decimal)i.Quantity * 1.15m))
                        , 2),
                    ItemQuantity = Math.Round(group
                        .Where(i => !i.SalesName.Contains("Discount Given"))
                        .Sum(i => i.Quantity)
                        , 2)
                });
            }

            if(filter.SortBy == null || filter.SortBy.ToLower() == "quantity")
            {
                resultList = resultList.OrderByDescending(r => r.ItemQuantity).Take(10).ToList();
            }
            else if (filter.SortBy.ToLower() == "amount")
            {
                resultList = resultList.OrderByDescending(r => r.AmountWithGST).Take(10).ToList();
            }
            else if (filter.SortBy.ToLower() == "profit")
            {
                resultList = resultList.OrderByDescending(r => r.ProfitWithGST).Take(10).ToList();
            }
            else
            {
                resultList = resultList.OrderByDescending(r => r.ItemQuantity).Take(10).ToList();
            }

            return resultList;
        }
    }
}
