using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using eCommerce_API.Data;
using eCommerce_API.Dto;
using Microsoft.Extensions.Logging;
using eCommerce_API.Services;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using eCommerce_API.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using eCommerce_API_RST.Dto;
using System.Net.Mail;
using System.Data.SqlClient;

namespace eCommerce_API.Controllers
{
    [Route("{hostId}/ecom/api/dps")]   //handle dps payment
    [ApiController]
    public class DpsPaymentController : ControllerBase
    {
        rst374_cloud12Context _context; //= new rst374_cloud12Context();
        FreightContext _contextf; //= new FreightContext();
        private ILogger<DpsPaymentController> _logger;
        private readonly ISetting _isettings;
        private readonly IConfiguration _config;
        private readonly iMailService _mail;

        private string PxPayUserId = Startup.Configuration["PxPayUserId"]; // _contextf.Settings.Where(s => s.Cat == "DPS" && s.Name == "PxPayUserId").FirstOrDefault().Value;
        private string PxPayKey = Startup.Configuration["PxPayKey"]; // _contextf.Settings.Where(s => s.Cat == "DPS" && s.Name == "PxPayKey").FirstOrDefault().Value;
        private string sServiceUrl = Startup.Configuration["sServiceUrl"]; // _contextf.Settings.Where(s => s.Cat == "DPS" && s.Name == "sServiceUrl").FirstOrDefault().Value;
        public DpsPaymentController(ILogger<DpsPaymentController> logger
                                    , rst374_cloud12Context context
                                    , FreightContext contextf
                                    , ISetting isettings
                                    , IConfiguration config
                                    , iMailService mail)
        {
            _logger = logger;
            _context = context;
            _contextf = contextf;
            _isettings = isettings;
            _config = config;
            _mail = mail;
        }

        [HttpPost()]
        public IActionResult CreateDpsUI([FromBody] DpsInputDto dpsInput)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid input!");
            var orderId = dpsInput.OrderId;
            var returnUrl = dpsInput.ReturnUrl;

            var siteName = _config["CurrentSite"];
            string host_url = "http://" + HttpContext.Request.Host + siteName;
            string host_url1 = _config["ApiUrl"] + siteName; // "http://api171.gpos.nz/dollaritems";
            string sReturnUrlFail =
                                  host_url1 
                                  //"localhost:8088"
                                 + "/api/dps/result?t=result&ret=fail&orderId=" + orderId;
            string sReturnUrlSuccess = 
                                host_url1
                                //"localhost:8088"
                                + "/api/dps/result?action=paymentSuccess&orderId=" + orderId;

            //get order total
            var order = _context.Orders.Where(o => o.Id == Convert.ToInt32(orderId))
                        .Join(_context.Invoice,
                                o=>o.InvoiceNumber, 
                                i=>i.InvoiceNumber,
                                (o,i) => new {o.InvoiceNumber, o.Id, Total = i.Total ?? 0}).FirstOrDefault();
            decimal orderAmount = 0;
            if (order != null)
                orderAmount = order.Total;
            else
                return BadRequest();
            
            PxPay WS = new PxPay(sServiceUrl, PxPayUserId, PxPayKey);
            RequestInput input = new RequestInput();
            input.AmountInput = Math.Round(orderAmount,2).ToString();
            input.CurrencyInput = "NZD";
            input.MerchantReference = orderId;
            input.TxnType = "Purchase";
            input.UrlFail = sReturnUrlFail; 
            input.UrlSuccess = sReturnUrlSuccess;
            input.TxnData1 = returnUrl;

            Guid newOrderId = Guid.NewGuid();
            input.TxnId = newOrderId.ToString().Substring(0, 16);
            RequestOutput output = WS.GenerateRequest(input);
            if(output.valid == "1")
            {
                var result = output.Url;
                return Ok(result);
            }

            return NotFound();
        }

        [HttpGet("result")]
   //   [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> GetPaymentResult([FromQuery] string result, [FromQuery] string action, [FromQuery] string orderId)
        {
            PxPay WS = new PxPay(sServiceUrl, PxPayUserId, PxPayKey);
            ResponseOutput outputQs = WS.ProcessResponse(result);
            string DpsTxnRef = outputQs.DpsTxnRef;
            string sSuccess = outputQs.Success;
            string returnUrl = outputQs.TxnData1;
            if (returnUrl == "")
                returnUrl = "www.google.com";

            if (result == null)
                return NotFound();
            if (action != "paymentSuccess")
                return BadRequest("Sorry, payment unsuccessful. Please try later. ");


            //1. Check if payment info record
            if (await _context.DpsOutput.AnyAsync(d => d.OrderId.ToString() == orderId )) //&& d.TxnId == outputQs.TxnId))
            {
                //if info recorded
            }
            else
            {
                //Record dps payment status
                var connectionString = _config.GetConnectionString("rst374_cloud12Context");//_context.Database.GetDbConnection().ConnectionString;
                var conn = new SqlConnection(connectionString);
                conn.Open();
                var comm = new SqlCommand("insert into DpsOutput(OrderId, valid, AmountSettlement, AuthCode,CardName,CardNumber,DateExpiry,DpsTxnRef,Success," +
                    "ResponseText,DpsBillingId,CardHolderName,CurrencySettlement,TxnData1,TxnData2,TxnData3,TxnType, CurrencyInput,MerchantReference," +
                    "ClientInfo,TxnId, EmailAddress,BillingId,TxnMac )values('" + orderId + "','" + outputQs.valid + "', " +
                    "                                                                                   '" + outputQs.AmountSettlement + "', " +
                    "                                                                                   '" + outputQs.AuthCode + "'," +
                    "                                                                                   '" + outputQs.CardName + "'," +
                    "                                                                                   '" + outputQs.CardNumber + "', " +
                                    "                                                                   '" + outputQs.DateExpiry + "', " +
                                                    "                                                   '" + outputQs.DpsTxnRef + "', " +
                                                                    "                                   '" + outputQs.Success + "', " +
                                                                                    "                   '" + outputQs.ResponseText + "', " +
                    "                                                                                   '" + outputQs.DpsBillingId + "', " +
                    "                                                                                   '" + outputQs.CardHolderName + "', " +
                    "                                                                                   '" + outputQs.CurrencySettlement + "', " +
                    "                                                                                   '" + outputQs.TxnData1 + "', " +
                    "                                                                                   '" + outputQs.TxnData2 + "', " +
                    "                                                                                   '" + outputQs.TxnData3 + "', " +
                    "                                                                                   '" + outputQs.TxnType + "', " +
                    "                                                                                   '" + outputQs.CurrencyInput + "', " +
                    "                                                                                   '" + outputQs.MerchantReference + "', " +
                    "                                                                                   '" + outputQs.ClientInfo + "', " +
                    "                                                                                   '" + outputQs.TxnId + "', " +
                    "                                                                                   '" + outputQs.EmailAddress + "', " +
                    "                                                                                   '" + outputQs.BillingId + "', " +
                    "                                                                                   '" + outputQs.TxnMac + "'" +
                    "                                                                                        )");

                try
                {
                    comm.Connection = conn;
                    comm.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message + "\r\n" + $"Record dps payment info error, order id: {orderId}.");
                    return BadRequest(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }


            var isProcessed = _context.TranDetail.Any(td => td.Note == DpsTxnRef || td.PaymentRef == DpsTxnRef);
            if (isProcessed)
                // return BadRequest("Order " + orderId + " has been processed!");
                return Redirect(returnUrl);

            PropertyInfo[] properties = outputQs.GetType().GetProperties();
            foreach (PropertyInfo oPropertyInfo in properties)
            {
                if (oPropertyInfo.CanRead)
                {
                    string name = oPropertyInfo.Name;
                    string value = (string)oPropertyInfo.GetValue(outputQs, null);
                }
            }

            var order = _context.Orders.Where(o => o.Id == Convert.ToInt32(orderId))
                        .Join(_context.Invoice,
                                o => o.InvoiceNumber,
                                i => i.InvoiceNumber,
                                (o, i) => new { o.InvoiceNumber, o.Id, o.CardId, Total = i.Total ?? 0 }).FirstOrDefault();
            int cardId = 0;
            decimal orderAmount = 0;
            string customerEmail = "";

            var customer = await _context.Card.FirstOrDefaultAsync(c => c.Id == order.CardId);
            if (customer != null)
                customerEmail = customer.Email; 
            if (order != null)
            {
                cardId = order.CardId;
                orderAmount = order.Total;
            }

            int paymentmethod = _isettings.getIdByPaymentMethod("dps");// 14; // paymentMethodCast(paymentInfo.payment_method);

            if (sSuccess == "1")
            {
                var invoiceExists = await _context.TranInvoice.AnyAsync(ti=> ti.InvoiceNumber == order.InvoiceNumber);
				if (invoiceExists)
				{
					return BadRequest($"This order {orderId} has already been processed!");
				}
				var connect = _context.Database.GetDbConnection();
                var connectstring = _context.Database.GetDbConnection().ConnectionString;
                connect.Open();
                System.Data.Common.DbCommand dbCommand = connect.CreateCommand();

//              using (var dbContextTransaction = connect.BeginTransaction())
                {
                    //input payment info
                    try
                    {
                        var note = dbCommand.CreateParameter();
                        note.ParameterName = "@note";
                        note.DbType = System.Data.DbType.String;
                        note.Value = DpsTxnRef;             //insert dps ref to tran_detail tables

                        var Payment_Ref = dbCommand.CreateParameter();
                        Payment_Ref.ParameterName = "@payment_ref";
                        Payment_Ref.DbType = System.Data.DbType.String;
                        Payment_Ref.Value = DpsTxnRef;

                        var shop_branch = dbCommand.CreateParameter();
                        shop_branch.ParameterName = "@shop_branch";
                        shop_branch.DbType = System.Data.DbType.Int32;
                        shop_branch.Value = _isettings.getOnlineShopId();

                        var Amount = dbCommand.CreateParameter();
                        Amount.ParameterName = "@Amount";
                        Amount.DbType = System.Data.DbType.String;
                        Amount.Value = order.Total;


                        var nDest = dbCommand.CreateParameter();
                        nDest.ParameterName = "@nDest";
                        nDest.DbType = System.Data.DbType.Int32;
                        nDest.Value = "1116";

                        var staff_id = dbCommand.CreateParameter();
                        staff_id.ParameterName = "@staff_id";
                        staff_id.DbType = System.Data.DbType.Int32;
                        staff_id.Value = order.CardId.ToString();

                        var card_id = dbCommand.CreateParameter();
                        card_id.ParameterName = "@card_id";
                        card_id.DbType = System.Data.DbType.Int32;
                        card_id.Value = order.CardId.ToString();

                        var payment_method = dbCommand.CreateParameter();
                        payment_method.ParameterName = "@payment_method";
                        payment_method.DbType = System.Data.DbType.Int32;
                        payment_method.Value = paymentmethod;

                        var invoice_number = dbCommand.CreateParameter();
                        invoice_number.ParameterName = "@invoice_number";
                        invoice_number.DbType = System.Data.DbType.Int32;
                        invoice_number.Value = Convert.ToInt32(order.InvoiceNumber);

                        var amountList = dbCommand.CreateParameter();
                        amountList.ParameterName = "@amountList";
                        amountList.DbType = System.Data.DbType.String;
                        amountList.Value = orderAmount;


                        var return_tran_id = dbCommand.CreateParameter();
                        return_tran_id.ParameterName = "@return_tran_id";
                        return_tran_id.Direction = System.Data.ParameterDirection.Output;
                        return_tran_id.DbType = System.Data.DbType.Int32;

                        var return_exist_trans = dbCommand.CreateParameter();
                        return_exist_trans.ParameterName = "@return_exist_trans";
                        return_exist_trans.Direction = System.Data.ParameterDirection.Output;
                        return_exist_trans.DbType = System.Data.DbType.Boolean;

                        dbCommand.Parameters.Add(note);
                        dbCommand.Parameters.Add(Payment_Ref);
                        dbCommand.Parameters.Add(shop_branch);
                        dbCommand.Parameters.Add(Amount);
                        dbCommand.Parameters.Add(staff_id);
                        dbCommand.Parameters.Add(card_id);
                        dbCommand.Parameters.Add(payment_method);
                        dbCommand.Parameters.Add(invoice_number);
                        dbCommand.Parameters.Add(amountList);
                        dbCommand.Parameters.Add(return_tran_id);
                        dbCommand.Parameters.Add(return_exist_trans);
                        dbCommand.CommandText = "eznz_payment";
                        dbCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        var obj = await dbCommand.ExecuteNonQueryAsync();


                        //if trans exists, do not send invoice and order 
                        string sExist_trans = return_exist_trans.Value.ToString();//dbCommand.Parameters["@return_exist_trans"].Value.ToString();
                        if (sExist_trans == null || sExist_trans == "0" || sExist_trans == "")
                        {
                            try
                            {
                                //create invoice pdf
                                var host = "http://" + HttpContext.Request.Host;
                                string host1 = _config["ApiUrl"]; // "http://api171.gpos.nz";
                                var currentSite = _config["CurrentSite"];
                                try
                                {
                                    using (var client = new HttpClient())
                                    {
                                        client.BaseAddress = new Uri(host1);

                                        var responseTask = client.GetAsync(currentSite + "/api/invoice/pdf/" + orderId);
                                        responseTask.Wait();
                                        var getResult = responseTask.Result;
                                        if (getResult.IsSuccessStatusCode)
                                        {
                                            //send order to customer by email
                                            var myAttachment = new Attachment(_config["PdfPath"] + orderId + ".pdf");
                                            await _mail.sendEmail(customerEmail, "Invoice", "DoNotReply! <br><br> Dear customer: <br>Thank you for your order from<a href='http://dollaritems.co.nz/ecom'> dollaritems.co.nz</a><br> Your order invoice is in attachment.", myAttachment);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex.Message + "\r\n" + $"Send pdf to customer order unsuccessful, order id: {orderId}.");
                                    return BadRequest(ex.Message);
                                }

                                var manage = _config["Management"];
                                if (manage == "true")
                                { 
                                    //if payment susseed, send order to supplier
                                    try
                                    {
                                        using (var client = new HttpClient())
                                        {
                                            client.BaseAddress = new Uri(host1);

                                            var responseTask = client.GetAsync(currentSite + "/api/order/SendOrderToSupplier/" + orderId);
                                            responseTask.Wait();
                                            var getResult = responseTask.Result;
                                            if (getResult.IsSuccessStatusCode)
                                            {
                                                //set a key, if order send to supplier successfully
                                                var currentOrderDpsOutput = await _context.DpsOutput.FirstOrDefaultAsync(d => d.OrderId.ToString() == orderId);
                                                if (currentOrderDpsOutput == null)
                                                {
                                                    _logger.LogError($"Cannot find this order, when send order to supplier, order id: {orderId}.");
                                                    return NotFound();
                                                }
                                                currentOrderDpsOutput.OrderSent = true;
                                                _context.DpsOutput.Update(currentOrderDpsOutput);
                                                await _context.SaveChangesAsync();
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex.Message + "\r\n" + $"Send order to supplier unsuccessful, order id: {orderId}.");
                                        BadRequest(ex);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                                _logger.LogError(ex.Message + "\r\n" + $"Send order to supplier unsuccessful, order id: {orderId}.");
                                BadRequest(ex.Message);
                            }
                        }
                        else
                        {
                            _logger.LogError($"Transcation for this order exists, order id: {orderId}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        //                    dbContextTransaction.Rollback();
                        _logger.LogError(ex.Message + "\r\n" + $"Insert transaction into Db unsuccessful, order id: {orderId}.");
                        return BadRequest(ex);
                    }
                    finally
                    {
                        connect.Close();
                        connect.Dispose();
                    }
                }
            }
            else
            {
                _logger.LogError($"Payment for order {orderId} unsuccessful.");
            }
            return Redirect(returnUrl);
        }
    }
}