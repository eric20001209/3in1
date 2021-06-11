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
    [Route("{hostId}/mreport/api/stocktake")]
    public class StockTakeReportController : Controller
    {
        private readonly farroContext _context;

        public StockTakeReportController(farroContext context)
        {
            _context = context;
        }

        [HttpPost()]
        public async Task<IActionResult> GetStockTake([FromBody] StockTakeFilterDto filter)
        {
            // Set NoTracking for ChangeTracker
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Make commandText ready
            string commandText = null;

            string branchIds = " and branch_id in (";
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

            commandText = @"with stock_take as
		                            (select sl.code, sl.branch_id
		                            , (select qty from stock_adj_log where id = max(sl.id)) as stock_adj
		                            , max(sl.log_time) as stock_take_time
		                            from stock_adj_log sl
		                            join branch b on sl.branch_id = b.id
		                            where sl.note not like '%transfer%'"
		                            + branchIds + @"
                                    and sl.log_time >= @startDateTime
		                            and sl.log_time < @endDateTime
		                            group by sl.code, sl.branch_id
		                            ),
	                            code_stock as
		                            (select sq.branch_id, c.code, c.cat, c.s_cat, c.ss_cat, c.name, sq.qty
		                            , c.inactive, c.is_service, c.hold_purchase
		                            from code_relations c
		                            join stock_qty sq on c.code = sq.code " + branchIds + @"
		                            where sq.qty <> 0
		                            and c.count_stock = 1
		                            and c.code > 1023"
                                    + categories + @"
		                            )
                            select (select name from branch where id = cs.branch_id) as branch
	                            , cs.code, cs.cat, cs.s_cat, cs.ss_cat, cs.name
	                            , cs.qty as stock_on_hand, st.stock_adj, st.stock_take_time
	                            , cs.inactive, cs.is_service, cs.hold_purchase
                            from code_stock cs 
	                            left join stock_take st on cs.code = st.code and cs.branch_id = st.branch_id
                            order by branch, cs.cat, cs.s_cat, cs.ss_cat, cs.name";

            // Run SQL Command
            using (var connection = (SqlConnection)_context.Database.GetDbConnection())
            {
                var command = new SqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@startDateTime", filter.StartDateTime);
                command.Parameters.AddWithValue("@endDateTime", filter.EndDateTime);

                _context.Database.OpenConnection();
                using (var result = await command.ExecuteReaderAsync())
                {
                    var resultToReturn = new List<StockTakeDto>();
                    while (result.Read())
                    {
                        resultToReturn.Add(new StockTakeDto
                        {
                            Branch = Convert.ToString(result["branch"]),
                            Code = Convert.ToString(result["code"]),
                            Cat = Convert.ToString(result["cat"]),
                            SCat = Convert.ToString(result["s_cat"]),
                            SSCat = Convert.ToString(result["ss_cat"]),
                            Name = Convert.ToString(result["name"]),
                            StockOnHand = result["stock_on_hand"] is DBNull 
                                            ? null as double? 
                                            : Math.Round(Convert.ToDouble(result["stock_on_hand"]), 2),
                            StockAdjustment = result["stock_adj"] is DBNull
                                            ? null as double?
                                            : Math.Round(Convert.ToDouble(result["stock_adj"]), 2),
                            StockTakeTime = result["stock_take_time"] is DBNull
                                            ? null as DateTime?
                                            : Convert.ToDateTime(result["stock_take_time"]),
                            IsInactive = Convert.ToBoolean(result["inactive"]),
                            IsService = Convert.ToBoolean(result["is_service"]),
                            IsHoldPurchase = Convert.ToBoolean(result["hold_purchase"])
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
