using System;
using System.Collections.Generic;

namespace eCommerce_API.Models
{
    public partial class FreightSettings
    {
        public int Id { get; set; }
        public bool? Active { get; set; }
        public string Region { get; set; }
        public decimal Freight { get; set; }
        public decimal FreeshippingActiveAmount { get; set; }

        public decimal FreightRangeStart1 { get; set; }
        public decimal FreightRangeStart2 { get; set; }
        public decimal FreightRangeStart3 { get; set; }
        public decimal FreightRangeEnd1 { get; set; }
        public decimal FreightRangeEnd2 { get; set; }
        public decimal FreightRangeEnd3 { get; set; }

    }
}
