using FarroAPI.Controllers;
using FarroAPI.Entities;
using FarroAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Controllers
{
    [Route("{hostId}/api/SalesInvoiceReport")]
    public class SalesInvoiceReportController : Controller
    {
        private readonly farroContext _context;
        public SalesInvoiceReportController(farroContext context)
        {
            this._context = context;
        }
        [HttpGet("Today")]
        public IActionResult GetSalesInvoiceReportToday([FromQuery] int? branchId, [FromQuery] string invoiceNumber, [FromQuery] string salesPerson)
        {
            // Set the filter to "Today"
            var filter = new SalesInvoiceReportFilterDto();
            filter.SetToday();
            filter.BranchId = branchId;
            filter.InvoiceNumber = invoiceNumber;
            filter.SalesPerson = salesPerson;

            // Return to response
            var listToReturn = GetSalesInvoiceReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("Yesterday")]
        public IActionResult GetSalesInvoiceReportYesterday([FromQuery] int? branchId, [FromQuery] string invoiceNumber, [FromQuery] string salesPerson)
        {
            // Set the filter to "Yesterday"
            var filter = new SalesInvoiceReportFilterDto();
            filter.SetYesterday();
            filter.BranchId = branchId;
            filter.InvoiceNumber = invoiceNumber;
            filter.SalesPerson = salesPerson;

            // Return to response
            var listToReturn = GetSalesInvoiceReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("ThisWeek")]
        public IActionResult GetSalesInvoiceReportThisWeek([FromQuery] int? branchId, [FromQuery] string invoiceNumber, [FromQuery] string salesPerson)
        {
            // Set the filter to "ThisWeek"
            var filter = new SalesInvoiceReportFilterDto();
            filter.SetThisWeek();
            filter.BranchId = branchId;
            filter.InvoiceNumber = invoiceNumber;
            filter.SalesPerson = salesPerson;

            // Return to response
            var listToReturn = GetSalesInvoiceReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastWeek")]
        public IActionResult GetSalesInvoiceReportLastWeek([FromQuery] int? branchId, [FromQuery] string invoiceNumber, [FromQuery] string salesPerson)
        {
            // Set the filter to "LastWeek"
            var filter = new SalesInvoiceReportFilterDto();
            filter.SetLastWeek();
            filter.BranchId = branchId;
            filter.InvoiceNumber = invoiceNumber;
            filter.SalesPerson = salesPerson;

            // Return to response
            var listToReturn = GetSalesInvoiceReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("ThisMonth")]
        public IActionResult GetSalesInvoiceReportThisMonth([FromQuery] int? branchId, [FromQuery] string invoiceNumber, [FromQuery] string salesPerson)
        {
            // Set the filter to "ThisMonth"
            var filter = new SalesInvoiceReportFilterDto();
            filter.SetThisMonth();
            filter.BranchId = branchId;
            filter.InvoiceNumber = invoiceNumber;
            filter.SalesPerson = salesPerson;

            // Return to response
            var listToReturn = GetSalesInvoiceReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastMonth")]
        public IActionResult GetSalesInvoiceReportLastMonth([FromQuery] int? branchId, [FromQuery] string invoiceNumber, [FromQuery] string salesPerson)
        {
            // Set the filter to "LastMonth"
            var filter = new SalesInvoiceReportFilterDto();
            filter.SetLastMonth();
            filter.BranchId = branchId;
            filter.InvoiceNumber = invoiceNumber;
            filter.SalesPerson = salesPerson;

            // Return to response
            var listToReturn = GetSalesInvoiceReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("LastThreeMonths")]
        public IActionResult GetSalesInvoiceReportLastThreeMonths([FromQuery] int? branchId, [FromQuery] string invoiceNumber, [FromQuery] string salesPerson)
        {
            // Set the filter to "LastThreeMonths"
            var filter = new SalesInvoiceReportFilterDto();
            filter.SetLastThreeMonths();
            filter.BranchId = branchId;
            filter.InvoiceNumber = invoiceNumber;
            filter.SalesPerson = salesPerson;

            // Return to response
            var listToReturn = GetSalesInvoiceReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("DateRange")]
        public IActionResult GetSalesInvoiceReportDateRange([FromQuery] DateTime startDateTime,
                [FromQuery] DateTime endDateTime, [FromQuery] int? branchId, [FromQuery] string invoiceNumber, [FromQuery] string salesPerson)
        {
            // Validate user inpute
            if (startDateTime == null || endDateTime == null)
            {
                return BadRequest();
            }

            // Set the filter to "DateRange"
            var filter = new SalesInvoiceReportFilterDto
            {
                BranchId = branchId,
                InvoiceNumber = invoiceNumber,
                SalesPerson = salesPerson
            };

            if (startDateTime != DateTime.MinValue)
                filter.StartDateTime = startDateTime;
            if (endDateTime != DateTime.MinValue)
                filter.EndDateTime = endDateTime;

            // Return to response
            var listToReturn = GetSalesInvoiceReport(filter);
            return Ok(listToReturn);
        }

        private List<SalesInvoiceReportDto> GetSalesInvoiceReport(SalesInvoiceReportFilterDto filter)
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
                .Where(i => filter.SalesPerson == null ? true : i.Sales == filter.SalesPerson)
                .Where(i => filter.InvoiceNumber == null ? true : i.InvoiceNumber.ToString().Contains(filter.InvoiceNumber))
                .Select(i => new { i.InvoiceNumber, i.Total, i.CommitDate, i.Branch, i.Sales, i.CardId })
                .Join(
                    _context.Branch
                        .Where(b => b.Country.ToLower() != "hidden")
                        .Select(b => new { b.Id, b.Name }),
                    i => i.Branch,
                    b => b.Id,
                    (i, b) => new {
                        i.InvoiceNumber,
                        i.Total,
                        i.CommitDate,
                        BranchName = b.Name,
                        SalesPerson = i.Sales,
                        CustomerId = i.CardId
                    }
                )
                .Join(
                    _context.Sales
                    .Select(s => new { s.InvoiceNumber, s.Code, s.Quantity, s.CommitPrice, s.SupplierPrice }),
                    i => i.InvoiceNumber,
                    s => s.InvoiceNumber,
                    (i, s) => new {
                        i.InvoiceNumber,
                        i.Total,
                        i.CommitDate,
                        i.BranchName,
                        i.CustomerId,
                        i.SalesPerson,
                        s.Code,
                        s.Quantity,
                        s.CommitPrice,
                        s.SupplierPrice
                    }
                )
                .GroupBy(i => i.InvoiceNumber);

            // Apply sales Daily report logic
            var resultList = new List<SalesInvoiceReportDto>();

            foreach (var group in invoiceGroups)
            {
                string CustomerName;
                if (group.FirstOrDefault().CustomerId == 0)
                {
                    CustomerName = "Cash Sales";
                }
                else
                {
                    CustomerName = _context.Card.FirstOrDefault(c => c.Id == group.FirstOrDefault().CustomerId).Name;
                }
                 
                resultList.Add(new SalesInvoiceReportDto()
                {
                    InvoiceNumber = group.Key,
                    BranchName = group.FirstOrDefault().BranchName,
                    TillNumber =_context.Orders.FirstOrDefault(o => o.InvoiceNumber == group.Key).StationId,
                    LocalInvoiceNumber = _context.Orders.FirstOrDefault(o => o.InvoiceNumber == group.Key).PoNumber,
                    InvoiceDateTime = group.FirstOrDefault().CommitDate,
                    Customer = CustomerName,
                    SalesPerson = group.FirstOrDefault().SalesPerson,
                    AmountWithGST = Math.Round(group.FirstOrDefault().Total.GetValueOrDefault(), 2),
                    ProfitWithGST = Math.Round(group
                        .Sum(i => (decimal)((i.CommitPrice - i.SupplierPrice) * (decimal)i.Quantity * 1.15m))
                        , 2)
                });
            }

            return resultList;
        }
    }
}
