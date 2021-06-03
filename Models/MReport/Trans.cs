using System;
using System.Collections.Generic;

namespace FarroAPI.Entities
{
    public partial class Trans
    {
        public int Id { get; set; }
        public int? Source { get; set; }
        public int? Dest { get; set; }
        public decimal Amount { get; set; }
        public decimal? DestAmount { get; set; }
        public bool? Banked { get; set; }
        public int? TransBankId { get; set; }
        public int? TransDate { get; set; }
        public byte Branch { get; set; }
        public bool Reconcile { get; set; }
    }
}
