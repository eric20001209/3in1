using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using eCommerce_API.Data;
using eCommerce_API.Dto;
using eCommerce_API.Models;
using eCommerce_API.Services;
using Microsoft.AspNetCore.JsonPatch;
using System.Text;

namespace eCommerce_API_RST.Controllers
{
    [Route("api/dealer/cart")]
    [ApiController]
    public class DealerCartController : ControllerBase
    {
        private rst374_cloud12Context _context;
        private FreightContext _contextf;
        private readonly ISetting _isettings;
        public DealerCartController(rst374_cloud12Context context, FreightContext contextf, ISetting settings)
        {
            _context = context;
            _contextf = contextf;
            _isettings = settings;
        }

        [HttpGet("{cardid}")]
        public IActionResult getCart(int? cardid, [FromQuery] string region, [FromQuery] string oversea, [FromQuery] int dfid)
        {
            var customer = _context.Card.Where(c => c.Id == cardid).FirstOrDefault();
            if (customer == null)
                return BadRequest();
            var customerGst = customer.GstRate;
            double tax = 0;
            decimal subtotal = 0;
            decimal total = 0;
            decimal freight = 0;
            double totalWeight = 0;
            int total_points = 0;
            var mycart = new CartDto();

            List<CartItemDto> myShoppingCart =
                (from c in _context.Cart.Where(c => cardid != null ? c.CardId == cardid : true && c.Code != null)
                 .Select(c => new { c.Id, c.CardId, c.Code, c.Name, c.Quantity, c.SalesPrice, c.SupplierPrice, c.SupplierCode, c.Barcode, c.Points, c.Note })
                 join cr in _context.CodeRelations.Where(cr => cr.IsWebsiteItem == true).Select(cr=>new { cr.Code,cr.Moq,cr.InnerPack, cr.outer_pack,cr.FreeDelivery,cr.Weight }
                ) on c.Code equals cr.Code.ToString()
                 select new CartItemDto
                 {
                     id = c.Id,
                     card_id = c.CardId,
                     code = c.Code,
                     name = c.Name,
                     quantity = c.Quantity,
                     sales_price = c.SalesPrice,
                     supplier_code = c.SupplierCode,
                     barcode = c.Barcode,
                     moq = (cr.Moq == null || cr.Moq == 0) ? 1 : cr.Moq ?? 1,
                     inner_pack = cr.InnerPack == 0 ? 1 : cr.InnerPack,
                     outer_pack = (cr.outer_pack == null || cr.outer_pack == 0) ? 1 : cr.outer_pack ?? 1,
                     free_delevery = cr.FreeDelivery,
                     weight = cr.Weight,
                     points = c.Points,
                     note = c.Note,
                     total = Math.Round(Convert.ToDouble(c.SalesPrice) * Convert.ToDouble(String.IsNullOrEmpty(c.Quantity) ? "0" : c.Quantity), 2),
                     total_points = (Convert.ToInt32(String.IsNullOrEmpty(c.Points) ? "0" : c.Points) * Convert.ToInt32(String.IsNullOrEmpty(c.Quantity) ? "0" : c.Quantity))
                 }).ToList();

            mycart.cartItems = myShoppingCart;
            foreach (var item in myShoppingCart)
            {
                total += (decimal)item.total;
                total_points += item.total_points;
                if (!item.free_delevery && item.weight != null)
                    totalWeight += item.weight.Value * Convert.ToDouble(item.quantity);
            }



            //var finalFreight = 0;
            //var regionShipping = _isettings.getRegionFreightSetting(int.Parse(dfid ?? "1"));
            //decimal? regionUnitPrice = 0;
            //decimal? regionFreeShippingActiveAmount = 0;
            //decimal rangeStart1 = 0;
            //decimal rangeStart2 = 0;
            //decimal rangeStart3 = 0;
            //decimal rangeEnd1 = 0;
            //decimal rangeEnd2 = 0;
            //decimal rangeEnd3 = 0;

            //if (regionShipping != null)
            //{
            //    regionUnitPrice = regionShipping.Freight;
            //    regionFreeShippingActiveAmount = regionShipping.FreeshippingActiveAmount;
            //    rangeStart1 = regionShipping.RangeStart1;
            //    rangeStart2 = regionShipping.RangeStart2;
            //    rangeStart3 = regionShipping.RangeStart3;
            //    rangeEnd1 = regionShipping.RangeEnd1;
            //    rangeEnd2 = regionShipping.RangeEnd2;
            //    rangeEnd3 = regionShipping.RangeEnd3;
            //}

            //decimal dfreeShippingActiveAmount = 100;
            //decimal unitFreight = 0;

            //if (dfid != null)
            //    unitFreight = regionUnitPrice ?? 0;
            //dfreeShippingActiveAmount = regionFreeShippingActiveAmount ?? 100;




            /*********** fixed freight start **************
            if (region == "0")
                mycart.freight = Math.Round(Convert.ToDouble(unitFreight) * totalWeight, 4);
            else
                mycart.freight = Math.Round(Convert.ToDouble(unitFreight), 4);
            /*********** fixed freight end **************/

            //if (sumUp >= Convert.ToDouble(dfreeShippingActiveAmount))
            //{
            //    mycart.freight = 0;
            //}
            //if (totalWeight < 1 && region == "0")
            //{
            //    mycart.freight = Convert.ToDouble(unitFreight);
            //}
            /************freeshipping settings end**********/

            subtotal = total;// Math.Round(total / (1 + customerGst), 4);
            mycart.freight = _isettings.getDealerFreight(subtotal, dfid);
            freight = mycart.freight ?? 0;
            tax = Math.Round((double)(total+ freight) * customerGst, 2); // Math.Round(total - subtotal, 4);
     
            mycart.sub_total = (double)subtotal;
            mycart.tax = Math.Round(tax, 2);
            mycart.total = Math.Round((double)total+ (double)freight + tax, 2);
            mycart.total_points = total_points;
            mycart.total_weight = totalWeight;
            mycart.customer_gst = customerGst;
            mycart.card_id = customer.Id;
            return Ok(mycart);
        }

        [HttpPost("add/{cardid}")]
        public async Task<IActionResult> addToCart(int cardid, [FromBody] AddItemToCartDto itemToCart)
        {
            if (itemToCart == null)
                return NotFound();
            if (_context.Cart.Any(c => c.Code == itemToCart.code
            && c.Name == itemToCart.name
            && c.SupplierCode == itemToCart.supplier_code
            && c.CardId == cardid
            && c.SalesPrice == itemToCart.sales_price.ToString()))
            {
                //Add new qty to this item
                var existingItem = _context.Cart.Where(c => c.Code == itemToCart.code && c.Name == itemToCart.name && c.SupplierCode == itemToCart.supplier_code && c.CardId == cardid && c.SalesPrice == itemToCart.sales_price.ToString()).FirstOrDefault();

                var dQuantity = Convert.ToDouble(existingItem.Quantity);
                if ((dQuantity + itemToCart.quantity) < 0)
                {
                    return BadRequest("qty < 0");
                }
                dQuantity += itemToCart.quantity;
                existingItem.Quantity = dQuantity.ToString();

                //if new qty == 0, remove this item from cart
                if (dQuantity == 0)
                {
                    //                   await deleteFromCart(cardid, existingItem.Id);
                    var itemToRemoveFromCart = new Cart();
                    itemToRemoveFromCart = _context.Cart.Where(c => c.Id == existingItem.Id && c.CardId == cardid).FirstOrDefault();

                    if (itemToRemoveFromCart == null)
                        return NotFound();

                    _context.Remove(itemToRemoveFromCart);
                }
                await _context.SaveChangesAsync(); //async
                return NoContent();
            }
            else
            {
                if (itemToCart.quantity <= 0)
                    return BadRequest("quantity <= 0");
                var newItem = new Cart();
                newItem.CardId = itemToCart.card_id;
                newItem.Code = itemToCart.code;
                newItem.Name = itemToCart.name;
                newItem.Barcode = itemToCart.barcode;
                newItem.SalesPrice = itemToCart.sales_price.ToString();
                newItem.Quantity = itemToCart.quantity.ToString();
                newItem.SupplierCode = itemToCart.supplier_code;
                newItem.Points = itemToCart.points.ToString();

                await _context.AddAsync(newItem);
                await _context.SaveChangesAsync();
                return Ok(newItem);
            }
        }
        [HttpDelete("del/{cardid}/{id}")]
        public async Task<IActionResult> deleteFromCart(int cardid, int id)
        {
            var itemToRemoveFromCart = new Cart();
            itemToRemoveFromCart = _context.Cart.Where(c => c.Id == id && c.CardId == cardid).FirstOrDefault();

            if (itemToRemoveFromCart == null)
                return NotFound();

            _context.Remove(itemToRemoveFromCart);
            await _context.SaveChangesAsync();
            return Ok(itemToRemoveFromCart);

        }

        [HttpPatch("update/{cardid}/{id}")]
        public async Task<IActionResult> updateCart(int cardid, int id, string currentRowVersion, [FromBody] JsonPatchDocument<UpdateCartDto> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();

            var itemToUpdate = _context.Cart.Where(c => c.CardId == cardid && c.Id == id).FirstOrDefault();
            if (itemToUpdate == null)
                return NotFound();

            string dbRowVersion = ByteArrayToString(itemToUpdate.RowVersion);

            if (dbRowVersion == currentRowVersion)
            {
                return BadRequest("Data has updated, please refresh page!");
            }

            var itemToPatch = new UpdateCartDto()
            {
                quantity = itemToUpdate.Quantity,
                note = itemToUpdate.Note
            };

            patchDoc.ApplyTo(itemToPatch, ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            itemToUpdate.Quantity = itemToPatch.quantity;
            itemToUpdate.Note = itemToPatch.note;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

    }
}