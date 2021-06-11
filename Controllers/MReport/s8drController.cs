using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FarroAPI.Entities;
using FarroAPI.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace FarroAPI.Controllers
{
    [Route("{hostId}/mreport/api/[controller]")]
    [ApiController]
    public class s8drController : ControllerBase
    {
        private readonly farroContext _context;

        public s8drController(farroContext context)
        {
            _context = context;
        }

        [HttpPost()]
        public async Task<IActionResult> GetS8daysReport([FromBody] Sales8DaysFilterDto filter)
        {
            // Set NoTracking for ChangeTracker
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

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
                branchName = (from o in _context.Branch
                              where o.Id == filter.BranchId
                              select o.Name).FirstOrDefault<string>();
                if (string.IsNullOrEmpty(branchName))
                {
                    branchName = "invalied branch";
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
            // Make commandText ready
            string commandText = null;
            commandText = @" with t1 as (select s.code,convert(date, i.commit_date) as 'date',sum(s.commit_price*s.quantity) as 'total'
                         ,sum(s.quantity) as 'total_qty'
                        ,sum( case when s.commit_price<>s.normal_price and s.price_on_special=0 then s.commit_price*s.quantity else 0 end) as 'markdown'
                        ,sum( case when s.commit_price<>s.normal_price and s.price_on_special=0 then s.quantity else 0 end) as 'markdown_qty'
                        ,sum( case when s.price_on_special=1 and s.commit_price<>s.normal_price then s.commit_price*s.quantity else 0 end) as 'special'
                        ,sum( case when s.price_on_special=1 and s.commit_price<>s.normal_price then s.quantity else 0 end) as 'special_qty'
                        ,sum( case when s.commit_price=s.normal_price  then s.commit_price*s.quantity else 0 end) as 'normal'
                        ,sum( case when s.commit_price=s.normal_price  then s.quantity else 0 end) as 'normal_qty'
                           from sales s 
                         left join invoice i on i.invoice_number=s.invoice_number
                         where  convert(date, i.commit_date)>=@startDateTime and  convert(date, i.commit_date)<@endDateTime And branch in" + branchIds + @"
                         group by convert(date, i.commit_date),code),
                         t2 as (
                        select *,0 as 'total',0 as 'total_qty',0 as 'markdown',0 as 'markdown_qty',0 as 'special', 0 as 'special_qty' ,0 as 'normal' , 0 as 'normal_qty' from 
                         (select * from (SELECT DATEADD(DAY, t1.number,convert(date,@startDateTime))as date
                        FROM master..spt_values t1
                        WHERE t1.[type] = 'p')as y where convert(date,y.date)<@endDateTime) as t1
                        cross join";
            if (filter.showAllItem)
            {
                commandText += "(select code from code_relations )as dateTable)";
            }
            else
            {
                commandText += "(select distinct code from sales s left join invoice i on i.id=s.invoice_number where convert(date, i.commit_date)>=@startDateTime and  convert(date, i.commit_date)<@endDateTime And branch in" + branchIds + @" ) as dateTable)";
            }
            commandText +=@" select distinct
                        t2.date,t2.code,
                        isnull(t1.total,t2.total) as 'total',
                        isnull(t1.total_qty,t2.total_qty) as 'total_qty',
                        isnull(t1.markdown,t2.markdown) as 'markdown',
                        isnull(t1.markdown_qty,t2.markdown_qty) as 'markdown_qty',
                        isnull(t1.special,t2.special) as 'special',
                        isnull(t1.special_qty,t2.special_qty) as 'special_qty',
                        isnull(t1.normal,t2.normal) as 'normal',
                        isnull(t1.normal_qty,t2.normal_qty) as 'normal_qty',
                        c.name,c.cat,c.s_cat,c.ss_cat,c.supplier
                          from t2
                          left join t1 on t1.date=t2.date and t1.code=t2.code
                        left join code_relations c on c.code =t2.code
        
                           where t2.code>0 " + categories;
            if (!String.IsNullOrEmpty(filter.keyword))
            {
                commandText += " AND (c.supplier_code LIKE '%" + filter.keyword + "%' OR c.name LIKE '%" + filter.keyword + "%' ";
                if (Regex.IsMatch(filter.keyword, @"^[0-9]*$"))
                {
                    commandText += " OR c.code = " + filter.keyword;
                }
                commandText += ")";
            }
            if (!String.IsNullOrEmpty(filter.supplierId))
            {
                commandText += " AND c.supplier_code =N'" + filter.supplierId + "'";

            }
            commandText += "order by t2.code,t2.date";
 



            // Run SQL Command
            using (var connection = (SqlConnection)_context.Database.GetDbConnection())
            {
                var command = new SqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@startDateTime", filter.StartDateTime);
                command.Parameters.AddWithValue("@endDateTime", filter.EndDateTime);

                _context.Database.OpenConnection();
                using (var result = await command.ExecuteReaderAsync())
                {
                    var resultToReturn = new List<Sales8DaysDto>();
                    while (result.Read())
                    {
                        resultToReturn.Add(new Sales8DaysDto
                        {
                            Branch = branchName,
                            Date = Convert.ToDateTime(result["date"]),
                            code = Convert.ToString(result["code"]),
                            name =Convert.ToString(result["name"]),
                           cat=Convert.ToString(result["cat"]),
                           s_cat=Convert.ToString(result["s_cat"]),
                           ss_cat=Convert.ToString(result["ss_cat"]),
                           total=Convert.ToDouble(result["total"]),
                            totalQty = Convert.ToDouble(result["total_qty"]),
                            markdown = Convert.ToDouble(result["markdown"]),
                            markdownQty = Convert.ToDouble(result["markdown_qty"]),
                           normal = Convert.ToDouble(result["normal"]),
                            normalQty = Convert.ToDouble(result["normal_qty"]),
                            special = Convert.ToDouble(result["special"]),
                            specialQty = Convert.ToDouble(result["special_qty"]),



                        });
                    }
                    return Ok(resultToReturn);
                }
                _context.Database.CloseConnection();
                return NotFound();
            }

            return NotFound();
        }
     }
}
