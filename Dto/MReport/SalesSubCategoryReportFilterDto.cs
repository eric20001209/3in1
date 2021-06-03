﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class SalesSubCategoryReportFilterDto : IFilter
    {
        public List<int> BranchIds { get; set; } = new List<int>();
        public List<string> Categories { get; set; } = new List<string>();
        public DateTime StartDateTime { get; set; } = new DateTime(1753, 1, 1);
        public DateTime EndDateTime { get; set; } = DateTime.MaxValue;
        public bool OnlineOrder { get; set; } = false;
    }
}
