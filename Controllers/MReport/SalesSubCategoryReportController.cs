using FarroAPI.Entities;
using FarroAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Controllers
{
    [Route("{hostId}/mreport/api/SalesSubCategoryReport")]
    public class SalesSubCategoryReportController : Controller
    {
        private readonly farroContext _context;

        public SalesSubCategoryReportController(farroContext context)
        {
            this._context = context;
        }
        [HttpPost("DateRange")]
        public async Task<IActionResult> GetSalesSubCategoryReport([FromBody] SalesSubCategoryReportFilterDto filter)
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

                string commandText = @"select b.id as BranchId, s.cat as Category, s.s_cat as SubCategory
                            , round(sum(s.commit_price * s.quantity), 2) as Sales
                            , round(sum((s.commit_price - s.supplier_price) * s.quantity), 2) as Profit
	                        , (select round(sum(amount), 2) from budget 
                                where cat = s.cat 
                                and s_cat = s.s_cat
                                and date >= @startDateTime 
	                            and date < @endDateTime 
                                and branch_id = b.id) as Budget
                            from sales s
                            join invoice i on s.invoice_number = i.invoice_number
                            join branch b on i.branch = b.id
                            where i.commit_date >= @startDateTime
                            and i.commit_date < @endDateTime
                            and i.tax=isnull(@tax,i.tax)
                            and s.code <> '-900001'
                            and s.code <> '-1001'"
                            + categories + branchIds +
                            @" group by b.id, s.cat, s.s_cat
                            order by b.id, s.cat, s.s_cat";

                var commandTextForMinus1001 = @"select b.id as BranchId
                                            , p.promo_cat as Category
                                            , round(sum(s.commit_price * s.quantity), 2) as Amount
                                            from invoice i
                                            join branch b on i.branch = b.id
                                            join sales s on i.invoice_number = s.invoice_number
                                            join promotion_list p on SUBSTRING(s.name, 11, LEN(s.name)) = p.promo_desc
                                            where b.fax <> 'hidden4mreport'
                                            and s.code = '-1001'
                                            and i.commit_date >= @startDateTime 
                                            and i.commit_date < @endDateTime
                                            and i.tax=isnull(@tax,i.tax)
                                            " + branchIds + @"
                                            " + categoriesForMinus1001 + @"
                                            group by b.id, p.promo_cat";

                // Run SQL Command
                var resultList = new List<SalesSubCategoryReportDto>();

                using (var connection = (SqlConnection)_context.Database.GetDbConnection())
                {
                    var commandForMinus1001 = new SqlCommand(commandTextForMinus1001, connection);
                    commandForMinus1001.Parameters.AddWithValue("@startDateTime", filter.StartDateTime);
                    commandForMinus1001.Parameters.AddWithValue("@endDateTime", filter.EndDateTime);
                    List<Tuple<int, string, decimal>> minus1001ResultList = new List<Tuple<int, string, decimal>>();
                
                if (filter.OnlineOrder)
                {
                    commandForMinus1001.Parameters.AddWithValue("@tax", 0);
                }
                else
                {
                    commandForMinus1001.Parameters.AddWithValue("@tax", DBNull.Value);
                }
                var command = new SqlCommand(commandText, connection);
                    command.Parameters.AddWithValue("@startDateTime", filter.StartDateTime);
                    command.Parameters.AddWithValue("@endDateTime", filter.EndDateTime);
                if (filter.OnlineOrder)
                {
                    command.Parameters.AddWithValue("@tax", 0);
                }
                else
                {
                    command.Parameters.AddWithValue("@tax", DBNull.Value);
                }
                _context.Database.OpenConnection();
                    using (var resultForMinus1001 = await commandForMinus1001.ExecuteReaderAsync())
                    {
                        while (resultForMinus1001.Read())
                        {
                            minus1001ResultList.Add(new Tuple<int, string, decimal>(
                                Convert.ToInt32(resultForMinus1001["BranchId"]),
                                Convert.ToString(resultForMinus1001["Category"]),
                                Convert.ToDecimal(resultForMinus1001["Amount"] is DBNull ? 0 : resultForMinus1001["Amount"])
                            ));
                        }
                    }

                    using (var result = await command.ExecuteReaderAsync())
                    {
                        while (result.Read())
                        {
                            SalesSubCategoryReportDto dto = new SalesSubCategoryReportDto
                            {
                                BranchId = Convert.ToInt32(result["BranchId"]),
                                Category = Convert.ToString(result["Category"]),
                                SubCategory = Convert.ToString(result["SubCategory"]),
                                AmountWithoutGST = Convert.ToDecimal(result["Sales"] is DBNull ? 0 : result["Sales"]),
                                ProfitWithoutGST = Convert.ToDecimal(result["Profit"] is DBNull ? 0 : result["Profit"]),
                                BudgetWithoutGST = Convert.ToDecimal(result["Budget"] is DBNull ? 0 : result["Budget"])
                            };
                            dto.BudgetWithoutGST = Math.Round(dto.BudgetWithoutGST, 2);
                            resultList.Add(dto);
                        }
                    }
                    _context.Database.CloseConnection();

                    minus1001ResultList.ForEach(m =>
                    {
                        resultList.Add(new SalesSubCategoryReportDto
                        {
                            BranchId = m.Item1,
                            Category = m.Item2,
                            SubCategory = "Promotion Loss",
                            AmountWithoutGST = m.Item3,
                            ProfitWithoutGST = m.Item3,
                            BudgetWithoutGST = 0
                        });
                    });

                    return Ok(resultList.OrderBy(r => r.BranchId).ThenBy(r => r.Category));
                }
            
        }
    }
}
