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
using System.Diagnostics;

namespace FarroAPI.Controllers
{
    [Route("{hostId}/api/SalesCategoryBranchReport")]
    public class SalesCategoryBranchReportController : Controller
    {
        private readonly farroContext _context;

        public SalesCategoryBranchReportController(farroContext context)
        {
            this._context = context;
        }
        [HttpPost("Today")]
        public async Task<IActionResult> GetSalesCategoryBranchReportToday([FromBody] SalesCategoryBranchReportFilterDto filter)
        {
            // Set the filter to "Today"
            filter.SetToday();

            // Return to response
            var listToReturn = await GetSalesCategoryBranchReport(filter);
            return Ok(listToReturn);
        }

        [HttpPost("Yesterday")]
        public async Task<IActionResult> GetSalesCategoryBranchReportYesterday([FromBody] SalesCategoryBranchReportFilterDto filter)
        {
            // Set the filter to "Yesterday"
            filter.SetYesterday();

            // Return to response
            var listToReturn = await GetSalesCategoryBranchReport(filter);
            return Ok(listToReturn);
        }

        [HttpPost("ThisWeek")]
        public async Task<IActionResult> GetSalesCategoryBranchReportThisWeek([FromBody] SalesCategoryBranchReportFilterDto filter)
        {
            // Set the filter to "ThisWeek"
            filter.SetThisWeek();

            // Return to response
            var listToReturn = await GetSalesCategoryBranchReport(filter);
            return Ok(listToReturn);
        }

        [HttpPost("LastWeek")]
        public async Task<IActionResult> GetSalesCategoryBranchReportLastWeek([FromBody] SalesCategoryBranchReportFilterDto filter)
        {
            // Set the filter to "LastWeek"
            filter.SetLastWeek();

            // Return to response
            var listToReturn = await GetSalesCategoryBranchReport(filter);
            return Ok(listToReturn);
        }

        [HttpPost("ThisMonth")]
        public async Task<IActionResult> GetSalesCategoryBranchReportThisMonth([FromBody] SalesCategoryBranchReportFilterDto filter)
        {
            // Set the filter to "ThisMonth"
            filter.SetThisMonth();

            // Return to response
            var listToReturn = await GetSalesCategoryBranchReport(filter);
            return Ok(listToReturn);
        }

        [HttpPost("LastMonth")]
        public async Task<IActionResult> GetSalesCategoryBranchReportLastMonth([FromBody] SalesCategoryBranchReportFilterDto filter)
        {
            // Set the filter to "LastMonth"
            filter.SetLastMonth();

            // Return to response
            var listToReturn = await GetSalesCategoryBranchReport(filter);
            return Ok(listToReturn);
        }

        [HttpPost("LastThreeMonths")]
        public async Task<IActionResult> GetSalesCategoryBranchReportLastThreeMonths([FromBody] SalesCategoryBranchReportFilterDto filter)
        {
            // Set the filter to "LastThreeMonths"
            filter.SetLastThreeMonths();

            // Return to response
            var listToReturn = await GetSalesCategoryBranchReport(filter);
            return Ok(listToReturn);
        }

        [HttpPost("DateRange")]
        public async Task<IActionResult> GetSalesCategoryBranchReportDateRange([FromBody] SalesCategoryBranchReportFilterDto filter)
        {
            // Return to response
            var listToReturn = await GetSalesCategoryBranchReport(filter);
            return Ok(listToReturn);
        }

        private async Task<List<SalesCategoryBranchReportDto>> GetSalesCategoryBranchReport(SalesCategoryBranchReportFilterDto filter)
        {
                // Set NoTracking for ChangeTracker
                _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                var resultList = new List<SalesCategoryBranchReportDto>();

                // Make commandText ready
                string commandText = null;
                string categories = "";

                if (filter.Categories.Count > 0)
                {
                    categories = " and cat in (";
                    for (int i = 0; i < filter.Categories.Count; i++)
                    {
                        categories += @"'" + filter.Categories[i] + @"'";
                        if (i < filter.Categories.Count - 1)
                            categories += ",";
                        else
                            categories += ") ";
                    }
                }

                commandText = @"select b.id as BranchId
                                , round(sum(s.commit_price * s.quantity * 1.15), 2) as Sales
                                , round(sum((s.commit_price - s.supplier_price) * s.quantity * 1.15), 2) as Profit
	                            , (select round(sum(amount * 1.15), 2) from budget 
                                    where branch_id = b.id"
                                    + categories +
                                    @" and date >= @startDateTime 
	                                and date < @endDateTime) as Budget
                                from sales s
                                join invoice i on s.invoice_number = i.invoice_number
                                join branch b on i.branch = b.id
                                where b.fax <> 'hidden4mreport'"
                                + categories +
                                @" and i.commit_date >= @startDateTime
                                and i.commit_date < @endDateTime
                                group by b.id
                                order by b.id";

                // Run SQL Command
                using (var connection = (SqlConnection)_context.Database.GetDbConnection())
                {
                    
                    var command = new SqlCommand(commandText, connection);
                    command.Parameters.AddWithValue("@startDateTime", filter.StartDateTime);
                    command.Parameters.AddWithValue("@endDateTime", filter.EndDateTime);

                    _context.Database.OpenConnection();
                    using (var result = await command.ExecuteReaderAsync())
                    {
                        while (result.Read())
                        {
                            SalesCategoryBranchReportDto dto = new SalesCategoryBranchReportDto
                            {
                                BranchId = Convert.ToInt32(result["BranchId"]),
                                AmountWithGST = Convert.ToDecimal(result["Sales"] is DBNull ? 0 : result["Sales"]),
                                ProfitWithGST = Convert.ToDecimal(result["Profit"] is DBNull ? 0 : result["Profit"]),
                                BudgetWithGST = Convert.ToDecimal(result["Budget"] is DBNull ? 0 : result["Budget"])
                            };
                            dto.BudgetWithGST = Math.Round(dto.BudgetWithGST, 2);
                            resultList.Add(dto);
                        }
                    }
                    _context.Database.CloseConnection();

                    return resultList;
                }
            
        }
    }
}
