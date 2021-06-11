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
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.Linq.Dynamic.Core;
using eCommerce_API;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace eCommerce_API_RST.Controllers
{
    [Route("{hostId}/ecom/api/dealer/order")]
    [ApiController]
    public class DealerOrderController : ControllerBase
    {
        rst374_cloud12Context _context;//= new rst374_cloud12Context();
        private readonly IItem _iitem;
        private readonly ISetting _isettings;
        private readonly IOrder _iorder;
        private ILogger<DealerOrderController> _logger;
        private string siteName = Startup.Configuration["SiteName"];
        public DealerOrderController(ILogger<DealerOrderController> logger
                                    , IItem iitem
                                    , rst374_cloud12Context context
                                    , ISetting isettings
                                    , IOrder iorder)
        {
            _logger = logger;
            _iitem = iitem;
            _context = context;
            _isettings = isettings;
            _iorder = iorder;
        }

        [HttpGet()]
        public IActionResult getOrders([FromQuery] int id, [FromQuery] bool? invoiced, [FromQuery] bool? paid, [FromQuery] int? status, [FromQuery] string customer,
    [FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] string keyword)
        {
            var filter = new OrderFilterDto();
            filter.id = id;
            filter.inoviced = invoiced;
            filter.paid = paid;
            filter.status = status;
            filter.customer = customer;
            if (start != DateTime.MinValue)
                filter.start = start;
            if (end != DateTime.MinValue)
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
                                 id = o.Id,
                                 card_id = o.CardId,
                                 branch = o.Branch,
                                 po_number = o.PoNumber,
                                 status = _isettings.getOrderStatus(Convert.ToInt32(o.Status)),//(myOrder.WebOrderStatus); o.Status.ToString(),
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
            //        _logger.LogCritical($"error occur when changing order status!", ex);

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
            var status = _context.Enum.Where(e => e.Id == status_id && e.Class == "web_order_status").FirstOrDefault().Name;
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
            try
            {
                _context.Update(orderToUpdate);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
            return NoContent();
        }

        [HttpGet("po/{po_number}")]
        public async Task<bool> orderExists(string po_number)
        {
            var orderExists = await _context.Orders.AnyAsync(o => o.PoNumber == po_number);
            return orderExists;
        }

        [HttpGet("po")]
        public async Task<IActionResult> onlineOrders()
        {
            var onlineOrders = await _context.Orders.Where(o=> o.IsWebOrder == true && o.PoNumber.Contains("eCom_Managment_"))
                                .Select(o=>o.PoNumber).ToListAsync();
            return Ok(onlineOrders);
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
                    .Include(b => b.invoiceFreight)
                    //.Join(_context.Invoice.Select(i => new { i.InvoiceNumber, i.Paid, i.PaymentType, i.Freight, i.Total, i.Tax, i.Price }),
                    //(b => b.InvoiceNumber),
                    //(i => i.InvoiceNumber),
                    //(b, i) => new { b.shippinginfo, b.invoiceFreight, b.Id, b.InvoiceNumber, b.PoNumber, b.CustomerGst, b.CardId, i.Freight, OrderTotal = i.Price, i.Total, i.Tax, b.WebOrderStatus, i.Paid, i.PaymentType })

                    .Join(_context.Enum.Where(e => e.Class == "payment_method"),
                    (b => (int)b.PaymentType),
                    (e => e.Id),
                    (b, e) => new
                    {
                        b.shippinginfo,
                        b.invoiceFreight,
                        b.Id,
                        b.Status,
                        b.ShippingMethod,
                        b.InvoiceNumber,
                        b.PoNumber,
                        b.CustomerGst,
                        b.CardId,
                        b.Freight,
                        b.RecordDate,
                        b.SalesNote,
 //                     b.WebOrderStatus,
                        b.Paid,
                        PaymentType = e.Name
                    }).FirstOrDefault();

                if (myOrder == null)
                {
                    _logger.LogInformation($"Order with id {order_id} was null.");
                    return NotFound();
                }

                var customerGst = myOrder.CustomerGst;
                decimal orderTotal = 0;
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
                        PriceGstInc = Math.Round(oi.CommitPrice * Convert.ToDecimal(1 + customerGst), 2),
                        Cat = oi.Cat,
                        Note = oi.Note
                    }).ToListAsync();
                foreach (var oi in await orderItem)
                {
                    orderTotal += oi.CommitPrice * (decimal) oi.Quantity;
                }


                var Tax = Math.Round(((orderTotal + myOrder.Freight) * (decimal)customerGst), 2);
                var Total = Math.Round(orderTotal + myOrder.Freight + Tax, 2);

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

                orderDetail.invoice_number = myOrder.InvoiceNumber ?? 0;
                orderDetail.po_number = myOrder.PoNumber;
                orderDetail.card_id = myOrder.CardId;
                orderDetail.freight = (double)myOrder.Freight;
                orderDetail.order_id = myOrder.Id;
                orderDetail.total = (double)Total;
                orderDetail.sub_total = (double)orderTotal; // myOrder.OrderTotal;
                orderDetail.tax = (double)Tax;
                orderDetail.payment_method = (myOrder.PaymentType);
                orderDetail.status =  _isettings.getOrderStatus(Convert.ToInt32(myOrder.Status));//(myOrder.WebOrderStatus);
                orderDetail.paid = _iorder.getOrderPaymentStatus(order_id ?? 0);
                orderDetail.orderItems = await orderItem;
                orderDetail.shippingInfo = shippingInfo;
                orderDetail.shipping_method = myOrder.ShippingMethod;
                orderDetail.freightInfo = _isettings.getFreightInfo(orderDetail.invoice_number); //freightInfo;
                orderDetail.sales_note = myOrder.SalesNote;

                return Ok(orderDetail);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Exception while getting order detail with order_id {order_id}.");

                return StatusCode(500, "A problem happened while handling your request.");
            }
        }

        [HttpGet("invoice/{invoice_number}")]
        public async Task<IActionResult> invoiceDetail(int? invoice_number)
        {
            if (invoice_number == null)
            {
                _logger.LogInformation($"invoice {invoice_number} was null.");
                return NotFound();
            }

            if (!await _context.Invoice.AnyAsync(i =>i.InvoiceNumber== invoice_number))
            {
                _logger.LogInformation($"Invoice {invoice_number} wasn't found.");
                return NotFound();
            }
            else
            {
                _logger.LogInformation($"Order {invoice_number} was found.");
            }

            try
            {
                var orderDetail = new OrderDetailDto();

                var myOrder = _context.Orders.Where(o => o.InvoiceNumber == invoice_number)
                    .Include(b => b.shippinginfo)
                    .Include(b => b.invoiceFreight)
                    .Join(_context.Enum.Where(e => e.Class == "payment_method"),
                    (b => (int)b.PaymentType),
                    (e => e.Id),
                    (b, e) => new
                    {
                        b.shippinginfo,
                        b.invoiceFreight,
                        b.Id,
                        b.Status,
                        b.ShippingMethod,
                        b.InvoiceNumber,
                        b.PoNumber,
                        b.CustomerGst,
                        b.CardId,
                        b.Freight,
                        b.RecordDate,
                        b.SalesNote,
                        //                     b.WebOrderStatus,
                        b.Paid,
                        PaymentType = e.Name
                    })
                    .Join(_context.Invoice,
                    (b=>b.InvoiceNumber),
                    (i=>i.InvoiceNumber),
                    (b,i)=>new 
                    {
                        b.shippinginfo,
                        b.invoiceFreight,
                        b.Id,
                        b.Status,
                        b.ShippingMethod,
                        b.InvoiceNumber,
                        b.PoNumber,
                        b.CustomerGst,
                        b.CardId,
                        b.Freight,
                        b.RecordDate,
                        b.SalesNote,
                        b.Paid,
                        b.PaymentType,
                        i.CommitDate
                    })
                    .FirstOrDefault();

                if (myOrder == null)
                {
                    _logger.LogInformation($"Order {myOrder.Id} was null.");
                    return NotFound();
                }

                var customerGst = myOrder.CustomerGst;
                decimal orderTotal = 0;
                var orderItem = await _context.OrderItem.Where(o => o.Id == myOrder.Id)
                    .Select(oi => new OrderItemDto
                    {
                        Kid = oi.Kid,
                        Id = oi.Id,
                        Code = oi.Code,
                        SupplierCode = oi.SupplierCode,
                        Barcode = _iitem.getBarcode(oi.Code),
                        Quantity = oi.Quantity,
                        ItemName = oi.ItemName,
                        ItemNameCn = oi.ItemNameCn,
                        CommitPrice = oi.CommitPrice,
                        PriceGstInc = Math.Round(oi.CommitPrice * Convert.ToDecimal(1 + customerGst), 2),
                        Cat = oi.Cat,
                        Note = oi.Note
                    }).ToListAsync();
                foreach (var oi in orderItem)
                {
                    orderTotal += oi.CommitPrice * (decimal)oi.Quantity;
                }


                var Tax = Math.Round(((orderTotal + myOrder.Freight) * (decimal)customerGst), 2);
                var Total = Math.Round(orderTotal + myOrder.Freight + Tax, 2);

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

                orderDetail.invoice_number = myOrder.InvoiceNumber ?? 0;
                orderDetail.po_number = myOrder.PoNumber;
                orderDetail.card_id = myOrder.CardId;
                orderDetail.freight = (double)myOrder.Freight;
                orderDetail.order_id = myOrder.Id;
                orderDetail.total = (double)Total;
                orderDetail.sub_total = (double)orderTotal; // myOrder.OrderTotal;
                orderDetail.tax = (double)Tax;
                orderDetail.payment_method = (myOrder.PaymentType);
                orderDetail.status = _isettings.getOrderStatus(Convert.ToInt32(myOrder.Status));//(myOrder.WebOrderStatus);
                orderDetail.paid = _iorder.getOrderPaymentStatus(myOrder.Id);
                orderDetail.orderItems = orderItem;
                orderDetail.commit_date = myOrder.CommitDate;
                orderDetail.shippingInfo = shippingInfo;
                orderDetail.shipping_method = myOrder.ShippingMethod;
                orderDetail.freightInfo = _isettings.getFreightInfo(orderDetail.invoice_number); //freightInfo;
                orderDetail.sales_note = myOrder.SalesNote;

                return Ok(orderDetail);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Exception while getting order detail with invoice {invoice_number}.");

                return StatusCode(500, "A problem happened while handling your request.");
            }
        }

        [HttpPost("createOrderByDealerId/{dealerId}/{orderId}")]
        public async Task<IActionResult> CreateOrderByDealerIdDto(int dealerId, string orderId,[FromBody] CreateOrderByDealerIdDto createOrderByDealerId)
        {
            if (!ModelState.IsValid)
            {
                string errorString = "";
                foreach (var modelState in ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        errorString += error + "\r\n";
                    }
                }
                return BadRequest(errorString);
      
            }
            var dealerExists = await _context.Card.AnyAsync(c => c.Id == dealerId);
            if (!dealerExists)
                return NotFound("Dealer not exists!");
            var orderExists = await _context.Orders.AnyAsync(o => o.PoNumber == orderId);
            if (orderExists)
                return NotFound("Order exists!");
            // get dealer price level
            var priceLevel = _context.Card.Where(c => c.Id == dealerId).FirstOrDefault().PriceLevel;
            if (priceLevel == 0)
                priceLevel = 1;

            double totalWeight = 0;
            decimal totalAmount = 0;
            var itemsInCart = new List<CartItemDto>();

            foreach (var i in createOrderByDealerId.cartItems)
            {
                var item = new CartItemDto();
                item.sales_price = _iitem.getLevelPrice(int.Parse(i.code), priceLevel).ToString(); //get levelprice
                item.quantity = i.quantity;
                item.total = Math.Round(Convert.ToDouble(item.sales_price) * Convert.ToDouble(item.quantity), 2); //get total for levelprice
                item.barcode = i.barcode;
                item.name = i.name;
                item.id = i.id;
                item.code = i.code;
                item.inner_pack = i.inner_pack;
                item.outer_pack = i.outer_pack;
                item.moq = i.moq;
                item.note = i.note;
                item.supplier_code = i.supplier_code;
                item.weight = i.weight;
                item.total_points = i.total_points;

                itemsInCart.Add(item);
                totalWeight += Convert.ToDouble(item.weight);
                totalAmount += Convert.ToDecimal(item.total);
            }
        

            //create invoice
            var inoviceInfo = new object();
            var branch_id = 1;

            var newOrder = new Orders();
            newOrder.CardId = dealerId;
            newOrder.PoNumber = "eCom_Managment_" + orderId;
            newOrder.Branch = branch_id;
            newOrder.Freight = (decimal)createOrderByDealerId.freight;
            newOrder.OrderTotal = totalAmount;
            newOrder.ShippingMethod = (byte)createOrderByDealerId.shipping_method;
            newOrder.CustomerGst = createOrderByDealerId.customer_gst;
            newOrder.IsWebOrder = true;
            newOrder.WebOrderStatus = 1;
            newOrder.Status = 1;
            newOrder.Number = newOrder.Id;
            newOrder.SalesNote = createOrderByDealerId.sales_note;
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    await _context.Orders.AddAsync(newOrder);
                    await _context.SaveChangesAsync();
                    var newOrderId = newOrder.Id;
                    var customerGst = newOrder.CustomerGst;
                    var totalGstInc = Math.Round(totalAmount * (1 + (decimal)customerGst), 2);

                    //create order
                    await inputOrderItem(itemsInCart, newOrderId, customerGst);
                    //input shopping info
                    await inputShippingInfo(createOrderByDealerId.ShippingInfo, newOrderId);

                    await _context.SaveChangesAsync();
                    dbContextTransaction.Commit();

                    string invoiceNumber = null;
                    return Ok(new { newOrderId, invoiceNumber, totalGstInc });
                    //   return Ok();
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    return BadRequest(ex.ToString());
                }
                finally
                {
                    NLog.LogManager.Shutdown();
                }

            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> createOrder([FromBody] CartDto cart)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            //check if record in shopping cart match records in cart table
            var itemsInCartTable = _context.Cart.Where(c => c.CardId == cart.card_id)
                    .Join(_context.CodeRelations.Where(cr=>cr.IsWebsiteItem == true)
                    , c => c.Code
                    , cr => cr.Code.ToString()
                    , (c, cr) => new { c.Id, c.CardId, c.Code, c.Quantity })
                    .ToList();//items in cart table
            var itemsInCartDto = cart.cartItems;                                        //items in cartDto

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
            var items = itemsInCartTableToObj.Where(a => !itemsInCartDtoToObj.Exists(t => a.code.Contains(t.code))).ToList();
            if (items.Count() > 0)
            {
                var codeList = items.SelectMany(c=>c.code);

                return BadRequest(items);
            } 

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
                logger.Debug("This user doesn't exists, id : " + cart.card_id + "");
                return NotFound("This account does not exist, card_id :" + cart.card_id + " !");
            }

            foreach (var item in cart.cartItems)
            {

                if (!await _context.CodeRelations.AnyAsync(c => c.Code == Convert.ToInt32(item.code)))
                {
                    logger.Debug("This item doesn't sell any longer, item code : " + item.code + "");
                    return NotFound("This item does not sell any longer, item code :" + item.code + " !");
                }
            }
            var inoviceInfo = new object();
            var branch_id = 1;

            //if (await _context.Branch.AnyAsync(b => b.Name.Trim() == "Online Shop"))
            //{
            //    branch_id = _isettings.getOnlineShopId();
            //    logger.Debug("Get online shop id: " + branch_id + "");
            //}

 

            var newOrder = new Orders();
            newOrder.CardId = cart.card_id;
            newOrder.PoNumber = cart.po_num;
            newOrder.Branch = branch_id;
            newOrder.Freight = (decimal)cart.freight;
            newOrder.OrderTotal = (decimal)cart.sub_total;
            newOrder.ShippingMethod = (byte)cart.shipping_method;
            newOrder.CustomerGst = cart.customer_gst;
            newOrder.IsWebOrder = true;
            newOrder.WebOrderStatus = 1;
            newOrder.Status = 1;
            newOrder.Number = newOrder.Id;
            newOrder.SalesNote = cart.sales_note;
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                try
                {

                    await _context.Orders.AddAsync(newOrder);
                    await _context.SaveChangesAsync();
                    var newOrderId = newOrder.Id;
                    var customerGst = newOrder.CustomerGst;
                    var totalGstInc = Math.Round((decimal)cart.sub_total * (1 + (decimal)customerGst), 2);

                    await inputOrderItem(cart.cartItems, newOrderId, customerGst);

                    await inputShippingInfo(cart.shippingInfo, newOrderId);
                    await ClearShoppingCart(cart.card_id);

                    await _context.SaveChangesAsync();
                    dbContextTransaction.Commit();

                    string invoiceNumber = null;
                    return Ok(new { newOrderId, invoiceNumber, totalGstInc });
               //   return Ok();
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    logger.Debug(ex.ToString());
                    return BadRequest(ex.ToString());
                }
                finally
                {
                    NLog.LogManager.Shutdown();
                }

            }





            //var newOrderId = newOrder.Id;
            //var customerGst = newOrder.CustomerGst;

 //           var totalGstInc = Math.Round((decimal)cart.sub_total * (1 + (decimal)customerGst),2);
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

            //return Ok(new { newOrderId, invoiceNumber, totalGstInc });
        }
        private async Task<IActionResult> CreateInvoiceAsync([FromBody] CartDto cart, int orderid)
        {
            if (cart == null || cart.cartItems == null) //if no item in cart, return not found; 
            {
                return NotFound();
            }

            if (!_context.Card.Any(c => c.Id == cart.card_id))
            {
                return NotFound("This account does not exist, card_id :" + cart.card_id + " !");
            }

            if (!_context.Orders.Any(o => o.Id == orderid))
            {
                return NotFound("This order does not exist, card_id :" + cart.card_id + " !");
            }

            var currentOrder = _context.Orders.Where(o => o.Id == orderid).FirstOrDefault();
            var branch = _context.Orders.Where(o => o.Id == orderid).FirstOrDefault().Branch;
            var shippingMethod = _context.Orders.Where(o => o.Id == orderid).FirstOrDefault().ShippingMethod;

            var newInvoice = new Invoice();
            newInvoice.Branch = branch;
            newInvoice.CardId = cart.card_id;
            newInvoice.Price = (decimal?)cart.sub_total;
            newInvoice.ShippingMethod = shippingMethod;
            newInvoice.Tax = (decimal?)cart.tax;
            newInvoice.Freight = (decimal?)cart.freight;
            newInvoice.Total = (decimal?)(cart.total);// + cart.freight);
            newInvoice.CommitDate = DateTime.Today;
            newInvoice.ShippingMethod = (byte)cart.shipping_method;
            _context.Add(newInvoice);
            _context.SaveChanges();

            var invoiceNumber = newInvoice.Id;
            var customerGst = cart.customer_gst;
            currentOrder.InvoiceNumber = invoiceNumber;
            newInvoice.InvoiceNumber = invoiceNumber;
            _context.SaveChanges();

            IActionResult a = await inputSalesItem(cart.cartItems, invoiceNumber, customerGst);

            return Ok(new { orderid, invoiceNumber, newInvoice.Total });
        }
        private async Task<IActionResult> inputOrderItem(List<CartItemDto> itemsInCart, int? orderId, double? customerGst)
        {
            var currentOrder = _context.Orders.Where(o => o.Id == orderId).FirstOrDefault();

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
                newOrderItem.CommitPrice = Convert.ToDecimal(item.sales_price);// / Convert.ToDecimal(1 + customerGst ?? 0.15);

                newOrderItem.Cat = _iitem.getCat("cat", newOrderItem.Code); // _context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().Cat;
                newOrderItem.SCat = _iitem.getCat("scat", newOrderItem.Code);  //_context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().SCat;
                newOrderItem.SsCat = _iitem.getCat("sscat", newOrderItem.Code);  //_context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().SsCat;
                await _context.AddAsync(newOrderItem);
                await _context.SaveChangesAsync();
            }
            currentOrder.Number = orderId ?? 0;
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
                newSales.CommitPrice = Convert.ToDecimal(item.sales_price) / Convert.ToDecimal(1 + customerGst ?? 0.15);

                newSales.Cat = _iitem.getCat("cat", newSales.Code); //_context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().Cat;
                newSales.SCat = _iitem.getCat("scat", newSales.Code); //_context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().SCat;
                newSales.SsCat = _iitem.getCat("sscat", newSales.Code); //_context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().SsCat;
                await _context.AddAsync(newSales);
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        private async Task<IActionResult> ClearShoppingCart(int card_id)
        {
            var recordAffected = _context.Cart.Where(c => c.CardId == card_id).ToList();
            if (recordAffected.Count == 0)
                return NotFound();
            if (recordAffected.Count > 0)
            {
                foreach (var i in recordAffected)
                {
                    _context.Remove(i);
                }
     //         _context.RemoveRange(recordAffected);
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
            newShipping.zip = shippingInfo.receiver_zip;
            newShipping.note = shippingInfo.note;

            await _context.ShippingInfo.AddAsync(newShipping);
 //         await _context.SaveChangesAsync();
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

        [HttpGet("status/{site}/{po_number}")]
        public IActionResult getOrderStatus(string site, string po_number)
        {
            if (po_number == null  || site == null)
                return BadRequest();
            var siteName = Startup.Configuration["SiteName"];
            if (site != siteName)
                return NotFound();

            var orderStatus = new OrderStatusDto();
            var thisOrder = _context.Orders.Where(o => o.PoNumber == po_number).FirstOrDefault();
            if (thisOrder != null)
                orderStatus.Status = _isettings.getOrderStatus(Convert.ToInt32(thisOrder.Status));


            return Ok(orderStatus);
        }
    }
}