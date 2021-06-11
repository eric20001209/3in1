using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eCommerce_API.Data;
using eCommerce_API.Dto;
using eCommerce_API.Models;
using eCommerce_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce_API_RST.Controllers
{
    [Route("{hostId}/ecom/api/rollback")]
    [ApiController]
    public class TestRollBackController : ControllerBase
    {
        private readonly IItem _iitem;
        rst374_cloud12Context _context;
        public TestRollBackController(IItem iitem, rst374_cloud12Context context)
        {
            _iitem = iitem;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> createOrder([FromBody] CartDto cart)
        {
            var newOrder = new Orders();
            newOrder.CardId = cart.card_id;
            newOrder.PoNumber = cart.po_num;
            newOrder.Branch = 1;
            newOrder.Freight = (decimal)cart.freight;
            newOrder.OrderTotal = (decimal)cart.sub_total;
            newOrder.ShippingMethod = (byte)cart.shipping_method;
            newOrder.CustomerGst = cart.customer_gst;
            newOrder.IsWebOrder = true;
            newOrder.WebOrderStatus = 1;
            newOrder.Status = 1;
            newOrder.Number = newOrder.Id;
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
                    await _context.SaveChangesAsync();
                    dbContextTransaction.Commit();
                    return Ok();
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    return BadRequest();
                }
            }
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
                newOrderItem.CommitPrice = Convert.ToDecimal(item.sales_price);

                newOrderItem.Cat = _iitem.getCat("cat", newOrderItem.Code); 
                newOrderItem.SCat = _iitem.getCat("scat", newOrderItem.Code); 
                newOrderItem.SsCat = _iitem.getCat("sscat", newOrderItem.Code);  
                await _context.AddAsync(newOrderItem);
            }
            currentOrder.Number = orderId ?? 0;
            return Ok();
        }
    }

}