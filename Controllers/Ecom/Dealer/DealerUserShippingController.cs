using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using eCommerce_API.Dto;
using eCommerce_API.Data;
using eCommerce_API.Models;
using eCommerce_API.Services;
using Microsoft.AspNetCore.JsonPatch;

namespace eCommerce_API_RST.Controllers
{
    [Route("api/dealer/shipping")]
    [ApiController]
    public class DealerUserShippingController : ControllerBase
    {
        private readonly rst374_cloud12Context _context;
        ISetting _isettings;
        public DealerUserShippingController(rst374_cloud12Context context, ISetting isettings)
        {
            _context = context;
            _isettings = isettings;
        }

        [HttpGet("{userid}")]
        public IActionResult shippingAddressList(int userid)
        {
            var shiptoList = _context.CardAddress.Where(c => c.CardId == userid)
                .Select(c => new ShippingToDto
                {
                    id = c.Id,
                    name = c.Contact,
                    company = c.Company,
                    address1 = c.Address,
                    address2 = c.Suburb,
                    address3 = _isettings.getDealerRegion(Convert.ToInt32(c.Region)),
                    city = c.City,
                    country = c.Country,
                    phone = c.Phone,
                    zip = c.Zip,
                    contact = c.Contact,
                }).ToList();
            return Ok(shiptoList);
        }
        [HttpPost("{userid}")]
        public async Task<IActionResult> addShippingAddress(int userid, [FromBody] AddShippingDto newShipping)
        {
            var shippingToAdd = new CardAddress();
            shippingToAdd.CardId = userid;
            shippingToAdd.Company = newShipping.company;
            shippingToAdd.Address = newShipping.address1;
            shippingToAdd.Suburb = newShipping.address2;
            shippingToAdd.Region = newShipping.address3;
            shippingToAdd.City = newShipping.city;
            shippingToAdd.Country = newShipping.country;
            shippingToAdd.Phone = newShipping.phone;
            shippingToAdd.Contact = newShipping.contact;
            shippingToAdd.Zip = newShipping.zip;
 //           shippingToAdd.Note = newShipping.note;
 //           shippingToAdd.Email = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString();
            try
            {
                await _context.AddAsync(shippingToAdd);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
            return Ok();
        }
        [HttpPatch("patch/{id}")]
        public async Task<IActionResult> updateShippingAddress(int id, [FromBody] JsonPatchDocument<UpdateShippingDto> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();
            var shippingAddressToUpdate = _context.CardAddress.Where(c => c.Id == id).FirstOrDefault();
            if (shippingAddressToUpdate == null)
                return NotFound();

            var shippingToPatch = new UpdateShippingDto()
            {
                
                company = shippingAddressToUpdate.Company,
                address1 = shippingAddressToUpdate.Address,
                address2 = shippingAddressToUpdate.Suburb,
                address3 = shippingAddressToUpdate.Region,
                city = shippingAddressToUpdate.City,
                country = shippingAddressToUpdate.Country,
                phone = shippingAddressToUpdate.Phone,
                contact = shippingAddressToUpdate.Contact,
 //               note = shippingAddressToUpdate.Note,
                zip = shippingAddressToUpdate.Zip
            };

            patchDoc.ApplyTo(shippingToPatch, ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            shippingAddressToUpdate.Company = shippingToPatch.company;
            shippingAddressToUpdate.Address = shippingToPatch.address1;
            shippingAddressToUpdate.Suburb = shippingToPatch.address2;
            shippingAddressToUpdate.Region = shippingToPatch.address3;
            shippingAddressToUpdate.City = shippingToPatch.city;
            shippingAddressToUpdate.Country = shippingToPatch.country;
            shippingAddressToUpdate.Phone = shippingToPatch.phone;
            shippingAddressToUpdate.Contact = shippingToPatch.contact;
 //         shippingAddressToUpdate.Note = shippingToPatch.note;
            shippingAddressToUpdate.Zip = shippingToPatch.zip;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpDelete("del/{id}")]
        public async Task<IActionResult> deleteShippingAddress(int id)
        {
            var shippingToDelete = _context.CardAddress.Where(c => c.Id == id).FirstOrDefault();

            if (shippingToDelete == null)
                return NotFound();
            try
            {
                _context.CardAddress.Remove(shippingToDelete);
                await _context.SaveChangesAsync();

            }
            catch (Exception e)
            {
                throw e;
            }

            return NoContent();
        }
    }
}