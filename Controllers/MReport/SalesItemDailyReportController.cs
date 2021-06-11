using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FarroAPI.Entities;
using FarroAPI.Models;
using System.Data.SqlClient;

namespace FarroAPI.Controllers
{
    [Route("{hostId}/mreport/api/SalesItemDailyReport")]
    public class SalesItemDailyReportController : Controller
    {
        private readonly farroContext _context;

        public SalesItemDailyReportController(farroContext context)
        {
            this._context = context;
        }
        [HttpPost("Today")]
        public async Task<IActionResult> GetSalesItemDailyReportToday([FromBody] SalesItemDailyReportFilterDto filter)
        {
            // input validation
            if (!ModelState.IsValid)
            {
                return BadRequest("Input data is invalid");
            }
            // Set the filter to "Today"
            filter.SetToday();

            // Return to response
            try
            {
                var listToReturn = await GetSalesItemDailyReport(filter);
                return Ok(listToReturn);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("Yesterday")]
        public async Task<IActionResult> GetSalesItemDailyReportYesterday([FromBody] SalesItemDailyReportFilterDto filter)
        {
            // input validation
            if (!ModelState.IsValid)
            {
                return BadRequest("Input data is invalid");
            }

            // Set the filter to "Yesterday"
            filter.SetYesterday();

            // Return to response
            try
            {
                var listToReturn = await GetSalesItemDailyReport(filter);
                return Ok(listToReturn);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("ThisWeek")]
        public async Task<IActionResult> GetSalesItemDailyReportThisWeek([FromBody] SalesItemDailyReportFilterDto filter)
        {
            // input validation
            if (!ModelState.IsValid)
            {
                return BadRequest("Input data is invalid");
            }

            // Set the filter to "ThisWeek"
            filter.SetThisWeek();

            // Return to response
            try
            {
                var listToReturn = await GetSalesItemDailyReport(filter);
                return Ok(listToReturn);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("LastWeek")]
        public async Task<IActionResult> GetSalesItemDailyReportLastWeek([FromBody] SalesItemDailyReportFilterDto filter)
        {
            // input validation
            if (!ModelState.IsValid)
            {
                return BadRequest("Input data is invalid");
            }

            // Set the filter to "LastWeek"
            filter.SetLastWeek();

            // Return to response
            try
            {
                var listToReturn = await GetSalesItemDailyReport(filter);
                return Ok(listToReturn);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("ThisMonth")]
        public async Task<IActionResult> GetSalesItemDailyReportThisMonth([FromBody] SalesItemDailyReportFilterDto filter)
        {
            // input validation
            if (!ModelState.IsValid)
            {
                return BadRequest("Input data is invalid");
            }

            // Set the filter to "ThisMonth"
            filter.SetThisMonth();

            // Return to response
            try
            {
                var listToReturn = await GetSalesItemDailyReport(filter);
                return Ok(listToReturn);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("LastMonth")]
        public async Task<IActionResult> GetSalesItemDailyReportLastMonth([FromBody] SalesItemDailyReportFilterDto filter)
        {
            // input validation
            if (!ModelState.IsValid)
            {
                return BadRequest("Input data is invalid");
            }

            // Set the filter to "LastMonth"
            filter.SetLastMonth();

            // Return to response
            try
            {
                var listToReturn = await GetSalesItemDailyReport(filter);
                return Ok(listToReturn);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("LastThreeMonths")]
        public async Task<IActionResult> GetSalesItemDailyReportLastThreeMonths([FromBody] SalesItemDailyReportFilterDto filter)
        {
            // input validation
            if (!ModelState.IsValid)
            {
                return BadRequest("Input data is invalid");
            }

            // Set the filter to "LastThreeMonths"
            filter.SetLastThreeMonths();

            // Return to response
            try
            {
                var listToReturn = await GetSalesItemDailyReport(filter);
                return Ok(listToReturn);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("DateRange")]
        public async Task<IActionResult> GetSalesItemDailyReportDateRange([FromBody] SalesItemDailyReportFilterDto filter)
        {
            // input validation
            if (!ModelState.IsValid)
            {
                return BadRequest("Input data is invalid");
            }

            if (filter.StartDateTime >= filter.EndDateTime)
            {
                return BadRequest("EndDateTime must be later than startDateTime");
            }

            // Return to response
            try
            {
                var listToReturn = await GetSalesItemDailyReport(filter);
                return Ok(listToReturn);
            }
            catch(ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private async Task<List<SalesItemDailyReportDto>> GetSalesItemDailyReport(SalesItemDailyReportFilterDto filter)
        {
            // Set NoTracking for ChangeTracker
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Make commandText ready
            string branchIds = "";
            if (filter.BranchIds.Count > 0)
            {
                branchIds = " and b.id in (";
                for (int i = 0; i < filter.BranchIds.Count; i++)
                {
                    branchIds += filter.BranchIds[i].ToString();
                    if (i < filter.BranchIds.Count - 1)
                        branchIds += ",";
                    else
                        branchIds += ") ";
                }
            }
            else
            {
                branchIds = " and b.fax <> 'Hidden4MReport' ";
            }

            string codes = "";
            if (filter.Codes.Count > 0)
            {
                codes = " and s.code in (";
                for (int i = 0; i < filter.Codes.Count; i++)
                {
                    codes += filter.Codes[i].ToString();
                    if (i < filter.Codes.Count - 1)
                        codes += ",";
                    else
                        codes += ") ";
                }
            }
            else
            {
                throw new ArgumentNullException("No items selected.");
            }

            string commandText = @"select s.code as Code
	                                    --, s.name
	                                    , i.branch as BranchId
	                                    , CAST(i.commit_date as date)  as StartDate
	                                    , DateAdd(day, 1, CAST(i.commit_date as date)) as EndDate
	                                    , round(sum(s.commit_price * s.quantity * 1.15), 2) as Amount
	                                    , round(sum((s.commit_price - s.supplier_price) * s.quantity * 1.15), 2) as Profit
	                                    , round(sum(s.quantity), 3) as Quantity
                                    from sales s 
                                    join invoice i on s.invoice_number = i.invoice_number
                                    join branch b on i.branch = b.id
                                    where i.commit_date >= @startDateTime
                                    and i.commit_date < @endDateTime"
                                    + codes + branchIds +
                                    @" group by s.code, i.branch, CAST(i.commit_date as date)--, s.name
                                    order by s.code, CAST(i.commit_date as date), i.branch";

            // Run SQL Command
            var resultList = new List<SalesItemDailyReportDto>();

            using(var connection = (SqlConnection)_context.Database.GetDbConnection())
            {
                var command = new SqlCommand(commandText, connection);
                command.Parameters.AddWithValue("startDateTime", filter.StartDateTime);
                command.Parameters.AddWithValue("endDateTime", filter.EndDateTime);

                _context.Database.OpenConnection();
                using(var result = await command.ExecuteReaderAsync())
                {
                    while (result.Read())
                    {
                        SalesItemDailyReportDto dto = new SalesItemDailyReportDto
                        {
                            BranchId = Convert.ToInt32(result["BranchId"]),
                            Code = Convert.ToInt32(result["Code"]),
                            StartDate = Convert.ToDateTime(result["StartDate"]),
                            EndDate = Convert.ToDateTime(result["EndDate"]),
                            AmountWithGST = Convert.ToDecimal(result["Amount"]),
                            ProfitWithGST = Convert.ToDecimal(result["Profit"]),
                            Quantity = Convert.ToDouble(result["Quantity"])
                        };
                        resultList.Add(dto);
                    }
                }
                _context.Database.CloseConnection();

                return resultList;
            }
        }
    }
}
