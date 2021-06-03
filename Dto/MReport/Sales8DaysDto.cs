using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class Sales8DaysDto
    {
        public DateTime Date { get; set; }
        public string name { get; set; }
      
        public string cat{ get; set; }
        public string s_cat { get; set; }
        public string ss_cat { get; set; }
        public string code { get; set; }
        public double total { get; set; }
        public double totalQty { get; set; }
        public double markdown { get; set; }
        public double markdownQty { get; set; }
        public double special { get; set; }
        public double specialQty { get; set; }
        public double normal { get; set; }
        public double normalQty { get; set; }
        public string Branch { get; set; }



    }
    public class Sales8DaysFilterDto
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public bool showAllItem { get; set; }
        public string keyword { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
        public string supplierId { get; set; }
        public int? BranchId { get; set; }

    }

}
