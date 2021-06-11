using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FarroAPI.Entities;
using FarroAPI.Models;
using System.Data;
using System.Data.SqlClient;

namespace FarroAPI.Controllers
{
    [Route("{hostId}/mreport/api/SalesMonthlyReport")]
    public class SalesMonthlyReportController : Controller
    {
        private readonly farroContext _context;

        public SalesMonthlyReportController(farroContext context)
        {
            this._context = context;
        }
        [HttpGet("ThisYear")]
        public IActionResult GetSalesMonthlyReportThisYear([FromQuery] int? branchId)
        {
            // Set the filter to "LastThreeMonths"
            var filter = new SalesMonthlyReportFilterDto();
            filter.SetThisYear();
            filter.BranchId = branchId;

            // Return to response
            var listToReturn = GetSalesMonthlyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("Last12Months")]
        public IActionResult GetSalesMonthlyReportLast12Months([FromQuery] int? branchId)
        {
            // Set the filter to "Last12Months"
            var filter = new SalesMonthlyReportFilterDto();
            filter.SetLast12Months();
            filter.BranchId = branchId;

            // Return to response
            var listToReturn = GetSalesMonthlyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("Last24Months")]
        public IActionResult GetSalesMonthlyReportLast24Months([FromQuery] int? branchId)
        {
            // Set the filter to "Last24Months"
            var filter = new SalesMonthlyReportFilterDto();
            filter.SetLast24Months();
            filter.BranchId = branchId;

            // Return to response
            var listToReturn = GetSalesMonthlyReport(filter);
            return Ok(listToReturn);
        }

        [HttpGet("DateRange")]
        public IActionResult GetSalesMonthlyReportDateRange([FromQuery] DateTime startDateTime,
            [FromQuery] DateTime endDateTime, [FromQuery] int? branchId)
        {
            // Validate user inpute
            if (startDateTime == null || endDateTime == null)
            {
                return BadRequest();
            }

            // Set the filter to "DateRange"
            var filter = new SalesMonthlyReportFilterDto
            {
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                BranchId = branchId
            };

            // Return to response
            var listToReturn = GetSalesMonthlyReport(filter);
            return Ok(listToReturn);
        }

        private List<SalesMonthlyReportDto> GetSalesMonthlyReport(SalesMonthlyReportFilterDto filter)
        {
            // Set NoTracking for ChangeTracker
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var resultList = new List<SalesMonthlyReportDto>();

            // Execute proc_MonthlySalesReport_TotalAndQuantity
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "proc_MonthlySalesReport_TotalAndQuantity";

                var param1 = command.CreateParameter();
                param1.ParameterName = "@branch_id";
                param1.DbType = DbType.Int32;
                param1.Value = filter.BranchId;
                command.Parameters.Add(param1);

                var param2 = command.CreateParameter();
                param2.ParameterName = "@start_datetime";
                param2.DbType = DbType.DateTime;
                param2.Value = filter.StartDateTime;
                command.Parameters.Add(param2);

                var param3 = command.CreateParameter();
                param3.ParameterName = "@end_datetime";
                param3.DbType = DbType.DateTime;
                param3.Value = filter.EndDateTime;
                command.Parameters.Add(param3);

                _context.Database.OpenConnection();
                using (var result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        SalesMonthlyReportDto monthlySales = new SalesMonthlyReportDto
                        {
                            StartDate = new DateTime(int.Parse(result["yearMonth"].ToString().Substring(0,4)), int.Parse(result["yearMonth"].ToString().Substring(5, 2)), 1),
                            EndDate = new DateTime(int.Parse(result["yearMonth"].ToString().Substring(0, 4)), int.Parse(result["yearMonth"].ToString().Substring(5, 2)), 1).AddMonths(1),
                            TotalWithGST = Convert.ToDecimal(result["totalWithGST"]),
                            InvoiceQuantity = Convert.ToInt32(result["invoiceQuantity"])
                        };
                        resultList.Add(monthlySales);
                    }
                }
                _context.Database.CloseConnection();
            }

            // Execute proc_MonthlySalesReport_ProfitAndQuantity
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "proc_MonthlySalesReport_ProfitAndQuantity";

                var param1 = command.CreateParameter();
                param1.ParameterName = "@branch_id";
                param1.DbType = DbType.Int32;
                param1.Value = filter.BranchId;
                command.Parameters.Add(param1);

                var param2 = command.CreateParameter();
                param2.ParameterName = "@start_datetime";
                param2.DbType = DbType.DateTime;
                param2.Value = filter.StartDateTime;
                command.Parameters.Add(param2);

                var param3 = command.CreateParameter();
                param3.ParameterName = "@end_datetime";
                param3.DbType = DbType.DateTime;
                param3.Value = filter.EndDateTime;
                command.Parameters.Add(param3);

                _context.Database.OpenConnection();
                using (var result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        var startDate = new DateTime(int.Parse(result["yearMonth"].ToString().Substring(0, 4)), int.Parse(result["yearMonth"].ToString().Substring(5, 2)), 1);
                        resultList.FirstOrDefault(r => r.StartDate == startDate).ProfitWithGST = Convert.ToDecimal(result["profitWithGST"]);
                    }
                }
                _context.Database.CloseConnection();
            }

            return resultList.OrderBy(r => r.StartDate).ToList();
        }

        private List<SalesMonthlyReportDto> GetSalesMonthlyReportEF(SalesMonthlyReportFilterDto filter)
        {
            // Set NoTracking for ChangeTracker
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Join invoice, and sales tables
            var invoiceGroups = _context.Invoice
                .Where(i => i.CommitDate >= filter.StartDateTime)
                .Where(i => i.CommitDate < filter.EndDateTime)
                // LINQ to SQL Leaky abstractions pitfull
                //.Where(i => filter.BranchId.HasValue ? (i.Monthly == filter.BranchId.Value) : true)
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
                .GroupBy(i => i.CommitDate.Year.ToString() + i.CommitDate.Month.ToString());

            // Apply sales Monthly report logic
            var resultList = new List<SalesMonthlyReportDto>();

            foreach (var group in invoiceGroups)
            {
                var year = group.FirstOrDefault().CommitDate.Year;
                var month = group.FirstOrDefault().CommitDate.Month;

                resultList.Add(new SalesMonthlyReportDto()
                {
                    StartDate = new DateTime(year, month, 1),
                    EndDate = new DateTime(year, month, 1).AddMonths(1),
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

            return resultList.OrderBy(r => r.StartDate).ToList();
        }
    }
}
