using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class StockMoveDto
    {
        public string Branch { get; set; }
      
        public string Code { get; set; }
        public string Cat { get; set; }
        public string SCat { get; set; }
        public string SSCat { get; set; }
        public string Name { get; set; }
        public double? UnitCost { get; set; }
        public double? InPurchase { get; set; }
        public double? OutSales { get; set; }
        public double? Waste { get; set; }
        public double? Adjustment { get; set; }
        public double? TransIn { get; set; }
        public double? TransOut { get; set; }
        public double? Stock { get; set; }
        public bool Skip { get; set; }
        public bool SOR { get; set; }
        public bool AutoZeroStock { get; set; }
        public double? StockValue { get; set; }

    }
}
