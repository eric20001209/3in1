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
    [Route("{hostId}/api/SalesItemCategoryReport")]
    public class SalesItemCategoryReportController : Controller
    {
        private readonly farroContext _context;

        public SalesItemCategoryReportController(farroContext context)
        {
            this._context = context;
        }
        [HttpGet("Today")]
        public IActionResult GetSalesItemCategoryReportToday([FromQuery] int? branchId, [FromQuery] string category)
        {
            // Set the filter to "Today"
            var filter = new SalesItemCategoryReportFilterDto();
            filter.SetToday();
            filter.BranchId = branchId;
            filter.Category = category;

            // Return to response
            var listToReturn = GetSalesItemCategoryReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("Yesterday")]
        public IActionResult GetSalesItemCategoryReportYesterday([FromQuery] int? branchId, [FromQuery] string category)
        {
            // Set the filter to "Yesterday"
            var filter = new SalesItemCategoryReportFilterDto();
            filter.SetYesterday();
            filter.BranchId = branchId;
            filter.Category = category;

            // Return to response
            var listToReturn = GetSalesItemCategoryReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("ThisWeek")]
        public IActionResult GetSalesItemCategoryReportThisWeek([FromQuery] int? branchId, [FromQuery] string category)
        {
            // Set the filter to "ThisWeek"
            var filter = new SalesItemCategoryReportFilterDto();
            filter.SetThisWeek();
            filter.BranchId = branchId;
            filter.Category = category;

            // Return to response
            var listToReturn = GetSalesItemCategoryReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastWeek")]
        public IActionResult GetSalesItemCategoryReportLastWeek([FromQuery] int? branchId, [FromQuery] string category)
        {
            // Set the filter to "LastWeek"
            var filter = new SalesItemCategoryReportFilterDto();
            filter.SetLastWeek();
            filter.BranchId = branchId;
            filter.Category = category;

            // Return to response
            var listToReturn = GetSalesItemCategoryReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("ThisMonth")]
        public IActionResult GetSalesItemCategoryReportThisMonth([FromQuery] int? branchId, [FromQuery] string category)
        {
            // Set the filter to "ThisMonth"
            var filter = new SalesItemCategoryReportFilterDto();
            filter.SetThisMonth();
            filter.BranchId = branchId;
            filter.Category = category;

            // Return to response
            var listToReturn = GetSalesItemCategoryReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastMonth")]
        public IActionResult GetSalesItemCategoryReportLastMonth([FromQuery] int? branchId, [FromQuery] string category)
        {
            // Set the filter to "LastMonth"
            var filter = new SalesItemCategoryReportFilterDto();
            filter.SetLastMonth();
            filter.BranchId = branchId;
            filter.Category = category;

            // Return to response
            var listToReturn = GetSalesItemCategoryReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastThreeMonths")]
        public IActionResult GetSalesItemCategoryReportLastThreeMonths([FromQuery] int? branchId, [FromQuery] string category)
        {
            // Set the filter to "LastThreeMonths"
            var filter = new SalesItemCategoryReportFilterDto();
            filter.SetLastThreeMonths();
            filter.BranchId = branchId;
            filter.Category = category;

            // Return to response
            var listToReturn = GetSalesItemCategoryReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("DateRange")]
        public IActionResult GetSalesItemCategoryReportDateRange([FromQuery] DateTime startDateTime,
            [FromQuery] DateTime endDateTime, [FromQuery] int? branchId, [FromQuery] string category)
        {
            // Validate user inpute
            if (startDateTime == null || endDateTime == null)
            {
                return BadRequest();
            }

            // Set the filter to "DateRange"
            var filter = new SalesItemCategoryReportFilterDto
            {
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                BranchId = branchId,
                Category = category
            };

            // Return to response
            var listToReturn = GetSalesItemCategoryReport(filter);
            return Ok(listToReturn);
        }

        private List<SalesItemCategoryReportDto> GetSalesItemCategoryReport(SalesItemCategoryReportFilterDto filter)
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
                .Select(i => new { i.InvoiceNumber, i.Branch })
                .Join(
                    _context.Branch
                        .Where(b => b.Fax.ToLower() != "hidden4mreport")
                        //.Where(b => b.Activated == true)
                        .Select(b => new { b.Id }),
                    i => i.Branch,
                    b => b.Id,
                    (i, b) => new { i.InvoiceNumber }
                )
                .Join(
                    _context.Sales
                        .Where(s => filter.Category == null ? true : (s.Cat == filter.Category))
                        .Select(s => new { s.InvoiceNumber, s.Code, s.Name, s.Quantity, s.CommitPrice, s.SupplierPrice }),
                    i => i.InvoiceNumber,
                    s => s.InvoiceNumber,
                    (i, s) => new { s.Code, s.Name, s.Quantity, s.CommitPrice, s.SupplierPrice }
                )
                //.Join(
                //    _context.CodeRelations
                //        .Where(c => filter.Category == null ? true : (c.Cat == filter.Category))
                //        .Select(c => new { c.Code, c.NameCn })
                //        .ToList(),
                //    i => i.Code,
                //    c => c.Code,
                //    (i, c) => new { i.Quantity, i.CommitPrice, i.SupplierPrice, Name = c.NameCn }
                //)
                .GroupBy(i => i.Code);

            // Apply sales Daily report logic
            var resultList = new List<SalesItemCategoryReportDto>();

            foreach (var group in invoiceGroups)
            {
                resultList.Add(new SalesItemCategoryReportDto()
                {
                    Name = _context.CodeRelations
                        .FirstOrDefault(c => c.Code == group.Key)?.Name ?? $"Deleted Item({group.Key})",
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

            return resultList.OrderByDescending(r => r.ItemQuantity).ToList();
        }
    }
}
