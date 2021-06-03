using System;
using System.Collections.Generic;

namespace FarroAPI.Entities
{
    public partial class Budget
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string Cat { get; set; }
        public string SCat { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }
}
