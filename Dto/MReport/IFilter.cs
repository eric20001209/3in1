using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public interface IFilter
    {
        DateTime StartDateTime { get; set; }
        DateTime EndDateTime { get; set; }
    }

    public interface IBranchFilter : IFilter
    {
        int? BranchId { get; set; }
    }
}
