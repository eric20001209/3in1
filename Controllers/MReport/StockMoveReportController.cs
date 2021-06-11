using FarroAPI.Entities;
using FarroAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FarroAPI.Controllers
{
    [Route("{hostId}/mreport/api/StockMove")]
    public class StockMoveReportController : Controller
    {
        private readonly farroContext _context;

        public StockMoveReportController(farroContext context)
        {
            _context = context;
        }
        private double? NullValue(Object value)
        {
            double? result = value is DBNull ? 0: Math.Round(Convert.ToDouble(value), 2);
            return result;
        }
        [HttpPost()]
        public async Task<IActionResult> GetStockMove([FromBody] StockMoveFilterDto filter)
        {
            // Set NoTracking for ChangeTracker
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Make commandText ready
            string commandText = null;

          
            string branchIds = "(";
            string branchName = "All";
            if (!filter.BranchId.HasValue)
            {
                var validBranchIds = _context.Branch
                                    .Where(b => b.Fax.ToLower() != "hidden4mreport" && b.Id != 15)
                                    .Select(b => b.Id)
                                    .ToArray();
                branchIds += string.Join(", ", validBranchIds);

            }
            else
            {
                branchIds += filter.BranchId.Value;
               branchName=(from o in _context.Branch
                          where o.Id == filter.BranchId
                          select o.Name).FirstOrDefault<string>();
                if (string.IsNullOrEmpty(branchName))
                {
                    branchName="invalied branch";
                }
            }
            branchIds += ") ";
      
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


            commandText = @"SET DATEFORMAT dmy 
                    SELECT c.code, c.name, c.cat, c.s_cat, c.ss_cat , c.supplier_price, c.special_cost ,c.skip ,c.sor,c.auto_zero_stock,";
            commandText += "@branchName as branch ";
            commandText +=@", ISNULL((SELECT code FROM code_relations 
                    WHERE code = c.code AND GETDATE() BETWEEN c.special_cost_start_date AND c.special_cost_end_date), 0) AS in_special , 
                     (SELECT SUM(d.qty) FROM dispatch d
                    WHERE d.received = 1 AND d.code = c.code And branch in" + branchIds + @" AND d.date_received BETWEEN @startDateTime AND @endDateTime ) AS purchase , 
                     (SELECT SUM(s.quantity) FROM sales s JOIN invoice i ON i.invoice_number = s.invoice_number
                    WHERE s.code = c.code And branch in" + branchIds + @" AND i.commit_date BETWEEN @startDateTime AND @endDateTime ) AS sales , 
                     (SELECT SUM(w.qty) FROM waste w 
                    WHERE w.code = c.code And branch_id in" + branchIds + @" AND w.record_date BETWEEN @startDateTime AND @endDateTime ) AS waste , 
                     (SELECT SUM(q.qty) FROM stock_qty q 
                    WHERE q.code = c.code And branch_id in" + branchIds + @" ) AS stock , 
                     (SELECT SUM(adj.qty) FROM stock_adj_log adj 
                     WHERE adj.code = c.code And branch_id in" + branchIds + @" AND adj.log_time BETWEEN @startDateTime AND @endDateTime ) AS adj ,
                    (SELECT SUM(stri.qty_transfered) FROM stock_transfer_request_item stri JOIN stock_transfer_request str ON str.id = stri.id WHERE stri.code = c.code And str.to_branch_id in" + branchIds + @"AND str.date_finished BETWEEN @startDateTime AND @endDateTime  ) AS trans_out ,
                    (SELECT SUM(stri.qty_transfered) FROM stock_transfer_request_item stri JOIN stock_transfer_request str ON str.id = stri.id WHERE stri.code = c.code And str.from_branch_id in" + branchIds + @"AND str.date_finished BETWEEN @startDateTime AND @endDateTime) AS trans_in 
                    FROM code_relations c    WHERE 1 = 1" + categories ;

            if (!String.IsNullOrEmpty(filter.Keyword))
            {
                commandText += " AND (c.supplier_code LIKE '%" + filter.Keyword + "%' OR c.name LIKE '%" + filter.Keyword + "%' ";
                if (Regex.IsMatch(filter.Keyword, @"^[0-9]*$"))
                {
                    commandText += " OR c.code = " + filter.Keyword ;
                }
                commandText += ")";
            }
            if (!String.IsNullOrEmpty(filter.DealerId))
            {
                commandText += " AND c.supplier_code =N'" + filter.DealerId+"'";
           
            }
            //if (filter.Skip == true)
            //{
            //    commandText += " AND c.skip=1 ";
            //}else if(filter.Skip == false)
            //{
            //    commandText += " AND c.skip=0 ";
            //}
            commandText += @" ORDER BY c.cat, c.s_cat, c.ss_cat, c.name";
 
            // Run SQL Command
            using (var connection = (SqlConnection)_context.Database.GetDbConnection())
            {
                var command = new SqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@startDateTime", filter.StartDateTime);
                command.Parameters.AddWithValue("@endDateTime", filter.EndDateTime);
                command.Parameters.AddWithValue("@branchName", branchName);
             
                _context.Database.OpenConnection();
                using (var result = await command.ExecuteReaderAsync())
                {
                    var resultToReturn = new List<StockMoveDto>();
                    while (result.Read())
                    {
                        resultToReturn.Add(new StockMoveDto
                        {
                            Branch = Convert.ToString(result["branch"]),
                            Code = Convert.ToString(result["code"]),
                            Cat = Convert.ToString(result["cat"]),
                            SCat = Convert.ToString(result["s_cat"]),
                            SSCat = Convert.ToString(result["ss_cat"]),
                            Name = Convert.ToString(result["name"]),
                            UnitCost = NullValue(result["supplier_price"]),
                            InPurchase = NullValue(result["purchase"]),
                            OutSales = NullValue(result["sales"]),
                            Waste= NullValue(result["waste"]),
                            Adjustment = NullValue(result["adj"]),
                            TransIn = NullValue(result["trans_in"]),
                            TransOut = NullValue(result["trans_out"]),
                            Stock = NullValue(result["stock"]),
                            SOR = Convert.ToBoolean(result["sor"]),
                            Skip = Convert.ToBoolean(result["skip"]),
                            AutoZeroStock = Convert.ToBoolean(result["auto_zero_stock"]),
                            StockValue= NullValue(result["stock"])*NullValue(result["supplier_price"])
                            //                           public int? InPurchase { get; set; }
                            //public int? OutSales { get; set; }
                            //public int? Waste { get; set; }
                            //public double? Adjustment { get; set; }
                            //public int? TransIn { get; set; }
                            //public int? TransOut { get; set; }
                            //public int Stock { get; set; }
                            //StockOnHand = result["stock_on_hand"] is DBNull 
                            //                ? null as double? 
                            //                : Math.Round(Convert.ToDouble(result["stock_on_hand"]), 2),
                            //StockAdjustment = result["stock_adj"] is DBNull
                            //                ? null as double?
                            //                : Math.Round(Convert.ToDouble(result["stock_adj"]), 2),
                            //StockMoveTime = result["stock_take_time"] is DBNull
                            //                ? null as DateTime?
                            //                : Convert.ToDateTime(result["stock_take_time"]),
                            //IsInactive = Convert.ToBoolean(result["inactive"]),
                            //IsService = Convert.ToBoolean(result["is_service"]),
                            //IsHoldPurchase = Convert.ToBoolean(result["hold_purchase"])
                        });
                    }
                    return Ok(resultToReturn);
                }
                _context.Database.CloseConnection();
                return NotFound();
            }
        }
    }


    
}
