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
    [Route("{hostId}/mreport/api/WholesaleCategoryDailyReport")]
    public class WholesaleCategoryDailyReportController : Controller
    {
        private readonly farroContext _context;

        public WholesaleCategoryDailyReportController(farroContext context)
        {
            this._context = context;
        }
        [HttpGet("Today")]
        public IActionResult GetWholesaleCategoryDailyReportToday(
            [FromQuery] int? dealerId, [FromQuery] string cat,
            [FromQuery] string sCat, [FromQuery] string sSCat)
        {
            // Set the filter to "Today"
            var filter = new WholesaleCategoryDailyReportFilterDto();
            filter.SetToday();
            filter.DealerId = dealerId;
            filter.Cat = cat;
            filter.SCat = sCat;
            filter.SSCat = sSCat;

            // Return to response
            var listToReturn = GetWholesaleCategoryDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("Yesterday")]
        public IActionResult GetWholesaleCategoryDailyReportYesterday(
            [FromQuery] int? dealerId, [FromQuery] string cat,
            [FromQuery] string sCat, [FromQuery] string sSCat)
        {
            // Set the filter to "Yesterday"
            var filter = new WholesaleCategoryDailyReportFilterDto();
            filter.SetYesterday();
            filter.DealerId = dealerId;
            filter.Cat = cat;
            filter.SCat = sCat;
            filter.SSCat = sSCat;

            // Return to response
            var listToReturn = GetWholesaleCategoryDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("ThisWeek")]
        public IActionResult GetWholesaleCategoryDailyReportThisWeek(
            [FromQuery] int? dealerId, [FromQuery] string cat,
            [FromQuery] string sCat, [FromQuery] string sSCat)
        {
            // Set the filter to "ThisWeek"
            var filter = new WholesaleCategoryDailyReportFilterDto();
            filter.SetThisWeek();
            filter.DealerId = dealerId;
            filter.Cat = cat;
            filter.SCat = sCat;
            filter.SSCat = sSCat;

            // Return to response
            var listToReturn = GetWholesaleCategoryDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastWeek")]
        public IActionResult GetWholesaleCategoryDailyReportLastWeek(
            [FromQuery] int? dealerId, [FromQuery] string cat,
            [FromQuery] string sCat, [FromQuery] string sSCat)
        {
            // Set the filter to "LastWeek"
            var filter = new WholesaleCategoryDailyReportFilterDto();
            filter.SetLastWeek();
            filter.DealerId = dealerId;
            filter.Cat = cat;
            filter.SCat = sCat;
            filter.SSCat = sSCat;

            // Return to response
            var listToReturn = GetWholesaleCategoryDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("ThisMonth")]
        public IActionResult GetWholesaleCategoryDailyReportThisMonth(
            [FromQuery] int? dealerId, [FromQuery] string cat,
            [FromQuery] string sCat, [FromQuery] string sSCat)
        {
            // Set the filter to "ThisMonth"
            var filter = new WholesaleCategoryDailyReportFilterDto();
            filter.SetThisMonth();
            filter.DealerId = dealerId;
            filter.Cat = cat;
            filter.SCat = sCat;
            filter.SSCat = sSCat;

            // Return to response
            var listToReturn = GetWholesaleCategoryDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastMonth")]
        public IActionResult GetWholesaleCategoryDailyReportLastMonth(
            [FromQuery] int? dealerId, [FromQuery] string cat,
            [FromQuery] string sCat, [FromQuery] string sSCat)
        {
            // Set the filter to "LastMonth"
            var filter = new WholesaleCategoryDailyReportFilterDto();
            filter.SetLastMonth();
            filter.DealerId = dealerId;
            filter.Cat = cat;
            filter.SCat = sCat;
            filter.SSCat = sSCat;

            // Return to response
            var listToReturn = GetWholesaleCategoryDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastThreeMonths")]
        public IActionResult GetWholesaleCategoryDailyReportLastThreeMonths(
            [FromQuery] int? dealerId, [FromQuery] string cat,
            [FromQuery] string sCat, [FromQuery] string sSCat)
        {
            // Set the filter to "LastThreeMonths"
            var filter = new WholesaleCategoryDailyReportFilterDto();
            filter.SetLastThreeMonths();
            filter.DealerId = dealerId;
            filter.Cat = cat;
            filter.SCat = sCat;
            filter.SSCat = sSCat;

            // Return to response
            var listToReturn = GetWholesaleCategoryDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("DateRange")]
        public IActionResult GetWholesaleCategoryDailyReportDateRange(
            [FromQuery] DateTime startDateTime, [FromQuery] DateTime endDateTime,
            [FromQuery] int? dealerId, [FromQuery] string cat,
            [FromQuery] string sCat, [FromQuery] string sSCat)
        {
            // Set the filter to "Today"
            var filter = new WholesaleCategoryDailyReportFilterDto();
            filter.StartDateTime = startDateTime;
            filter.EndDateTime = endDateTime;
            filter.DealerId = dealerId;
            filter.Cat = cat;
            filter.SCat = sCat;
            filter.SSCat = sSCat;

            // Return to response
            var listToReturn = GetWholesaleCategoryDailyReport(filter);
            return Ok(listToReturn);
        }

        private List<WholesaleCategoryDailyReportDto> GetWholesaleCategoryDailyReport(WholesaleCategoryDailyReportFilterDto filter)
        {
            // Set NoTracking for ChangeTracker
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Join invoice, and sales tables
            var invoiceGroups = _context.Invoice
                .Where(i => i.Branch == 1)
                .Where(i => filter.DealerId != null ? i.CardId == filter.DealerId.GetValueOrDefault() : true)
                .Where(i => i.CommitDate >= filter.StartDateTime)
                .Where(i => i.CommitDate < filter.EndDateTime)
                .Select(i => new { i.InvoiceNumber, i.CommitDate })
                .Join(
                    _context.Sales
                        .Select(s => new { s.InvoiceNumber, s.Quantity, s.CommitPrice, s.SupplierPrice, s.Code }),
                    i => i.InvoiceNumber,
                    s => s.InvoiceNumber,
                    (i, s) => new { i.CommitDate, s.Quantity, s.CommitPrice, s.SupplierPrice, s.Code }
                )
                .Join(
                    _context.CodeRelations
                        .Where(c => filter.Cat != null ? c.Cat == filter.Cat : true)
                        .Where(c => filter.SCat != null ? c.SCat == filter.SCat : true)
                        .Where(c => filter.SSCat != null ? c.SsCat == filter.SSCat : true)
                        .Select(c => new { c.Code, c.Cat, c.SCat, c.SsCat }).ToList(),
                    i => i.Code,
                    c => c.Code,
                    (i, c) => new { i.Quantity, i.CommitPrice, i.SupplierPrice, i.CommitDate }
                )
                .GroupBy(i => i.CommitDate.Date);

            // Apply sales Daily report logic
            var resultList = new List<WholesaleCategoryDailyReportDto>();

            foreach (var group in invoiceGroups)
            {
                resultList.Add(new WholesaleCategoryDailyReportDto()
                {
                    StartDate = group.Key,
                    EndDate = group.Key.AddDays(1),
                    AmountWithGST = Math.Round(group
                        .Sum(i => (decimal)(i.CommitPrice * (decimal)i.Quantity * 1.15m))
                        , 2),
                    ProfitWithGST = Math.Round(group
                        .Sum(i => (decimal)((i.CommitPrice - i.SupplierPrice) * (decimal)i.Quantity * 1.15m))
                        , 2)
                });
            }

            return resultList.OrderBy(r => r.StartDate).ToList();
        }
    }
}
