using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce_API_RST_Multi.Dto.Sync
{
    public class UploadItemDto
    {
        [Required]
        public int BranchId { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SupplierCode { get; set; }
        public string Brand { get; set; }
        public string Cat { get; set; }
        public string SCat { get; set; }
        public string SSCat { get; set; }

        //public string BrandNew { get; set; }
        //public string CatNew { get; set; }
        //public string SCatNew { get; set; }
        //public string SSCatNew { get; set; }

        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }
        public string Barcode { get; set; }
        public bool? AutoWeigh { get; set; }
        public bool? PriceBarcode { get; set; }
        public bool? IdCheck { get; set; }
    }
}
