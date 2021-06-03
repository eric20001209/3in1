using System;
using System.Collections.Generic;

namespace eCommerce_API.Models
{
    public partial class StoreSpecial
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public int BranchId { get; set; }
        public bool Enabled { get; set; }
        public decimal Price { get; set; }
        public DateTime PriceStartDate { get; set; }
        public DateTime PriceEndDate { get; set; }
        public decimal Cost { get; set; }
        public DateTime CostStartDate { get; set; }
        public DateTime CostEndDate { get; set; }
    }
}
