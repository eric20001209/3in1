using System;
using System.Collections.Generic;

namespace eCommerce_API.Models
{
    public partial class CardAddress
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public string Company { get; set; }
        public string Contact { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public string Suburb{ get; set; }
        public string City { get; set; }
        public string Region{ get; set; }
        public string Country { get; set; }
        public string Zip{ get; set; }
        public bool IsDefault { get; set; }
    }
}
