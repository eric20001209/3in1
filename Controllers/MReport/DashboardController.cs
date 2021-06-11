using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarroAPI.Entities;
using FarroAPI.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.SqlClient;
using System;
using System.Linq;

namespace FarroAPI.Controllers
{
    [Route("{hostId}/mreport/api/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly farroContext _context;

        public DashboardController(farroContext context)
        {
            _context = context;
        }

        /* Daily/DateRange for line chart*/
        [HttpPost("Daily/DateRange")]
        public async Task<IActionResult> GetDailyDashboardDateRange([FromBody] DashboardFilterDto filter)
        {
            // Set NoTracking for ChangeTracker
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var resultList = new List<DailyDashboardDto>();

            // Make commandText ready
            string commandText = null;

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

            string categories = "";
            if (filter.Categories.Count > 0)
            {
                categories = " and c.cat in (";
                for (int i = 0; i < filter.Categories.Count; i++)
                {
                    categories += @"'" + filter.Categories[i] + @"'";
                    if (i < filter.Categories.Count - 1)
                        categories += ",";
                    else
                        categories += ") ";
                }
            }

            string categoriesForMinus1001 = "";
            if (filter.Categories.Count > 0)
            {
                categoriesForMinus1001 = " and p.promo_cat in (";
                for (int i = 0; i < filter.Categories.Count; i++)
                {
                    categoriesForMinus1001 += @"'" + filter.Categories[i] + @"'";
                    if (i < filter.Categories.Count - 1)
                        categoriesForMinus1001 += ",";
                    else
                        categoriesForMinus1001 += ") ";
                }
            }

            commandText = @"select convert(date, i.commit_date) as Date 
                                , round(sum(s.commit_price * s.quantity), 2) as Revenue
                                , round(sum((s.commit_price - s.supplier_price) * s.quantity), 2) as Profit
                                , count(distinct i.invoice_number) as InvoiceQuantity
                                , round(sum(s.commit_price * s.quantity)/count(distinct i.invoice_number), 2) as BasketSpend
                                from invoice i
                                join sales s on i.invoice_number = s.invoice_number
                                join branch b on i.branch = b.id
                                left join code_relations c on s.code = c.code
                                left join promotion_list p on SUBSTRING(s.name, 11, LEN(s.name)) = p.promo_desc
                                where b.fax <> 'hidden4mreport'
                                and s.code <> '-900001'
                                and i.commit_date >= @startDateTime 
                                and i.commit_date < @endDateTime
                                " + branchIds + @"
                                and (s.code <> '-1001' " + categories + @"
                                    or s.code = '-1001' " + categoriesForMinus1001 + @")
                                group by convert(date, i.commit_date)
                                order by convert(date, i.commit_date)";

            // Run SQL Command
            using (var connection = (SqlConnection)_context.Database.GetDbConnection())
                {
                    
                    // This year
                    var command = new SqlCommand(commandText, connection);
                    command.Parameters.AddWithValue("@startDateTime", filter.StartDateTime);
                    command.Parameters.AddWithValue("@endDateTime", filter.EndDateTime);

                    command.CommandTimeout = 300;    

                    _context.Database.OpenConnection();
                    using (var result = await command.ExecuteReaderAsync())
                    {
                        while (result.Read())
                        {
                            var dto = new DailyDashboardDto
                            {
                                StartDateTime = Convert.ToDateTime(result["Date"]),
                                EndDateTime = Convert.ToDateTime(result["Date"]).AddDays(1),
                                TotalWithoutGST = Convert.ToDecimal(result["Revenue"] is DBNull ? 0 : result["Revenue"]),
                                ProfitWithoutGST = Convert.ToDecimal(result["Profit"] is DBNull ? 0 : result["Profit"]),
                                InvoiceQuantity = Convert.ToInt32(result["InvoiceQuantity"] is DBNull ? 0 : result["InvoiceQuantity"]),
                                BasketSpendWithoutGST = Convert.ToDecimal(result["BasketSpend"] is DBNull ? 0 : result["BasketSpend"])
                            };
                            resultList.Add(dto);
                        }
                    }
                    _context.Database.CloseConnection();

                    return Ok(resultList);
                }
            
        }

        /* Sales for Revnue, Profit, and BasketSpend */
        [HttpPost("Sales")]
        public async Task<IActionResult> GetSales([FromBody] DashboardFilterDto filter)
        {
                // Set NoTracking for ChangeTracker
                _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                // Make commandText ready
                string commandText = null;
                string commandTextForMinus1001 = null;

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

                string categories = "";
                if (filter.Categories.Count > 0)
                {
                    categories = " and c.cat in (";
                    for (int i = 0; i < filter.Categories.Count; i++)
                    {
                        categories += @"'" + filter.Categories[i] + @"'";
                        if (i < filter.Categories.Count - 1)
                            categories += ",";
                        else
                            categories += ") ";
                    }
                }

                string categoriesForMinus1001 = "";
                if (filter.Categories.Count > 0)
                {
                    categoriesForMinus1001 = " and p.promo_cat in (";
                    for (int i = 0; i < filter.Categories.Count; i++)
                    {
                        categoriesForMinus1001 += @"'" + filter.Categories[i] + @"'";
                        if (i < filter.Categories.Count - 1)
                            categoriesForMinus1001 += ",";
                        else
                            categoriesForMinus1001 += ") ";
                    }
                }

                commandText = @"select round(sum(s.commit_price * s.quantity), 2) as Revenue
                                    , round(sum((s.commit_price - s.supplier_price) * s.quantity), 2) as Profit
                                    , count(distinct i.invoice_number) as InvoiceQuantity
                                    from invoice i
                                    join sales s on i.invoice_number = s.invoice_number
                                    join branch b on i.branch = b.id
                                    left join code_relations c on s.code = c.code
                                    where b.fax <> 'hidden4mreport'
                                    and s.code <> '-900001'
                                    and s.code <> '-1001'
                                    and i.commit_date >= @startDateTime 
                                    and i.commit_date < @endDateTime
                                    " + branchIds + @"
                                    " + categories;

                commandTextForMinus1001 = @"select round(sum(s.commit_price * s.quantity), 2) as Amount
                                            from invoice i
                                            join branch b on i.branch = b.id
                                            join sales s on i.invoice_number = s.invoice_number
                                            join promotion_list p on SUBSTRING(s.name, 11, LEN(s.name)) = p.promo_desc
                                            where b.fax <> 'hidden4mreport'
                                            and s.code = '-1001'
                                            and i.commit_date >= @startDateTime 
                                            and i.commit_date < @endDateTime
                                            " + branchIds + @"
                                            " + categoriesForMinus1001;

                // Run SQL Command
                using (var connection = (SqlConnection)_context.Database.GetDbConnection())
                {
                    var commandForMinus1001 = new SqlCommand(commandTextForMinus1001, connection);
                    commandForMinus1001.Parameters.AddWithValue("@startDateTime", filter.StartDateTime);
                    commandForMinus1001.Parameters.AddWithValue("@endDateTime", filter.EndDateTime);
                    decimal amountForMinus1001 = 0m;

                    var command = new SqlCommand(commandText, connection);
                    command.Parameters.AddWithValue("@startDateTime", filter.StartDateTime);
                    command.Parameters.AddWithValue("@endDateTime", filter.EndDateTime);

                    _context.Database.OpenConnection();
                    using(var resultForMinus1001 = await commandForMinus1001.ExecuteReaderAsync())
                    {
                        while (resultForMinus1001.Read())
                        {
                            amountForMinus1001 = Convert.ToDecimal(
                                resultForMinus1001["Amount"] is DBNull ? 0 : resultForMinus1001["Amount"]);
                        }
                    }

                    using (var result = await command.ExecuteReaderAsync())
                    {
                        while (result.Read())
                        {
                            var revenue = Convert.ToDecimal(result["Revenue"] is DBNull ? 0 : result["Revenue"]);
                            var profit = Convert.ToDecimal(result["Profit"] is DBNull ? 0 : result["Profit"]);
                            var invoiceQuantity = Convert.ToInt32(result["InvoiceQuantity"] is DBNull ? 0 : result["InvoiceQuantity"]);
                            return Ok(new
                            {
                                TotalWithoutGST = amountForMinus1001 + revenue,
                                ProfitWithoutGST = amountForMinus1001 + profit,
                                BasketSpendWithoutGST = invoiceQuantity == 0 ? 0 : Math.Round((revenue + amountForMinus1001) / invoiceQuantity, 2),
                                InvoiceQuantity = invoiceQuantity
                            });
                        }
                    }
                    _context.Database.CloseConnection();
                    return NotFound();
                }
            
        }

        [HttpPost("Waste")]
        public async Task<IActionResult> GetWaste([FromBody] DashboardFilterDto filter)
        {
                // Set NoTracking for ChangeTracker
                _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                // Make commandText ready
                string commandText = null;

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

                string categories = "";
                if (filter.Categories.Count > 0)
                {
                    categories = " and c.cat in (";
                    for (int i = 0; i < filter.Categories.Count; i++)
                    {
                        categories += @"'" + filter.Categories[i] + @"'";
                        if (i < filter.Categories.Count - 1)
                            categories += ",";
                        else
                            categories += ") ";
                    }
                }

                commandText = @"select round(sum(qty*cost), 2) as Waste
	                            from waste w 
	                            join branch b on w.branch_id = b.id
	                            join code_relations c on w.code = c.code
	                            where record_date >= @startDateTime 
	                            and record_date < @endDateTime
                                and reason_id in (1, 2, 7, 9)
	                            and b.fax <> 'hidden4mreport'
	                            " + branchIds + @"
	                            " + categories;

                // Run SQL Command
                using (var connection = (SqlConnection)_context.Database.GetDbConnection())
                {
                    var command1 = new SqlCommand(commandText, connection);
                    command1.Parameters.AddWithValue("@startDateTime", filter.StartDateTime);
                    command1.Parameters.AddWithValue("@endDateTime", filter.EndDateTime);

                    _context.Database.OpenConnection();
                    using (var result = await command1.ExecuteReaderAsync())
                    {
                        while (result.Read())
                        {
                            return Ok(new
                            {
                                WasteWithoutGST = Convert.ToDecimal(result["Waste"] is DBNull ? 0 : result["Waste"])
                            });
                        }
                    }
                    _context.Database.CloseConnection();
                    return NotFound();
                }
            
        }

        [HttpPost("Budget")]
        public async Task<IActionResult> GetBudget([FromBody] DashboardFilterDto filter)
        {
                // Set NoTracking for ChangeTracker
                _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                // Make commandText ready
                string commandText = null;

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

                string categories = "";
                if (filter.Categories.Count > 0)
                {
                    categories = " and c.cat in (";
                    for (int i = 0; i < filter.Categories.Count; i++)
                    {
                        categories += @"'" + filter.Categories[i] + @"'";
                        if (i < filter.Categories.Count - 1)
                            categories += ",";
                        else
                            categories += ") ";
                    }
                }

                commandText = @"select round(sum(amount), 2) as Budget
	                            from budget c
	                            join branch b on c.Branch_Id = b.id
                                and c.date >= @startDateTime
	                            and c.date < @endDateTime
	                            and b.fax <> 'hidden4mreport'
	                            " + branchIds + @"
                                " + categories;

                // Run SQL Command
                using (var connection = (SqlConnection)_context.Database.GetDbConnection())
                {
                    var command1 = new SqlCommand(commandText, connection);
                    command1.Parameters.AddWithValue("@startDateTime", filter.StartDateTime);
                    command1.Parameters.AddWithValue("@endDateTime", filter.EndDateTime);

                    _context.Database.OpenConnection();
                    using (var result = await command1.ExecuteReaderAsync())
                    {
                        while (result.Read())
                        {
                            return Ok(new
                            {
                                BudgetWithoutGST = Convert.ToDecimal(result["Budget"] is DBNull ? 0 : result["Budget"])
                            });
                        }
                    }
                    _context.Database.CloseConnection();
                    return NotFound();
                }
            
        }

        [HttpPost("MarkDown")]
        public async Task<IActionResult> GetMarkDown([FromBody] DashboardFilterDto filter)
        {
            // Set NoTracking for ChangeTracker
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Make commandText ready
            string commandText = null;

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

            string categories = "";
            if (filter.Categories.Count > 0)
            {
                categories = " and c.cat in (";
                for (int i = 0; i < filter.Categories.Count; i++)
                {
                    categories += @"'" + filter.Categories[i] + @"'";
                    if (i < filter.Categories.Count - 1)
                        categories += ",";
                    else
                        categories += ") ";
                }
            }

            commandText = @"select round(sum(s.quantity * s.commit_price), 2) as MarkDown
	                        from invoice i
	                        join branch b on i.branch = b.id
	                        join sales s on i.invoice_number = s.invoice_number
	                        left join code_relations c on s.code = c.code
	                        where b.fax <> 'hidden4mreport'
	                        and s.price_on_special <> 1
	                        and c.code <> '-1001'
	                        and round((s.normal_price - commit_price), 6) > 0
	                        and i.commit_date >= @startDateTime 
	                        and i.commit_date < @endDateTime
	                        " + branchIds + @"
	                        " + categories;



     
            // Run SQL Command
            using (var connection = (SqlConnection)_context.Database.GetDbConnection())
            {
                var command1 = new SqlCommand(commandText, connection);
                command1.Parameters.AddWithValue("@startDateTime", filter.StartDateTime);
                command1.Parameters.AddWithValue("@endDateTime", filter.EndDateTime);

                _context.Database.OpenConnection();
                using (var result = await command1.ExecuteReaderAsync())
                {
                    while (result.Read())
                    {
                        return Ok(new
                        {
                            MarkDownWithoutGST = Convert.ToDecimal(result["MarkDown"] is DBNull ? 0 : result["MarkDown"])
                        });
                    }
                }
                _context.Database.CloseConnection();
                return NotFound();
            }
        }

        [HttpPost("StockTake")]
        public async Task<IActionResult> GetStockTake([FromBody] DashboardFilterDto filter)
        {
            // Set NoTracking for ChangeTracker
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Make commandText ready
            string commandText = null;

            string branchIds = "";
            if (filter.BranchIds.Count == 0)
            {
                var validBranchIds = _context.Branch
                                    .Where(b => b.Fax.ToLower() != "hidden4mreport" && b.Id != 15)
                                    .Select(b => b.Id);
                foreach (var branchId in validBranchIds)
                {
                    filter.BranchIds.Add(branchId);
                }
            }
            branchIds = " and branch_id in (";
            for (int i = 0; i < filter.BranchIds.Count; i++)
            {
                branchIds += filter.BranchIds[i].ToString();
                if (i < filter.BranchIds.Count - 1)
                    branchIds += ",";
                else
                    branchIds += ") ";
            }

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

            commandText = @" select  count (l.stock_take_time) as Counted
                            , (select count(*) from code_relations c
	                            join stock_qty q on c.code = q.code
	                            where c.code > 1023 
                                " + branchIds + @"
	                            " + categories + @"
	                            and q.qty <> 0) as Total
                            from code_relations c join
	                            (select code, branch_id, max(log_time) as stock_take_time
	                            from stock_adj_log 
	                            where log_time >= @startDateTime
	                            and log_time < @endDateTime
	                            " + branchIds + @"
	                            group by code, branch_id) as l on c.code = l.code
                                join stock_qty q on l.code = q.code and l.branch_id = q.branch_id
                             where c.code > 1023
                                and q.qty <> 0"
                                + categories;

            // Run SQL Command
            using (var connection = (SqlConnection)_context.Database.GetDbConnection())
            {
                var command1 = new SqlCommand(commandText, connection);
                command1.Parameters.AddWithValue("@startDateTime", filter.StartDateTime);
                command1.Parameters.AddWithValue("@endDateTime", filter.EndDateTime);

                _context.Database.OpenConnection();
                using (var result = await command1.ExecuteReaderAsync())
                {
                    while (result.Read())
                    {
                        var counted = Convert.ToDouble(result["Counted"]);
                        var total = Convert.ToDouble(result["Total"]);
                        var countedPercentage = total == 0 ? 0 : Math.Round((counted / total), 4);
                        var uncountedPercentage = Math.Round(1 - countedPercentage, 4);
                        return Ok(new
                        {
                            Counted = countedPercentage,
                            UnCounted = uncountedPercentage
                        });
                    }
                }
                _context.Database.CloseConnection();
                return NotFound();
            }
        }
    }
}