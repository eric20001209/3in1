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
    [Route("{hostId}/api/SalesCategoryReport")]
    public class SalesCategoryReportController : Controller
    {
        private readonly farroContext _context;

        public SalesCategoryReportController(farroContext context)
        {
            this._context = context;
        }
        [HttpPost("DateRange")]
        public async Task<IActionResult> GetSalesCategoryReport([FromBody]SalesCategoryReportFilterDto filter)
        {
                // Set NoTracking for ChangeTracker
                _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                var resultList = new List<SalesCategoryReportDto>();

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

                commandText = @"select c.cat as Category 
                                    , round(sum(s.commit_price * s.quantity), 2) as Revenue
                                    , round(sum((s.commit_price - s.supplier_price) * s.quantity), 2) as Profit
                                    , count(distinct i.invoice_number) as InvoiceQuantity
                                    , (select round(sum(s.quantity * s.commit_price), 2)
	                                    from invoice i
	                                    join branch b on i.branch = b.id
	                                    join sales s on i.invoice_number = s.invoice_number
	                                    left join code_relations cr on s.code = cr.code
	                                    where b.fax <> 'hidden4mreport'
	                                    and s.price_on_special <> 1
	                                    and cr.code <> '-1001'
	                                    and round((s.normal_price - commit_price), 6) > 0
	                                    and i.commit_date >= @startDateTime 
	                                    and i.commit_date < @endDateTime
                                        and i.tax=isnull(@tax,i.tax) 
	                                    " + branchIds + @"
	                                    and cr.cat = c.cat ) as MarkDown
                                    from invoice i
                                    join sales s on i.invoice_number = s.invoice_number
                                    join branch b on i.branch = b.id
                                    left join code_relations c on s.code = c.code
                                    where b.fax <> 'hidden4mreport'
                                    and s.code <> '-900001'
                                    and s.code <> '-1001'
                                    and i.commit_date >= @startDateTime 
                                    and i.commit_date < @endDateTime
                                    and i.tax=isnull(@tax,i.tax) 
                                    " + branchIds + @"
                                    " + categories + @"
                                    group by c.cat
                                    order by c.cat
                                    Collate Database_Default";

                commandTextForMinus1001 = @"select p.promo_cat as Category
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
                                            group by p.promo_cat";

                // Run SQL Command
                using (var connection = (SqlConnection)_context.Database.GetDbConnection())
                {
                    var commandForMinus1001 = new SqlCommand(commandTextForMinus1001, connection);
                    commandForMinus1001.Parameters.AddWithValue("@startDateTime", filter.StartDateTime);
                    commandForMinus1001.Parameters.AddWithValue("@endDateTime", filter.EndDateTime);
                if (filter.OnlineOrder)
                {
                    commandForMinus1001.Parameters.AddWithValue("@tax", 0);
                }
                else
                {
                    commandForMinus1001.Parameters.AddWithValue("@tax", DBNull.Value);
                }
                List<Tuple<string, decimal>> minus1001ResultList = new List<Tuple<string, decimal>>();

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
                            minus1001ResultList.Add(new Tuple<string, decimal>(
                                Convert.ToString(resultForMinus1001["Category"]),
                                Convert.ToDecimal(resultForMinus1001["Amount"] is DBNull ? 0 : resultForMinus1001["Amount"])
                            ));
                        }
                    }

                    using (var result = await command.ExecuteReaderAsync())
                    {
                        while (result.Read())
                        {
                            SalesCategoryReportDto dto = new SalesCategoryReportDto
                            {
                                Category = Convert.ToString(result["Category"]),
                                TotalWithoutGST = Convert.ToDecimal(result["Revenue"] is DBNull ? 0 : result["Revenue"]),
                                ProfitWithoutGST = Convert.ToDecimal(result["Profit"] is DBNull ? 0 : result["Profit"]),
                                // Temparorily BasketSpendWithoutGST is used as InvoiceQuantity
                                BasketSpendWithoutGST = Convert.ToDecimal(result["InvoiceQuantity"] is DBNull ? 0 : result["InvoiceQuantity"]),
                       
                                
                                MarkDownWithoutGST = Convert.ToDecimal(result["MarkDown"] is DBNull ? 0 : result["MarkDown"])
                            };
                           
                            resultList.Add(dto);
                        }
                    }
                    _context.Database.CloseConnection();

                    resultList.ForEach(r =>
                    {
                        if (minus1001ResultList.Any(m => m.Item1 == r.Category))
                        {
                            var amountForMinus1001 = minus1001ResultList.First(m => m.Item1 == r.Category).Item2;
                            r.TotalWithoutGST += amountForMinus1001;
                            r.ProfitWithoutGST += amountForMinus1001;
                            // Temparorily BasketSpendWithoutGST is used as InvoiceQuantity
                            r.BasketSpendWithoutGST = Math.Round(r.TotalWithoutGST / r.BasketSpendWithoutGST, 2);
                        }
                        else
                        {
                            // Temparorily BasketSpendWithoutGST is used as InvoiceQuantity
                            r.BasketSpendWithoutGST = Math.Round(r.TotalWithoutGST / r.BasketSpendWithoutGST, 2);
                        }
                    });

                    return Ok(resultList);
                }
            
        }
    }
}
