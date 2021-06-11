using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FarroAPI.Entities;
using FarroAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarroAPI.Controllers
{
    [Route("{hostId}/mreport/api/[controller]")]
    [ApiController]
    public class ItemReportController : ControllerBase
    {
        private readonly farroContext _context;
        public ItemReportController(farroContext context)
        {
            _context = context;
        }
        [HttpPost()]
        public async Task<IActionResult> ItemReport([FromBody] ItemReportFilterDto filter)
        {
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
            var commandText = @"with t as (select 
i.invoice_number,i.commit_date,s.code,s.name,s.normal_price,s.commit_price,s.price_on_special,s.quantity
,t1.invoice_promo,t1.promo_desc,
case when (p1.promo_type=4 or p1.promo_type=6) then ROUND(p1.volumn_discount_price_total/p1.volumn_discount_qty/1.15,2)  
when p1.promo_type=7 then ROUND(c.price1*p1.volumn_discount_price_total/t.promo_amount/1.15,2) end as 'promo_price',
case when s.price_on_special=1 then 'S' 
when (s.price_on_special=0 and s.commit_price<>s.normal_price) then 'M'
when (s.price_on_special=0 and s.commit_price=s.normal_price and t1.invoice_promo is null) then 'N' 
when (s.price_on_special=0 and s.commit_price=s.normal_price and t1.invoice_promo is not null) then 'P' 
end as 'sales_type'
 from (select * from invoice)as  i
left join sales s on s.invoice_number=i.invoice_number 
left join code_relations c on c.code=s.code
left join (
		select s.invoice_number,c.code ,pl.promo_id as 'invoice_promo',pl.promo_desc from sales s 
		left join invoice i on i.invoice_number=s.invoice_number
		join promotion_list pl on pl.promo_desc=SUBSTRING(s.name,11,len(s.name)) and pl.promo_active=1
		 join code_relations c on c.promo_id=pl.promo_id 
		where s.code=-1001 and pl.promo_type=4  
		union 
		select s.invoice_number,t.code,pl.promo_id as 'invoice_promo',pl.promo_desc  from sales s 
		left join invoice i on i.invoice_number=s.invoice_number
		join promotion_list pl on pl.promo_desc=SUBSTRING(s.name,11,len(s.name)) and pl.promo_active=1
		join (select distinct b.item_code as 'code',pg.promo_id from promotion_group pg		
			left join barcode b on b.barcode =pg.barcode
			where b.item_code is not null
			) as t on t.promo_id=pl.promo_id
		where s.code =-1001 and pl.promo_type <>4 ) as  t1 on t1.invoice_number=i.invoice_number and t1.code=s.code
left join (select p.promo_id,sum(c.price1) as 'promo_amount'
		from promotion_list p 
		left join promotion_group pg on pg.promo_id=p.promo_id
		left join barcode b on b.barcode=pg.barcode
		left join code_relations c on b.item_code=c.code
		group by p.promo_id having sum(c.price1) >0) as t on t.promo_id=t1.invoice_promo
left join promotion_list p1 on p1.promo_id=t1.invoice_promo
	where convert(date,commit_date)>=@startDateTime and convert(date,commit_date)<@endDateTime and branch in" + branchIds + @" )
		select c.name ,c.cat,c.s_cat,c.ss_cat,t1.*,(t1.totalQty*c.average_cost) as 'cost' from   (
		select  code,
		sum (t.quantity) as totalQty,
		sum (case when t.sales_type='p' then t.quantity*t.promo_price else t.quantity*t.commit_price end) as 'total',
		sum( case when t.sales_type='N'  then t.commit_price*t.quantity else 0 end) as 'normal',
		sum( case when t.sales_type='N'  then t.quantity else 0 end) as 'normalQty',
		sum( case when t.sales_type='S'  then t.commit_price*t.quantity else 0 end) as 'special',
		sum( case when t.sales_type='S'  then t.quantity else 0 end) as 'specialQty',
		sum( case when t.sales_type='M'  then t.commit_price*t.quantity else 0 end) as 'markdown',
		sum( case when t.sales_type='M'  then t.quantity else 0 end) as 'markdownQty',
		sum( case when t.sales_type='P'  then t.promo_price*t.quantity else 0 end) as 'promotion',
		sum( case when t.sales_type='P'  then t.quantity else 0 end) as 'promotionQty'
		 from t
		where t.code>0
		 group by t.code) as t1 
		left join code_relations c on c.code =t1.code  
        where 1=1 " + categories;

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


            // Run SQL Command
            using (var connection = (SqlConnection)_context.Database.GetDbConnection())
            {
                var command = new SqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@startDateTime", filter.StartDateTime);
                command.Parameters.AddWithValue("@endDateTime", filter.EndDateTime);

                _context.Database.OpenConnection();
                using (var result = await command.ExecuteReaderAsync())
                {
                    var resultToReturn = new List<ItemReportDto>();
                    while (result.Read())
                    {
                        resultToReturn.Add(new ItemReportDto
                        {              
                            //Date = Convert.ToDateTime(result["date"]),
                            code = Convert.ToString(result["code"]),
                            name = Convert.ToString(result["name"]),
                            cat = Convert.ToString(result["cat"]),
                            s_cat = Convert.ToString(result["s_cat"]),
                            ss_cat = Convert.ToString(result["ss_cat"]),
                            total = Convert.ToDouble(result["total"]),
                            totalQty = Convert.ToDouble(result["totalQty"]),
                            markdown = Convert.ToDouble(result["markdown"]),
                            markdownQty = Convert.ToDouble(result["markdownQty"]),
                            normal = Convert.ToDouble(result["normal"]),
                            normalQty = Convert.ToDouble(result["normalQty"]),
                            special = Convert.ToDouble(result["special"]),
                            specialQty = Convert.ToDouble(result["specialQty"]),
                            promotion = Convert.ToDouble(result["promotion"]),
                            promotionQty = Convert.ToDouble(result["promotionQty"]),
                            cost = Convert.ToDouble(result["cost"]),
                        });
                    }
                    return Ok(resultToReturn);
                }
                _context.Database.CloseConnection();
                return NotFound();
            }

            return Ok();
        }
    }
}
