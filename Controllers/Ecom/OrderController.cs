using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using eCommerce_API.Data;
using eCommerce_API.Dto;
using eCommerce_API.Models;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using eCommerce_API_RST.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using eCommerce_API.Services;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using NLog.Web;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using eCommerce_API_RST.Dto;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;


namespace eCommerce_API.Controllers
{
    [Route("{hostId}/ecom/api/order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        rst374_cloud12Context _context;//= new rst374_cloud12Context();
        private readonly IItem _iitem;
        private readonly ISetting _isettings;
        private readonly IOrder _iorder;
        private ILogger<OrderController> _logger;
        private IConfiguration _config;
        private iMailService _mail;

        public OrderController(ILogger<OrderController> logger, 
                                IItem iitem, 
                                rst374_cloud12Context context, 
                                ISetting isettings, 
                                IOrder iorder, 
                                IConfiguration config,
                                iMailService mail)
        {
            _logger = logger;
            _iitem = iitem;
            _context = context;
            _isettings = isettings;
            _iorder = iorder;
            _config = config;
            _mail = mail;
        }

        [HttpGet()]
        public IActionResult getOrders([FromQuery] int id, [FromQuery] bool? invoiced, [FromQuery] bool? paid,[FromQuery] int? status, [FromQuery] string customer,
            [FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] string keyword)
        {
            var filter = new OrderFilterDto();
            filter.id = id;
            filter.inoviced = invoiced;
            filter.paid = paid;
            filter.status = status;
            filter.customer = customer;
            if(start != DateTime.MinValue )
                filter.start = start;
            if(end != DateTime.MinValue)
                filter.end = end;
            filter.keyword = keyword;

            var orderList = orderlist(filter);
            return Ok(orderList);
        }

        private List<OrderDto> orderlist([FromBody] OrderFilterDto filter)
        {

            _context.ChangeTracker.QueryTrackingBehavior
                = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            OrderListDto orderListDto = new OrderListDto();
            var orderList = (from o in _context.Orders
                             join i in _context.Invoice on o.InvoiceNumber equals i.InvoiceNumber into oi
                             from i in oi.DefaultIfEmpty()

                             join si in _context.ShippingInfo on o.Id equals si.orderId into sio
                             from si in sio.DefaultIfEmpty()

                             where o.CardId == filter.id
                              //&& (filter.paid != null ? i.Paid == filter.paid : true)
                              //&& (filter.status != null ? i.Status == filter.status : true)
                              //&& (filter.start != null ? o.RecordDate >= filter.start : true) && (filter.end != null ? o.RecordDate <= filter.end : true)
                              //&& (filter.keyword != null ? i.InvoiceNumber.ToString().Contains(filter.keyword) || o.PoNumber.ToString().Contains(filter.keyword) : true)
                             select new OrderDto
                             {
                                 id= o.Id,
                                 card_id = o.CardId,
                                 branch = o.Branch,
                                 po_number = o.PoNumber,
                                 status = _isettings.getOrderStatus(Convert.ToInt32(o.Status)), //o.Status.ToString(),
                                 invoice_number = o.InvoiceNumber,
                                 TotalAmount_GstIncl = i.Total,
                                 TotalAmount_GSTExcl = i.Price,
                                 GstAmount = i.Tax,
                                 record_date = o.RecordDate,
                                 shipto = o.Shipto,
                                 special_shipto = o.SpecialShipto,
                                 date_shipped = o.DateShipped,
                                 freight = o.Freight,
                                 ticket = o.Ticket,
                                 shipping_method = o.ShippingMethod,
                                 payment_type = o.PaymentType,
                                 paid = _iorder.getOrderPaymentStatus(o.Id),//o.Paid,
                                 receiver_name = si.receiver,
                                 receiver_phone = si.receiver_phone,
                                 is_web_order = true,
                                 freightInfo = _iorder.getSupplierShippingInfo(o.Id),
                                 web_order_status = o.WebOrderStatus
                             }).ToList()
                             ;

            //var itemCount = orderList.Count();
            //var pageCount = (int)Math.Ceiling(itemCount / (double)pagination.PageSize);
            //var finalList =
            //        orderList
            //        .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            //        .Take(pagination.PageSize);

            //foreach (var o in orderList)
            //{
            //    try
            //    {
            //        if (o.web_order_status == 0)
            //            o.web_order_status = 1;
            //        o.status = getOrderStatus(o.web_order_status);
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogCritical($"error occur when changing order status!",ex);
                    
            //    }
            //}

            //orderListDto.Orders = finalList;
            //orderListDto.CurrentPage = pagination.PageNumber;
            //orderListDto.PageSize = pagination.PageSize;
            //orderListDto.PageCount = pageCount;
            //orderListDto.ItemCount = itemCount;
            return orderList;
        }

        private string getOrderStatus(int status_id)
        {
            //if (status_id == null)
            //    return "1";
            var status = _context.Enum.Where(e => e.Id == status_id && e.Class =="web_order_status").FirstOrDefault().Name;
            return status;
        }

        [HttpPut("shipping/{order_id}")]
        public async Task<IActionResult> updateOrderShipping(int? order_id)
        {
            var orderToUpdate = _context.Orders.Where(o => o.Id == order_id).FirstOrDefault();
            if (orderToUpdate == null)
                return NotFound();
            var shippingStatus = orderToUpdate.Status;
            if (shippingStatus == 5)
                orderToUpdate.Status = 6;    //from shipping to received
            else if (shippingStatus == 6)
                orderToUpdate.Status = 5;    //from received to shipping
            try {
                _context.Update(orderToUpdate);
                await _context.SaveChangesAsync();
            }
            catch(Exception e)
            {
                throw e;
            }
            return NoContent();
        }

        [HttpGet("{order_id}")]
        public async Task<IActionResult> orderDetail(int? order_id)
        {
            if (order_id == null)
            {
                _logger.LogInformation($"Order with id {order_id} was null.");
                return NotFound();
            }

            if (!await _context.Orders.AnyAsync(o => o.Id == order_id))
            {
                _logger.LogInformation($"Order with id {order_id} wasn't found.");
                return NotFound();
            }
            else
            {
                _logger.LogInformation($"Order with id {order_id} was found.");
            }

            try
            {
                var orderDetail = new OrderDetailDto();

                var myOrder = _context.Orders.Where(o => o.Id == order_id)
                    .Include(b => b.shippinginfo)
                    .Include(b=> b.invoiceFreight)
                    .Join(_context.Invoice.Select(i=>new { i.InvoiceNumber, i.Paid, i.PaymentType,i.Freight, i.Total,i.Tax,i.Price}),
                    (b=>b.InvoiceNumber),
                    (i=>i.InvoiceNumber),
                    (b,i)=> new {b.shippinginfo,b.invoiceFreight, b.Id, b.ShippingMethod, b.InvoiceNumber,b.PoNumber,b.CustomerGst, b.CardId, i.Freight, OrderTotal = i.Price, i.Total, i.Tax, b.WebOrderStatus, b.Status, i.Paid, i.PaymentType  })

                    .Join(_context.Enum.Where(e=>e.Class == "payment_method"),
                    (b=>(int)b.PaymentType),
                    (e=>e.Id),
                    (b, e) => new { b.shippinginfo, b.invoiceFreight, b.Id, b.ShippingMethod, b.InvoiceNumber, b.PoNumber, b.CustomerGst, b.CardId, b.Freight, b.OrderTotal,b.Total, b.Tax, b.WebOrderStatus, b.Status, b.Paid, PaymentType = e.Name })

                    //.Join(_context.Enum.Where(e => e.Class == "web_order_status"),
                    //(b => b.WebOrderStatus),
                    //(e => e.Id),
                    //(b, e) => new { b.shippinginfo, b.invoiceFreight, b.Id, b.ShippingMethod, b.InvoiceNumber, b.PoNumber, b.CustomerGst, b.CardId, b.Freight, b.OrderTotal, b.Total, b.Tax, WebOrderStatus = e.Name, b.Paid, b.PaymentType })

                    .FirstOrDefault();
                    if (myOrder == null)
                    {
                        _logger.LogInformation($"Order with id {order_id} was null.");
                        return NotFound();
                    }

                var customerGst = myOrder.CustomerGst;

                var orderItem = _context.OrderItem.Where(o => o.Id == order_id)
                    .Select(oi => new OrderItemDto
                    {
                        Kid = oi.Kid,
                        Id = oi.Id,
                        Code = oi.Code,
                        SupplierCode = oi.SupplierCode,
                        Quantity = oi.Quantity,
                        ItemName = oi.ItemName,
                        ItemNameCn = oi.ItemNameCn,
                        CommitPrice = oi.CommitPrice,
                        PriceGstInc = Math.Round(oi.CommitPrice * Convert.ToDecimal(1+customerGst),2),
                        Cat = oi.Cat,
                        Note = oi.Note
                    }).ToListAsync();

                var shippingInfo = myOrder.shippinginfo
                    .Select(s => new ShippingInfoDto
                    {
                        id = s.id,
                        sender = s.sender,
                        orderId = s.orderId,
                        sender_phone = s.sender_phone,
                        sender_address = s.sender_address,
                        sender_city = s.sender_city,
                        sender_country = s.receiver_country,
                        receiver = s.receiver,
                        receiver_company = s.receiver_company,
                        receiver_address1 = s.receiver_address1,
                        receiver_address2 = s.receiver_address2,
                        receiver_address3 = s.receiver_address3,
                        receiver_city = s.receiver_city,
                        receiver_country = s.receiver_country,
                        receiver_phone = s.receiver_phone,
                        receiver_contact = s.receiver_contact,
                        note = s.note
                    })
                    .FirstOrDefault();

                if (shippingInfo == null)
                {
                    shippingInfo = new ShippingInfoDto
                    {
                        sender = "",
                        sender_phone = "",
                        sender_address = "",
                        sender_city = "",
                        sender_country = "",
                        receiver = "",
                        receiver_company = "",
                        receiver_address1 = "",
                        receiver_address2 = "",
                        receiver_address3 = "",
                        receiver_city = "",
                        receiver_country = "",
                        receiver_phone = "",
                        receiver_contact = "",
                        note = ""
                    };
                }



                List<FreightInfoDto> freightInfo = myOrder.invoiceFreight
                    .Select(i => new FreightInfoDto
                    {
                        ship_name = i.ShipName,
                        ship_desc = i.ShipDesc,
                        ship_id = i.ShipId.Value,
                        ticket = i.Ticket,
                        price = i.Price
                        
                    }).ToList();

                orderDetail.invoice_number = myOrder.InvoiceNumber.Value;
                orderDetail.po_number = myOrder.PoNumber;
                orderDetail.card_id = myOrder.CardId;
                orderDetail.freight = (double)myOrder.Freight * (1+ customerGst);
                orderDetail.order_id = myOrder.Id;
                orderDetail.total = (double)myOrder.Total;
                orderDetail.sub_total = (double)myOrder.OrderTotal;
                orderDetail.tax = (double)myOrder.Tax;
                orderDetail.payment_method = (myOrder.PaymentType);
                orderDetail.status = _isettings.getOrderStatus(Convert.ToInt32(myOrder.Status));// (myOrder.WebOrderStatus);
                orderDetail.paid = _iorder.getOrderPaymentStatus(order_id ?? 0);
                orderDetail.orderItems = await orderItem;
                orderDetail.shippingInfo = shippingInfo;
                orderDetail.shipping_method = myOrder.ShippingMethod;
                orderDetail.freightInfo = freightInfo;

                return Ok(orderDetail);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Exception while getting order detail with order_id {order_id}.");

                return StatusCode(500, "A problem happened while handling your request.");
            }
        }

        [HttpPatch("UpdateShippingStatus/{orderId}")]
        public async Task<IActionResult> SendEmailToCustomerByOrderId(int orderId, [FromBody] JsonPatchDocument<UpdateShippingStatusDto> patchDoc)
        {
			try
			{
                //step 1. update order status to 'shipping'
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var orderToUpdate = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
                if (orderToUpdate == null)
                    return NotFound("This order does not exist!");
                var itemToPatch = new UpdateShippingStatusDto
                {
                    Status = orderToUpdate.Status ?? 0
                };
                patchDoc.ApplyTo(itemToPatch, ModelState);
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                orderToUpdate.Status = itemToPatch.Status;
                await _context.SaveChangesAsync();

                //step 2. send email to notice customer
                var cardId = orderToUpdate.CardId;
                var customer = await _context.Card.FirstOrDefaultAsync(c => c.Id == cardId);
                if (customer == null)
                    return NotFound();
                var customerEmail = customer.Email;
                await _mail.sendEmail(customerEmail, "Shipping", "DoNotReply! <br><br> Dear customer: <br>Your order has been shipped.<br>Please check shipping detail from <a href='http://dollaritems.co.nz/ecom'> dollaritems.co.nz</a>.", null);
                
                return Ok("Shipping status updated!");
            }
			catch (Exception)
			{

				throw;
			}

        }

        [HttpGet("SendOrderToSupplier/{order_id}")]
        public async Task<IActionResult> SendOrderToSupplier(int order_id)
        {
            var hasOrder = await _context.Orders.AnyAsync(o => o.Id == order_id);
            if (!hasOrder)
                return BadRequest("order " + order_id + " not exists! ");

            var dealerId = _config["DealerId"];
            var apiUrl = _config["ApiUrl"];
            var siteName = _config["SiteName"];

            var sales_note = _context.Orders.Where(o => o.Id == order_id).FirstOrDefault().SalesNote;
            var shipping_method = _context.Orders.Where(o => o.Id == order_id).FirstOrDefault().ShippingMethod;
            var freight = _context.Orders.Where(o => o.Id == order_id).FirstOrDefault().Freight;
            var shippingInfo = _context.ShippingInfo.Where(o => o.orderId == order_id).FirstOrDefault();
            var shppingInfoDto =  new ShippingInfoDto();
            if (shippingInfo != null)
            {
                shppingInfoDto = new ShippingInfoDto()
                {
                    id = shippingInfo.id,
                    sender = shippingInfo.sender,
                    sender_phone = shippingInfo.sender_phone,
                    sender_address = shippingInfo.sender_address,
                    sender_city = shippingInfo.sender_city,
                    sender_country = shippingInfo.sender_country,
                    orderId = shippingInfo.orderId,
                    note = shippingInfo.note,
                    receiver = shippingInfo.receiver,
                    receiver_address1 = shippingInfo.receiver_address1,
                    receiver_address2 = shippingInfo.receiver_address2,
                    receiver_address3 = shippingInfo.receiver_address3,
                    receiver_city = shippingInfo.receiver_city,
                    receiver_company = shippingInfo.receiver_company,
                    receiver_contact = shippingInfo.receiver_contact,
                    receiver_country = shippingInfo.receiver_country,
                    receiver_phone = shippingInfo.receiver_phone,
                    receiver_zip = shippingInfo.zip,
                    oversea = false
                };
            }
            else
                shppingInfoDto = null;

                var orderItems = _context.OrderItem.Where(oi => oi.Id == order_id)
                                .Select(i => new CartItemDto
                                {
                                    code = i.Code.ToString(),
                                    quantity = i.Quantity.ToString(),
                                    barcode = i.Barcode,
                                    name = i.ItemName,
                                    id = i.Id,
                                    note = i.Note,
                                    supplier_code = i.SupplierCode
                                }).ToList();

            var newCreateOrderByDealerId = new CreateOrderByDealerIdDto()
            { 
                freight = freight,
                sales_note = sales_note,
                shipping_method = shipping_method,
                ShippingInfo = shppingInfoDto,
                cartItems = orderItems
            };
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    //check if this order has been sent already!
                    var responseTask = client.GetAsync("/"+siteName+"/api/dealer/order/po/eCom_Managment_" + order_id);
                    responseTask.Wait();
                    var final = responseTask.Result;
                    if (final.IsSuccessStatusCode)
                    {
                        var readTask = final.Content.ReadAsAsync<bool>();
                        readTask.Wait();
                        var myfinal = readTask.Result;
                        if (myfinal)
                            return BadRequest("order exists already!");
                    }

                    var content = newCreateOrderByDealerId;
                    var postTask = client.PostAsJsonAsync<CreateOrderByDealerIdDto>("/"+siteName+"/api/dealer/order/createOrderByDealerId/" + dealerId + "/" + order_id, content);
                    postTask.Wait();

                    var reault = postTask.Result;
                    if (reault.IsSuccessStatusCode)
                    {
                        return Ok("order sent!");
                    }
                    else
                    {
                        return BadRequest("something wrong!");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> createOrder([FromBody] CartDto cart)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            //check if record in shopping cart match records in cart table
            var itemsInCartTable = _context.Cart.Where(c => c.CardId == cart.card_id).ToList();  //items in cart table
            var itemsInCartDto = cart.cartItems;
            var itemsInCartTableToObj = itemsInCartTable.ConvertAll(x => new
            {
                id = x.Id,
                card_id = x.CardId,
                code = x.Code,
                quantity = x.Quantity
            });

            var itemsInCartDtoToObj = itemsInCartDto.ConvertAll(x => new
            {
                id = x.id,
                card_id = x.card_id,
                code = x.code,
                quantity = x.quantity
            });

            if (itemsInCartTable.Count() == itemsInCartDto.Count() && itemsInCartTableToObj.All(itemsInCartDtoToObj.Contains))
            {

            }
            else
            {
                return BadRequest("Items pass to request do not match items in Cart table!");
            }

            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

            if (cart == null || cart.cartItems == null) //if no item in cart, return not found; 
            {
                logger.Debug("shopping cart is empty");
                return NotFound();
            }
            //bool hasCardid = await _context.Card.AnyAsync(c => c.Id == cart.card_id);

            if (!await _context.Card.AnyAsync(c => c.Id == cart.card_id))
            {
                logger.Debug("This user doesn't exists, id : "+cart.card_id+"");
                return NotFound("This account does not exist, card_id :" + cart.card_id + " !");
            }

            //foreach (var item in cart.cartItems)
            //{

            //    if (!await _context.CodeRelations.AnyAsync(c => c.Code == Convert.ToInt32(item.code)))
            //    {
            //        logger.Debug("This item doesn't sell any longer, item code : "+item.code+"");
            //        return NotFound("This item does not sell any longer, item code :" + item.code + " !");
            //    }
            //}
            var inoviceInfo = new object();
            var branch_id = 1;

 //         if (await _context.Branch.AnyAsync(b => b.Name.Trim() == "Online Shop"))
            {
                branch_id = _isettings.getOnlineShopId();  //_context.Branch.Where(b => b.Name.Trim() == "Online Shop").FirstOrDefault().Id;
                logger.Debug("Get online shop id: " + branch_id + "");
            }
            var customerGst = cart.customer_gst;
            var newOrder = new Orders();
            newOrder.CardId = cart.card_id;
            newOrder.PoNumber = cart.po_num;
            newOrder.Branch = branch_id;
            newOrder.Freight = Math.Round((decimal)(cart.freight / (1 + (decimal?)customerGst)), 4);
            newOrder.OrderTotal = (decimal)cart.sub_total;
            newOrder.ShippingMethod = (byte)cart.shipping_method;
            newOrder.CustomerGst = cart.customer_gst;
            newOrder.IsWebOrder = true;
            newOrder.WebOrderStatus = 1;
            newOrder.Status = 1;

            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    await _context.Orders.AddAsync(newOrder);
                    await _context.SaveChangesAsync();
                    var newOrderId = newOrder.Id;
//                  var customerGst = newOrder.CustomerGst;
                    var totalGstInc = Math.Round((decimal)cart.sub_total * (1 + (decimal)customerGst), 2);
                    await inputOrderItem(cart.cartItems, newOrderId, customerGst);
                    inoviceInfo = await CreateInvoiceAsync(cart, newOrderId);
                    await ClearShoppingCart(cart.card_id);
                    await inputShippingInfo(cart.shippingInfo, newOrderId);

                    await _context.SaveChangesAsync();
                    dbContextTransaction.Commit();
                    return Ok(inoviceInfo);
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    logger.Error(ex.ToString());
                    return BadRequest(ex.ToString());
                }
                finally
                {
                    NLog.LogManager.Shutdown();
                }

            }

            //try
            //{

            //    await _context.Orders.AddAsync(newOrder);
            //    await _context.SaveChangesAsync();
            //    logger.Debug("Input order item, order id: " + newOrder.Id + "");
            //}
            //catch(Exception ex)
            //{
            //    logger.Error(ex, "Stopped program because of exception on creating order.");
            //    throw;
            //}
            //var newOrderId = newOrder.Id;
            //var customerGst = newOrder.CustomerGst;
            //try
            //{
            //    logger.Debug("Input order item, order id: " + newOrderId + "");
            //    await inputOrderItem(cart.cartItems, newOrderId, customerGst);
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex, "Stopped program because of exception on inputing order items.");
            //    throw;
            //}

            //try
            //{

            //    logger.Debug("Create invoice, order id: " + newOrderId + "");
            //    inoviceInfo = await CreateInvoiceAsync(cart, newOrderId);

            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex, "Stopped program because of exception on creating invoice.");
            //    throw;
            //}


            //try
            //{
            //    logger.Debug("shipping and clear shopping cart.");
            //    await ClearShoppingCart(cart.card_id);
            //    await inputShippingInfo(cart.shippingInfo, newOrderId);
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex, "Stopped program because of exception happen on shipping");
            //    throw;
            //}
            //finally
            //{
            //    NLog.LogManager.Shutdown();
            //}


            //return Ok(inoviceInfo);
        }
        private async Task<IActionResult> CreateInvoiceAsync([FromBody] CartDto cart, int orderid )
        {
            if (cart == null || cart.cartItems == null) //if no item in cart, return not found; 
            {
                return NotFound();
            }

            if (!_context.Card.Any(c => c.Id == cart.card_id))
            {
                return NotFound("This account does not exist, card_id :" + cart.card_id + " !");
            }

            if (!_context.Orders.Any(o=> o.Id == orderid))
            {
                return NotFound("This order does not exist, card_id :" + cart.card_id + " !");
            }

            var customerGst = cart.customer_gst;
            var currentOrder = _context.Orders.Where(o => o.Id == orderid).FirstOrDefault();
            var branch = _context.Orders.Where(o => o.Id == orderid).FirstOrDefault().Branch;
            var shippingMethod = _context.Orders.Where(o => o.Id == orderid).FirstOrDefault().ShippingMethod;
            var freightTax = cart.freight - Math.Round((decimal)(cart.freight / (1 + (decimal?)customerGst)), 4);

            var newInvoice = new Invoice();
            newInvoice.Branch = branch;
            newInvoice.CardId = cart.card_id;
            newInvoice.Price = (decimal?)cart.sub_total;
            newInvoice.ShippingMethod = shippingMethod;
            newInvoice.Tax = (decimal?)cart.tax + freightTax;
            newInvoice.Freight = Math.Round((decimal)(cart.freight/(1+ (decimal?)customerGst)),4);
            newInvoice.Total = (decimal?)(cart.total);// + cart.freight);
            newInvoice.CommitDate = DateTime.Now;
            newInvoice.ShippingMethod = (byte)cart.shipping_method;
            _context.Add(newInvoice);
            _context.SaveChanges();

            var invoiceNumber = newInvoice.Id;

            currentOrder.InvoiceNumber = invoiceNumber;
            newInvoice.InvoiceNumber = invoiceNumber;
            _context.SaveChanges();

            IActionResult a = await inputSalesItem(cart.cartItems, invoiceNumber, customerGst);

            return Ok(new { orderid, invoiceNumber, newInvoice.Total });
        }
        private async Task<IActionResult> inputOrderItem(List<CartItemDto> itemsInCart, int? orderId, double? customerGst)
        {

            if (itemsInCart == null || orderId == null)
            {
                return NotFound("Nothing in shopping cart!");
            }
            foreach (var item in itemsInCart)
            {
                var newOrderItem = new OrderItem();
                newOrderItem.Id = orderId.GetValueOrDefault();
                newOrderItem.Code = Convert.ToInt32(item.code);
                newOrderItem.ItemName = item.name;
                newOrderItem.Note = item.note;
                newOrderItem.Quantity = Convert.ToDouble(item.quantity);
                newOrderItem.SupplierCode = item.supplier_code ?? "";
                newOrderItem.Supplier = "";
                newOrderItem.CommitPrice = Convert.ToDecimal(item.sales_price) / Convert.ToDecimal(1 + customerGst ?? 0.15);

                newOrderItem.Cat = _iitem.getCat("cat", newOrderItem.Code); // _context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().Cat;
                newOrderItem.SCat = _iitem.getCat("scat", newOrderItem.Code);  //_context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().SCat;
                newOrderItem.SsCat = _iitem.getCat("sscat", newOrderItem.Code);  //_context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().SsCat;
                 await _context.AddAsync(newOrderItem);
            }
 //         await _context.SaveChangesAsync();
            return Ok();
        }
        private async Task<IActionResult> inputSalesItem(List<CartItemDto> itemsInCart, int? inoviceNumber, double? customerGst)
        {

            if (itemsInCart == null || inoviceNumber == null)
            {
                return NotFound("Cannot find inoivce!");
            }
            foreach (var item in itemsInCart)
            {
                var newSales = new Sales();
                newSales.InvoiceNumber = inoviceNumber.GetValueOrDefault();
                newSales.Code = Convert.ToInt32(item.code);
                newSales.Name = item.name;
                newSales.Note = item.note;
                newSales.Quantity = Convert.ToDouble(item.quantity);
                newSales.SupplierCode = item.supplier_code ?? "";
                newSales.Supplier = "";
                newSales.CommitPrice = Convert.ToDecimal(item.sales_price) / Convert.ToDecimal(1+ customerGst ?? 0.15);

                newSales.Cat = _iitem.getCat("cat", newSales.Code); //_context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().Cat;
                newSales.SCat = _iitem.getCat("scat", newSales.Code); //_context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().SCat;
                newSales.SsCat = _iitem.getCat("sscat", newSales.Code); //_context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().SsCat;
                await _context.AddAsync(newSales);
            }
//          await _context.SaveChangesAsync();
            return Ok();
        }

        private async Task<IActionResult>ClearShoppingCart(int card_id)
        {
            var recordAffected = _context.Cart.Where(c => c.CardId == card_id).ToList();
            if (recordAffected.Count == 0)
                return NotFound();
            if (recordAffected.Count > 0)
            {
                 _context.RemoveRange(recordAffected);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
        private async Task<IActionResult> inputShippingInfo(ShippingInfoDto shippingInfo, int? orderId)
        {
            if (shippingInfo == null || orderId == null)
            {
                return NotFound("No shipping address or order_id!");
            }
            shippingInfo.orderId = orderId.GetValueOrDefault();
            var newShipping = new ShippingInfo();
            newShipping.orderId = shippingInfo.orderId;
            newShipping.sender = shippingInfo.sender;
            newShipping.sender_phone = shippingInfo.sender_phone;
            newShipping.sender_address = shippingInfo.sender_address;
            newShipping.sender_city = shippingInfo.sender_city;
            newShipping.sender_country = shippingInfo.sender_country;

            newShipping.receiver = shippingInfo.receiver;
            newShipping.receiver_phone = shippingInfo.receiver_phone;
            newShipping.receiver_address1 = shippingInfo.receiver_address1;
            newShipping.receiver_address2 = shippingInfo.receiver_address2;
            newShipping.receiver_address3 = shippingInfo.receiver_address3;
            newShipping.receiver_city = shippingInfo.receiver_city;
            newShipping.receiver_country = shippingInfo.receiver_country;
            newShipping.receiver_company = shippingInfo.receiver_company;
            newShipping.receiver_contact = shippingInfo.receiver_contact;
            newShipping.note = shippingInfo.note;

            await _context.ShippingInfo.AddAsync(newShipping);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("del/{id}")]
        public async Task<IActionResult> deleteOrder(int? id)
        {
            if (id == null)
                return NotFound();
            var orderToDel = _context.Orders.Where(o => o.Id == id).FirstOrDefault();
            orderToDel.OrderDeleted = 1;
            orderToDel.WebOrderStatus = 2;
            _context.Update(orderToDel);
            //var orderItemToDel = _context.OrderItem.Where(oi => oi.Id == id).ToList();
            //var invoiceToDel = _context.Invoice.Where(i => i.InvoiceNumber == orderToDel.InvoiceNumber).FirstOrDefault();
            //var salesTodel = _context.Sales.Where(s => s.InvoiceNumber == orderToDel.InvoiceNumber).ToList();
            //_context.Orders.Remove(orderToDel);
            //_context.OrderItem.RemoveRange(orderItemToDel);
            //_context.Invoice.Remove(invoiceToDel);
            //_context.Sales.RemoveRange(salesTodel);
            await _context.SaveChangesAsync();
            return Ok();
        }


        [HttpPost("test")]
 //       [Consumes("application/x-www-form-urlencoded")]
        public IActionResult test(string url, string postData)
        {
            string result = "";
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "POST";

                req.ContentType = "application/x-www-form-urlencoded";

                req.Timeout = 800;//请求超时时间

                byte[] data = Encoding.UTF8.GetBytes(postData);

                req.ContentLength = data.Length;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(data, 0, data.Length);

                    reqStream.Close();
                }

                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                Stream stream = resp.GetResponseStream();

                //获取响应内容
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
            }
            catch
            {

            }
          return Ok(result);
        }

        [HttpPost("pay")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> updatePayment([FromForm] LatipayPaymentDto paymentInfo)
        {
            //         data = "merchant_reference=10110&order_id=2017232323345678&amount=12.50&currency=NZD&payment_method=alipay&pay_time=2017-07-07%2010%3A53%3A50&status=paid&signature=840151e0dc39496e22b410b83058b4ddd633b786936c505ae978fae029a1e0f1";
            if (paymentInfo == null)
                return BadRequest("model is null");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //string ObjInStr = "{ \r\n";
            //string[] newstr = data.Split("&");
            //foreach (string ns in newstr)
            //{
            //    var index = newstr.ToList().IndexOf(ns);
            //    if (index < newstr.Length - 1)
            //    {
            //        string[] key = ns.Split("=");
            //        ObjInStr += "\"";
            //        ObjInStr += key[0] + "\" : ";
            //        ObjInStr += "\"";
            //        ObjInStr += key[1] + "\", \r\n";
            //    }
            //    else
            //    {
            //        string[] key = ns.Split("=");
            //        ObjInStr += "\"";
            //        ObjInStr += key[0] + "\" : ";
            //        ObjInStr += "\"";
            //        ObjInStr += key[1] + "\" \r\n";
            //    }
            //}
            //ObjInStr += "}";

            //LatipayPaymentDto paymentInfo = JsonConvert.DeserializeObject<LatipayPaymentDto>(ObjInStr);

            var merchant_reference = paymentInfo.merchant_reference;

            var order = _context.Orders.Where(o => o.InvoiceNumber == Convert.ToInt32(merchant_reference)).FirstOrDefault();
            bool isTran_invoiced = await _context.TranInvoice.AnyAsync(i => i.InvoiceNumber == Convert.ToInt32(merchant_reference));
            if (order == null)
            {
                return BadRequest("Can not find this order!");
            }
            var paid = order.WebOrderStatus;
            if (paid > 1 && isTran_invoiced)
                return Ok("This order is paid!");
            //int latiinvoice_number = Convert.ToInt32(merchant_reference);

            var latipayment_method = paymentInfo.payment_method;
            var status = paymentInfo.status;
            var currenty = paymentInfo.currency;
            var amount = paymentInfo.amount;
            var signature = paymentInfo.signature;
            var order_id = paymentInfo.order_id;

            string myCheckingString =  merchant_reference + latipayment_method + status + currenty + amount;

            var apikey = Startup.Configuration["Latipay_apiKey"];
            byte[] secret = Encoding.UTF8.GetBytes(apikey);
            byte[] msg = Encoding.UTF8.GetBytes(myCheckingString);
            MyHMACSHA256 hmacsha256 = new MyHMACSHA256();
            byte[] SHA256HMACSignature = hmacsha256.HashHMAC(secret, msg);
            string mysignature = BitConverter.ToString(SHA256HMACSignature).Replace("-", "").ToLower();

           // return Ok(paymentInfo.signature + "////" + mysignature);

            if (signature != mysignature)
            {
                _logger.LogCritical($"error occur when update payment!");
                return BadRequest("error occur when update payment!");

            }

            if (paymentInfo == null)
                return NotFound();
            var connect = _context.Database.GetDbConnection();
            var connectstring = _context.Database.GetDbConnection().ConnectionString;
            connect.Open();
            System.Data.Common.DbCommand dbCommand = connect.CreateCommand();

            var cardid = _context.Invoice.Where(i => i.InvoiceNumber.ToString() == paymentInfo.merchant_reference).FirstOrDefault().CardId;
            int paymentmethod = paymentMethodCast(paymentInfo.payment_method);

            try
            {
                var note = dbCommand.CreateParameter();

                note.ParameterName = "@note";
                note.DbType = System.Data.DbType.String;
                note.Value = order_id;

                var shop_branch = dbCommand.CreateParameter();
                shop_branch.ParameterName = "@shop_branch";
                shop_branch.DbType = System.Data.DbType.Int32;
                shop_branch.Value = 1032;

                var Amount = dbCommand.CreateParameter();
                Amount.ParameterName = "@Amount";
                Amount.DbType = System.Data.DbType.String;
                Amount.Value = paymentInfo.amount;

                var nDest = dbCommand.CreateParameter();
                nDest.ParameterName = "@nDest";
                nDest.DbType = System.Data.DbType.Int32;
                nDest.Value = "1116";

                var staff_id = dbCommand.CreateParameter();
                staff_id.ParameterName = "@staff_id";
                staff_id.DbType = System.Data.DbType.Int32;
                staff_id.Value = cardid.ToString();

                var card_id = dbCommand.CreateParameter();
                card_id.ParameterName = "@card_id";
                card_id.DbType = System.Data.DbType.Int32;
                card_id.Value = cardid.ToString();

                var payment_method = dbCommand.CreateParameter();
                payment_method.ParameterName = "@payment_method";
                payment_method.DbType = System.Data.DbType.Int32;
                payment_method.Value = paymentmethod;

                var invoice_number = dbCommand.CreateParameter();
                invoice_number.ParameterName = "@invoice_number";
                invoice_number.DbType = System.Data.DbType.Int32;
                invoice_number.Value = Convert.ToInt32(merchant_reference);

                var amountList = dbCommand.CreateParameter();
                amountList.ParameterName = "@amountList";
                amountList.DbType = System.Data.DbType.String;
                amountList.Value = paymentInfo.amount;


                var return_tran_id = dbCommand.CreateParameter();
                return_tran_id.ParameterName = "@return_tran_id";
                return_tran_id.Direction = System.Data.ParameterDirection.Output;
                return_tran_id.DbType = System.Data.DbType.Int32;

                dbCommand.Parameters.Add(note);
                dbCommand.Parameters.Add(shop_branch);
                dbCommand.Parameters.Add(Amount);
                dbCommand.Parameters.Add(staff_id);
                dbCommand.Parameters.Add(card_id);
                dbCommand.Parameters.Add(payment_method);
                dbCommand.Parameters.Add(invoice_number);
                dbCommand.Parameters.Add(amountList);
                dbCommand.Parameters.Add(return_tran_id);
                dbCommand.CommandText = "eznz_payment";
                dbCommand.CommandType = System.Data.CommandType.StoredProcedure;

                var obj = await dbCommand.ExecuteNonQueryAsync();
                //       return Ok(return_tran_id.Value);

                order.WebOrderStatus = 4;
                _context.Update(order);
                await _context.SaveChangesAsync();
                return Ok("sent");
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
            finally
            {
                connect.Close();
                connect.Dispose();
            }
        }

        private string PaymentType(byte? pm)
        {
            var mytype = _context.Enum.Where(e => e.Class == "payment_method" && e.Id == pm).FirstOrDefault().Name;
            return mytype;

        }
        private string status(int status)
        {
            var mystatus = _context.Enum.Where(e => e.Class == "web_order_status" && e.Id == status).FirstOrDefault().Name;
            return mystatus;

        }
        private int paymentMethodCast(string payment_method)
        {
            if (payment_method == "wechat")
                return 14;
            else if (payment_method == "alipay")
                return 15;
            else if (payment_method == "onlineBank")
                return 16;
            else if (payment_method == "unionpay")
                return 17;
            return 1;
        }
    }

}