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
    [Route("{hostId}/api/WholesaleItemReport")]
    public class WholesaleItemReportController : Controller
    {
        private readonly farroContext _context;

        public WholesaleItemReportController(farroContext context)
        {
            this._context = context;
        }
        [HttpGet("Today")]
        public IActionResult GetWholesaleItemReportToday(
            [FromQuery] int? dealerId, [FromQuery] string cat,
            [FromQuery] string sCat, [FromQuery] string sSCat,
            [FromQuery] string keyWord)
        {
            // Set the filter to "Today"
            var filter = new WholesaleItemReportFilterDto();
            filter.SetToday();
            filter.DealerId = dealerId;
            filter.Cat = cat;
            filter.SCat = sCat;
            filter.SSCat = sSCat;
            filter.Keyword = keyWord;
            
            // Return to response
            var listToReturn = GetWholesaleItemReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("Yesterday")]
        public IActionResult GetWholesaleItemReportYesterday(
            [FromQuery] int? dealerId, [FromQuery] string cat,
            [FromQuery] string sCat, [FromQuery] string sSCat,
            [FromQuery] string keyWord)
        {
            // Set the filter to "Yesterday"
            var filter = new WholesaleItemReportFilterDto();
            filter.SetYesterday();
            filter.DealerId = dealerId;
            filter.Cat = cat;
            filter.SCat = sCat;
            filter.SSCat = sSCat;
            filter.Keyword = keyWord;

            // Return to response
            var listToReturn = GetWholesaleItemReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("ThisWeek")]
        public IActionResult GetWholesaleItemReportThisWeek(
            [FromQuery] int? dealerId, [FromQuery] string cat,
            [FromQuery] string sCat, [FromQuery] string sSCat,
            [FromQuery] string keyWord)
        {
            // Set the filter to "ThisWeek"
            var filter = new WholesaleItemReportFilterDto();
            filter.SetThisWeek();
            filter.DealerId = dealerId;
            filter.Cat = cat;
            filter.SCat = sCat;
            filter.SSCat = sSCat;
            filter.Keyword = keyWord;
            
            // Return to response
            var listToReturn = GetWholesaleItemReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastWeek")]
        public IActionResult GetWholesaleItemReportLastWeek(
            [FromQuery] int? dealerId, [FromQuery] string cat,
            [FromQuery] string sCat, [FromQuery] string sSCat,
            [FromQuery] string keyWord)
        {
            // Set the filter to "LastWeek"
            var filter = new WholesaleItemReportFilterDto();
            filter.SetLastWeek();
            filter.DealerId = dealerId;
            filter.Cat = cat;
            filter.SCat = sCat;
            filter.SSCat = sSCat;
            filter.Keyword = keyWord;
            
            // Return to response
            var listToReturn = GetWholesaleItemReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("ThisMonth")]
        public IActionResult GetWholesaleItemReportThisMonth(
            [FromQuery] int? dealerId, [FromQuery] string cat,
            [FromQuery] string sCat, [FromQuery] string sSCat,
            [FromQuery] string keyWord)
        {
            // Set the filter to "ThisMonth"
            var filter = new WholesaleItemReportFilterDto();
            filter.SetThisMonth();
            filter.DealerId = dealerId;
            filter.Cat = cat;
            filter.SCat = sCat;
            filter.SSCat = sSCat;
            filter.Keyword = keyWord;

            // Return to response
            var listToReturn = GetWholesaleItemReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastMonth")]
        public IActionResult GetWholesaleItemReportLastMonth(
            [FromQuery] int? dealerId, [FromQuery] string cat,
            [FromQuery] string sCat, [FromQuery] string sSCat,
            [FromQuery] string keyWord)
        {
            // Set the filter to "LastMonth"
            var filter = new WholesaleItemReportFilterDto();
            filter.SetLastMonth();
            filter.DealerId = dealerId;
            filter.Cat = cat;
            filter.SCat = sCat;
            filter.SSCat = sSCat;
            filter.Keyword = keyWord;

            // Return to response
            var listToReturn = GetWholesaleItemReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastThreeMonths")]
        public IActionResult GetWholesaleItemReportLastThreeMonths(
            [FromQuery] int? dealerId, [FromQuery] string cat,
            [FromQuery] string sCat, [FromQuery] string sSCat,
            [FromQuery] string keyWord)
        {
            // Set the filter to "LastThreeMonths"
            var filter = new WholesaleItemReportFilterDto();
            filter.SetLastThreeMonths();
            filter.DealerId = dealerId;
            filter.Cat = cat;
            filter.SCat = sCat;
            filter.SSCat = sSCat;
            filter.Keyword = keyWord;

            // Return to response
            var listToReturn = GetWholesaleItemReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("DateRange")]
        public IActionResult GetWholesaleItemReportDateRange(
            [FromQuery] DateTime startDateTime, [FromQuery] DateTime endDateTime,
            [FromQuery] int? dealerId, [FromQuery] string cat,
            [FromQuery] string sCat, [FromQuery] string sSCat,
            [FromQuery] string keyWord)
        {
            // Set the filter to "Today"
            var filter = new WholesaleItemReportFilterDto();
            filter.StartDateTime = startDateTime;
            filter.EndDateTime = endDateTime;
            filter.DealerId = dealerId;
            filter.Cat = cat;
            filter.SCat = sCat;
            filter.SSCat = sSCat;
            filter.Keyword = keyWord;
            
            // Return to response
            var listToReturn = GetWholesaleItemReport(filter);
            return Ok(listToReturn);
        }

        private List<WholesaleItemReportDto> GetWholesaleItemReport(WholesaleItemReportFilterDto filter)
        {
            // Set NoTracking for ChangeTracker
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Join invoice, and sales tables
            var invoiceGroups = _context.Invoice
                .Where(i => i.Branch == 1)
                .Where(i => filter.DealerId != null ? i.CardId == filter.DealerId.GetValueOrDefault() : true)
                .Where(i => i.CommitDate >= filter.StartDateTime)
                .Where(i => i.CommitDate < filter.EndDateTime)
                .Select(i => new { i.InvoiceNumber })
                .Join(
                    _context.Sales
                        .Select(s => new { s.InvoiceNumber, s.Quantity, s.CommitPrice, s.SupplierPrice, s.Code }),
                    i => i.InvoiceNumber,
                    s => s.InvoiceNumber,
                    (i, s) => new { s.Quantity, s.CommitPrice, s.SupplierPrice, s.Code }
                )
                .Join(
                    _context.CodeRelations
                        .Where(c => filter.Cat != null ? c.Cat == filter.Cat : true)
                        .Where(c => filter.SCat != null ? c.SCat == filter.SCat : true)
                        .Where(c => filter.SSCat != null ? c.SsCat == filter.SSCat : true)
                        .Where(c => filter.Keyword != null ? (c.Name.Contains(filter.Keyword) || c.NameCn.Contains(filter.Keyword)) : true)
                        .Select(c => new { c.Code, c.Name, c.Cat, c.SCat, c.SsCat }).ToList(),
                    i => i.Code,
                    c => c.Code,
                    (i, c) => new { i.Quantity, i.CommitPrice, i.SupplierPrice, i.Code, c.Cat, c.SCat, c.SsCat, c.Name }
                )
                .GroupBy(i => i.Code);

            // Apply sales Daily report logic
            var resultList = new List<WholesaleItemReportDto>();

            foreach (var group in invoiceGroups)
            {
                resultList.Add(new WholesaleItemReportDto()
                {
                    ItemCode = group.Key,
                    Name = group.FirstOrDefault().Name,
                    Cat = group.FirstOrDefault().Cat,
                    SCat = group.FirstOrDefault().SCat,
                    SSCat = group.FirstOrDefault().SsCat,
                    AmountWithGST = Math.Round(group
                        .Sum(i => (decimal)(i.CommitPrice * (decimal)i.Quantity * 1.15m))
                        , 2),
                    ProfitWithGST = Math.Round(group
                        .Sum(i => (decimal)((i.CommitPrice - i.SupplierPrice) * (decimal)i.Quantity * 1.15m))
                        , 2),
                    ItemQuantity = Math.Round(group
                        .Sum(i => i.Quantity)
                        , 2)
                });
            }

            return resultList;
        }
    }
}
