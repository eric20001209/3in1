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
    [Route("{hostId}/api/SalesPaymentReport")]
    public class SalesPaymentReportController : Controller
    {
        private readonly farroContext _context;

        public SalesPaymentReportController(farroContext context)
        {
            this._context = context;
        }
        [HttpGet("Today")]
        public IActionResult GetSalesPaymentReportToday([FromQuery] int? branchId)
        {
            // Set the filter to "Today"
            var filter = new SalesPaymentReportFilterDto();
            filter.SetToday();
            filter.BranchId = branchId;

            // Return to response
            var listToReturn = GetSalesPaymentReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("Yesterday")]
        public IActionResult GetSalesPaymentReportYesterday([FromQuery] int? branchId)
        {
            // Set the filter to "Yesterday"
            var filter = new SalesPaymentReportFilterDto();
            filter.SetYesterday();
            filter.BranchId = branchId;

            // Return to response
            var listToReturn = GetSalesPaymentReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("ThisWeek")]
        public IActionResult GetSalesPaymentReportThisWeek([FromQuery] int? branchId)
        {
            // Set the filter to "ThisWeek"
            var filter = new SalesPaymentReportFilterDto();
            filter.SetThisWeek();
            filter.BranchId = branchId;

            // Return to response
            var listToReturn = GetSalesPaymentReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastWeek")]
        public IActionResult GetSalesPaymentReportLastWeek([FromQuery] int? branchId)
        {
            // Set the filter to "LastWeek"
            var filter = new SalesPaymentReportFilterDto();
            filter.SetLastWeek();
            filter.BranchId = branchId;

            // Return to response
            var listToReturn = GetSalesPaymentReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("ThisMonth")]
        public IActionResult GetSalesPaymentReportThisMonth([FromQuery] int? branchId)
        {
            // Set the filter to "ThisMonth"
            var filter = new SalesPaymentReportFilterDto();
            filter.SetThisMonth();
            filter.BranchId = branchId;

            // Return to response
            var listToReturn = GetSalesPaymentReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastMonth")]
        public IActionResult GetSalesPaymentReportLastMonth([FromQuery] int? branchId)
        {
            // Set the filter to "LastMonth"
            var filter = new SalesPaymentReportFilterDto();
            filter.SetLastMonth();
            filter.BranchId = branchId;

            // Return to response
            var listToReturn = GetSalesPaymentReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastThreeMonths")]
        public IActionResult GetSalesPaymentReportLastThreeMonths([FromQuery] int? branchId)
        {
            // Set the filter to "LastThreeMonths"
            var filter = new SalesPaymentReportFilterDto();
            filter.SetLastThreeMonths();
            filter.BranchId = branchId;

            // Return to response
            var listToReturn = GetSalesPaymentReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("DateRange")]
        public IActionResult GetSalesPaymentReportDateRange([FromQuery] DateTime startDateTime,
            [FromQuery] DateTime endDateTime, [FromQuery] int? branchId)
        {
            // Validate user inpute
            if (startDateTime == null || endDateTime == null)
            {
                return BadRequest();
            }

            // Set the filter to "DateRange"
            var filter = new SalesPaymentReportFilterDto
            {
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                BranchId = branchId
            };

            // Return to response
            var listToReturn = GetSalesPaymentReport(filter);
            return Ok(listToReturn);
        }

        private List<SalesPaymentReportDto> GetSalesPaymentReport(SalesPaymentReportFilterDto filter)
        {
            // Turn Object Tracking off
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Join trans, tran_detail, and enum tables
            var tranGroups = _context.Trans
                .Where(t => filter.BranchId.HasValue ? t.Branch == filter.BranchId.GetValueOrDefault() : true)
                .Select(t => new { t.Id, t.Amount, t.Branch })
                .Join(
                    _context.Branch
                        .Where(b => b.Fax.ToLower() != "hidden4mreport")
                        //.Where(b => b.Activated == true)
                        .Select(b => new { b.Id }),
                    t => t.Branch,
                    b => b.Id,
                    (t, b) => new { t.Id, t.Amount }
                )
                .Join(
                    _context.TranDetail
                        .Where(td => td.TransDate >= filter.StartDateTime)
                        .Where(td => td.TransDate < filter.EndDateTime)
                        .Select(td => new { td.Id, td.PaymentMethod }),
                    t => t.Id,
                    td => td.Id,
                    (t, td) => new { t.Amount, td.PaymentMethod })
                .Join(
                    _context.Enum
                        .Where(e => e.Class == "payment_method")
                        .Select(e => new { e.Id, e.Name })
                        .ToList(), // because the id in enum table is not unique
                    t => t.PaymentMethod,
                    e => e.Id,
                    (t, e) => new { t.Amount, PaymentMethod = e.Name })
                .GroupBy(t => t.PaymentMethod);

            /* Apply report logic */
            var resultList = new List<SalesPaymentReportDto>();
            
            foreach(var group in tranGroups)
            {
                resultList.Add(new SalesPaymentReportDto
                {
                    PaymentMethod = group.Key,
                    Total = Math.Round(group
                        .Sum(g => g.Amount)
                        , 2)
                });
            }

            return resultList;
        }
    }
}
