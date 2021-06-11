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
    [Route("{hostId}/mreport/api/SalesSubSubCategoryReport")]
    public class SalesSubSubCategoryReportController : Controller
    {
        private readonly farroContext _context;

        public SalesSubSubCategoryReportController(farroContext context)
        {
            this._context = context;
        }
        [HttpPost("DateRange")]
        public async Task<IActionResult> GetSalesSubSubCategoryReport([FromBody] SalesSubSubCategoryReportFilterDto filter)
        {
                // Set NoTracking for ChangeTracker
                _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                var resultList = new List<SalesSubSubCategoryReportDto>();

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
                else
                {
                    branchIds = " and b.fax <> 'Hidden4MReport' ";
                }

                string subCategories = "";
                if (filter.SubCategories.Count > 0)
                {
                    subCategories = " and (";
                    for (int i = 0; i < filter.SubCategories.Count; i++)
                    {
                        subCategories += @"(s.cat = '" + filter.SubCategories[i].CategoryName + @"' ";
                        if (filter.SubCategories[i].SubCategoryName.ToLower() != "all")
                            subCategories += @" and s.s_cat = '" + filter.SubCategories[i].SubCategoryName + @"' ";
                        subCategories += @")";

                        if (i < filter.SubCategories.Count - 1)
                            subCategories += " or ";
                        else
                            subCategories += ") ";
                    }
                }

                commandText = @"select b.id as BranchId
                            , s.cat as Category
                            , s.s_cat as SubCategory
                            , s.ss_cat as SubSubCategory
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
                            and i.tax=isnull(@tax,i.tax) 
                            and i.commit_date < @endDateTime
                            and s.code <> '-900001'"

                            + subCategories + branchIds +
                            @" group by b.id, s.cat, s.s_cat, s.ss_cat
                            order by b.id, s.cat, s.s_cat, s.ss_cat";

                // Run SQL Command
                using (var connection = (SqlConnection)_context.Database.GetDbConnection())
                {

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
                    using (var result = await command.ExecuteReaderAsync())
                    {
                        while (result.Read())
                        {
                            SalesSubSubCategoryReportDto dto = new SalesSubSubCategoryReportDto
                            {
                                BranchId = Convert.ToInt32(result["BranchId"]),
                                Category = result["Category"].ToString(),
                                SubCategory = result["SubCategory"].ToString(),
                                SubSubCategory = result["SubSubCategory"].ToString(),
                                AmountWithoutGST = Convert.ToDecimal(result["Sales"] is DBNull ? 0 : result["Sales"]),
                                ProfitWithoutGST = Convert.ToDecimal(result["Profit"] is DBNull ? 0 : result["Profit"]),
                                BudgetWithoutGST = Convert.ToDecimal(result["Budget"] is DBNull ? 0 : result["Budget"])
                            };
                            dto.BudgetWithoutGST = Math.Round(dto.BudgetWithoutGST, 2);
                            resultList.Add(dto);
                        }
                    }
                    _context.Database.CloseConnection();

                    return Ok(resultList);
                }
            
        }
    }
}
