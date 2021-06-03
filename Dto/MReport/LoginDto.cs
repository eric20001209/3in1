using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class LoginDto
    {
        public string Email { get; set; } = null;
        public string Password { get; set; } = null;
        public bool IsMatched { get; set; } = false;
        public string Name { get; set; } = null;
        public int? AccessLevel { get; set; } = null;
        public int? BranchId { get; set; } = null;
    }
}
