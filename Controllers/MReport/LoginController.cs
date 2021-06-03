using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FarroAPI.Entities;
using FarroAPI.Models;

namespace FarroAPI.Controllers
{
    [Route("{hostId}/api/login")]
    public class LoginController : Controller
    {
        private readonly farroContext _context;

        public LoginController(farroContext context)
        {
            this._context = context;
        }

        [HttpPost()]
        public IActionResult Login([FromBody]LoginDto login)
        {
            var user = _context.Card.Select(c=> new { c.Name,c.Email,c.Password,c.AccessLevel,c.OurBranch}).FirstOrDefault(c => c.Email == login.Email);

            if (user == null)
            {
                return NotFound();
            }

            var loginResult = new LoginDto()
            {
                Email = login.Email,
                IsMatched = login.Password == user.Password ? true : false,
                Name = login.Password == user.Password ? user.Name : null,
                AccessLevel = login.Password == user.Password ? user.AccessLevel : (int?)null,
                BranchId = login.Password == user.Password ? user.OurBranch : (int?)null
            };

            return Ok(loginResult);
        }
    }
}
