using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Sync.Dtos;

namespace eCommerce_API_RST_Multi.Dto.Sync
{
    public class EditItemDto
    {
        [Required]
        public int BranchId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SupplierCode { get; set; }
        public string Brand { get; set; }
        public string Cat { get; set; }
        public string SCat { get; set; }
        public string SSCat { get; set; }

        public string BrandNew { get; set; }
        public string CatNew { get; set; }
        public string SCatNew { get; set; }
        public string SSCatNew { get; set; }

        public double? Qty { get; set; }
        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }
        public string Barcode { get; set; }
        public List<BarcodeDto> Barcodes { get; set; }
        public decimal? LevelPrice1 { get; set; }
        public decimal? LevelPrice2 { get; set; }
        public decimal? LevelPrice3 { get; set; }
        public decimal? LevelPrice4 { get; set; }
        public decimal? LevelPrice5 { get; set; }
        public decimal? LevelPrice6 { get; set; }
        public bool IsSpecial { get; set; }
        public decimal? SpecialPrice { get; set; }
        public DateTime SpecialStartTime { get; set; }
        public DateTime SpecialEndTime { get; set; }
        public int? PromoId { get; set; }
        public bool? AutoWeigh { get; set; }
        public bool? PriceBarcode { get; set; }
        public bool? IdCheck { get; set; }
    }
}
