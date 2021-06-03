using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eCommerce_API.Dto;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace eCommerce_API_RST.Services.Utility
{
	public class PdfInvoiceTemplateGenerator
	{
        private readonly IOrder _iorder;
        private readonly IConfiguration _config;
        public PdfInvoiceTemplateGenerator(IOrder iorder,
                                          IConfiguration config)
        {
            _iorder = iorder;
            _config = config;
        }
        public string GetHTMLString(int orderId)
        {
            PdfInvoiceDataStorage dataStorage = new PdfInvoiceDataStorage(_iorder);

            var myInvoice = dataStorage.getInvoice(orderId);
            var sb = new StringBuilder();

            sb.Append(@"
                        <html>
                            <head>
                            <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
                            </head>
                            <body style='tab-inerval:36pt;'>
                                <div class='logo'><img width='100' height='100' src='http://dollaritems.co.nz/i/logo.png'></div>
                                <div class='header'>Dollar items</div>");
            sb.Append(@"<div class='shipping_gst'>");
            sb.Append(myInvoice.gst);
            sb.Append(@"</div><div class='shipping_name'>  ");
            sb.Append(myInvoice.shipto.name);

            sb.Append(@"</div><div class='shipping_invoicennum'>Inv Num ： ");
            sb.Append(myInvoice.inovice_number);
            sb.Append(@"</div><div class='shipping_address'>");
            sb.Append(myInvoice.shipto.address1);
            sb.Append(@"</div><div class='shipping_commitdate'>Date : ");
            sb.Append(myInvoice.commit_date.ToString("dd-MM-yyyy"));
            sb.Append(@"</div><div class='shipping_suburb'>");
            sb.Append(myInvoice.shipto.address2);
            sb.Append(@"</div><div class='shipping_pobox'>");
            sb.Append(myInvoice.po_box);
            sb.Append(@"</div><div class='shipping_city'>");
            sb.Append(myInvoice.shipto.city);
            sb.Append(@"</div><div class='shipping_phone'>");
            sb.Append(myInvoice.shipto.phone);
            sb.Append(@"</div><div class='shipping_note'>");
            //sb.Append(myInvoice.shipto.note);
            sb.Append(@"&nbsp;
                                </div>
                                <table>");
            sb.Append(@"
                                    <tr><td style='height:120px'></td></tr>
                                    <tr>
                                        <th></th>
                                        <th>Name</th>
                                        <th>Quantity</th>
                                        <th>Total</th>
                                    </tr>
                                    <tr><td colspan=4><hr></td></tr>");

            foreach (var emp in myInvoice.sales_items)
            {
                sb.AppendFormat(@"<tr>
                                    <td><img src='{0}' width=80 height=80></td>    
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                  </tr>", emp.ImageUrl, emp.ItemName, emp.Quantity, emp.OrderTotal.ToString("C"));
                sb.AppendFormat(@"<tr><td colspan=4></td></tr>");                  
            }
            sb.AppendFormat(@"<tr><td colspan=4 height=10px><hr></td></tr>");
            // sb.Append(@"
            //                    <tr><td colspan=4 height=24> </td></tr>
            //                    <tr><td colspan=3 align=right>Tax:</td><td>");
            //sb.Append(myInvoice.tax.ToString("C"));

            //sb.Append(@"</td></tr><tr><td colspan=3 align=right>Freight:</td><td>");
            //sb.Append(myInvoice.freight.ToString("C"));
            //sb.Append(@"

            //                    </td></tr><tr><td colspan=3 align=right>Total:</td><td>");
            //sb.Append(myInvoice.total.ToString("C"));
            //sb.Append(@"</td></tr>

            sb.Append(@"</table>");

            sb.Append(@"<div class='subtotal'>Sub-Total : ");
            sb.Append(myInvoice.sub_total.ToString("C"));
            sb.Append(@"</div><div class='tax'>Tax : ");
            sb.Append(myInvoice.tax.ToString("C"));
            sb.Append(@"</div><div class='freight'>Freight ： ");
            sb.Append(myInvoice.freight.ToString("C"));
            sb.Append(@"</div><div class='total'>Total ： ");
            sb.Append(myInvoice.total.ToString("C"));
            sb.Append(@"</div>");
			//sb.Append(@"<div class='terms'>Return Terms and Conditions:<br>  Please return this receipt as proof of purchase for exchange or rerund. You are welcome to return your purchase
   //                  within 30 days for refund or exchange or within 90 days for store credit. Returned items must be unused and in original
   //                  , unopened, undamaged and resalable condition. However in the case where an item has been lightly sampled, we are happy to provide a store
   //                   credit for your return. </div>");

			sb.Append(@"</body>
                        </html>");
            return sb.ToString();
        }
    }
}
