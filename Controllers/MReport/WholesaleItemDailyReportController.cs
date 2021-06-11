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
    [Route("{hostId}/mreport/api/WholesaleItemDailyReport")]
    public class WholesaleItemDailyReportController : Controller
    {
        private readonly farroContext _context;

        public WholesaleItemDailyReportController(farroContext context)
        {
            this._context = context;
        }
        [HttpGet("Today")]
        public IActionResult GetWholesaleItemDailyReportToday([FromQuery] int? dealerId, [FromQuery] int? code)
        {
            // input validation
            if (code == null)
            {
                return BadRequest("Code must not be null; you must input a code");
            }
            // Set the filter to "Today"
            var filter = new WholesaleItemDailyReportFilterDto();
            filter.SetToday();
            filter.DealerId= dealerId;
            filter.Code = code;

            // Return to response
            var listToReturn = GetWholesaleItemDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("Yesterday")]
        public IActionResult GetWholesaleItemDailyReportYesterday([FromQuery] int? dealerId, [FromQuery] int? code)
        {
            // input validation
            if (code == null)
            {
                return BadRequest("Code must not be null; you must input a code");
            }

            // Set the filter to "Yesterday"
            var filter = new WholesaleItemDailyReportFilterDto();
            filter.SetYesterday();
            filter.DealerId= dealerId;
            filter.Code = code;

            // Return to response
            var listToReturn = GetWholesaleItemDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("ThisWeek")]
        public IActionResult GetWholesaleItemDailyReportThisWeek([FromQuery] int? dealerId, [FromQuery] int? code)
        {
            // input validation
            if (code == null)
            {
                return BadRequest("Code must not be null; you must input a code");
            }

            // Set the filter to "ThisWeek"
            var filter = new WholesaleItemDailyReportFilterDto();
            filter.SetThisWeek();
            filter.DealerId= dealerId;
            filter.Code = code;

            // Return to response
            var listToReturn = GetWholesaleItemDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastWeek")]
        public IActionResult GetWholesaleItemDailyReportLastWeek([FromQuery] int? dealerId, [FromQuery] int? code)
        {
            // input validation
            if (code == null)
            {
                return BadRequest("Code must not be null; you must input a code");
            }

            // Set the filter to "LastWeek"
            var filter = new WholesaleItemDailyReportFilterDto();
            filter.SetLastWeek();
            filter.DealerId= dealerId;
            filter.Code = code;

            // Return to response
            var listToReturn = GetWholesaleItemDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("ThisMonth")]
        public IActionResult GetWholesaleItemDailyReportThisMonth([FromQuery] int? dealerId, [FromQuery] int? code)
        {
            // input validation
            if (code == null)
            {
                return BadRequest("Code must not be null; you must input a code");
            }

            // Set the filter to "ThisMonth"
            var filter = new WholesaleItemDailyReportFilterDto();
            filter.SetThisMonth();
            filter.DealerId= dealerId;
            filter.Code = code;

            // Return to response
            var listToReturn = GetWholesaleItemDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastMonth")]
        public IActionResult GetWholesaleItemDailyReportLastMonth([FromQuery] int? dealerId, [FromQuery] int? code)
        {
            // input validation
            if (code == null)
            {
                return BadRequest("Code must not be null; you must input a code");
            }

            // Set the filter to "LastMonth"
            var filter = new WholesaleItemDailyReportFilterDto();
            filter.SetLastMonth();
            filter.DealerId= dealerId;
            filter.Code = code;

            // Return to response
            var listToReturn = GetWholesaleItemDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastThreeMonths")]
        public IActionResult GetWholesaleItemDailyReportLastThreeMonths([FromQuery] int? dealerId, [FromQuery] int? code)
        {
            // input validation
            if (code == null)
            {
                return BadRequest("Code must not be null; you must input a code");
            }

            // Set the filter to "LastThreeMonths"
            var filter = new WholesaleItemDailyReportFilterDto();
            filter.SetLastThreeMonths();
            filter.DealerId= dealerId;
            filter.Code = code;

            // Return to response
            var listToReturn = GetWholesaleItemDailyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("DateRange")]
        public IActionResult GetWholesaleItemDailyReportDateRange([FromQuery] DateTime startDateTime,
            [FromQuery] DateTime endDateTime, [FromQuery] int? dealerId, [FromQuery] int? code)
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
            var filter = new WholesaleItemDailyReportFilterDto
            {
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                DealerId = dealerId,
                Code = code
            };

            // Return to response
            var listToReturn = GetWholesaleItemDailyReport(filter);
            return Ok(listToReturn);
        }

        private List<WholesaleItemDailyReportDto> GetWholesaleItemDailyReport(WholesaleItemDailyReportFilterDto filter)
        {
            // Set NoTracking for ChangeTracker
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Join invoice, and sales tables
            var invoiceGroups = _context.Invoice
                .Where(i => i.Branch == 1)
                .Where(i => i.CommitDate >= filter.StartDateTime)
                .Where(i => i.CommitDate < filter.EndDateTime)
                .Where(i => filter.DealerId.HasValue ? (i.CardId == filter.DealerId.GetValueOrDefault()) : true)
                .Select(i => new { i.InvoiceNumber, i.CommitDate })
                .Join(
                    _context.Sales
                    .Where(s => s.Code == filter.Code)
                    .Select(s => new { s.InvoiceNumber, s.Quantity, s.CommitPrice, s.SupplierPrice }),
                    i => i.InvoiceNumber,
                    s => s.InvoiceNumber,
                    (i, s) => new { i.CommitDate, s.Quantity, s.CommitPrice, s.SupplierPrice }
                )
                .GroupBy(i => i.CommitDate.Date);

            // Apply sales Daily report logic
            var resultList = new List<WholesaleItemDailyReportDto>();

            foreach (var group in invoiceGroups)
            {
                resultList.Add(new WholesaleItemDailyReportDto()
                {
                    StartDate = group.Key,
                    EndDate = group.Key.AddDays(1),
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

            return resultList.OrderBy(r => r.StartDate).ToList();
        }
    }
}
