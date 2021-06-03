using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class StockTakeDto
    {
        public string Branch { get; set; }
        public string Code { get; set; }
        public string Cat { get; set; }
        public string SCat { get; set; }
        public string SSCat { get; set; }
        public string Name { get; set; }
        public double? StockOnHand { get; set; }
        public double? StockAdjustment { get; set; }
        public DateTime? StockTakeTime { get; set; }
        public bool IsInactive { get; set; }
        public bool IsService { get; set; }
        public bool IsHoldPurchase { get; set; }
    }
}
