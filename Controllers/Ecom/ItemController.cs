using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using eCommerce_API.Data;
using eCommerce_API.Dto;
using eCommerce_API.Services;

using eCommerce_API_RST.Dto;
using Microsoft.AspNetCore.Cors;
using eCommerce_API_RST.Services;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using NLog.LayoutRenderers;
using Microsoft.Extensions.Configuration;
using eCommerce_API.Models;

namespace eCommerce_API.Controllers
{
    //    [Authorize]
    [Route("api/item")]
    [ApiController]
//  [EnableCors("AllowMyOrigin")]
    public class ItemController : ControllerBase
    {
        private rst374_cloud12Context _context; // = new rst374_cloud12Context();
        private readonly ISetting _isettings;
        private readonly IItem _iitem;
        string siteName = Startup.Configuration["SiteName"];
        private readonly IConfiguration _config;
        public ItemController(rst374_cloud12Context context, ISetting isettings, IItem iitem, IConfiguration config)
        {
            _context = context;
            _isettings = isettings;
            _iitem = iitem;
            _config = config;
        }

        /****************item list*********************/
        [HttpGet()]
        public IActionResult itemList([FromQuery] string supplier, [FromQuery] string brand, [FromQuery] string cat, [FromQuery] string scat, [FromQuery] string sscat
            , [FromQuery] bool? hot, [FromQuery] bool? skip, [FromQuery] bool? clearance, [FromQuery] bool? commingsoon, [FromQuery] bool? newitem, [FromQuery] bool inactive
            , [FromQuery] bool? freedelivery, [FromQuery] bool? special
            , [FromQuery] string keyword, [FromQuery] Pagination pagination)
        {
            var myfilter = new itemFilterDto();
            myfilter.Supplier = supplier;
            myfilter.Brand = brand;
            myfilter.Cat = cat;
            myfilter.SCat = scat;
            myfilter.SsCat = sscat;
            myfilter.Hot = hot;
            myfilter.Skip = skip;
            myfilter.Clearance = clearance;
            myfilter.ComingSoon = commingsoon;
            myfilter.NewItem = newitem;
            myfilter.Inactive = inactive;
            myfilter.FreeDelivery = freedelivery;
            myfilter.Special = special;
            myfilter.KeyWord = keyword;
            var finalList = myList(myfilter, pagination);

            return Ok(finalList);
        }

        private ItemListDto myList([FromBody] itemFilterDto filter, [FromQuery] Pagination pagination)
        {

            _context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            ItemListDto final = new ItemListDto();
            var orderExpression = string.Format("{0} {1}", pagination.SortName, pagination.SortType);
            var onlineshopId = _isettings.getOnlineShopId();
            var count = (from c in _context.CodeRelations
                         where
                         //c.IsWebsiteItem == true && 
                         c.Skip == false && c.OnLineRetail == true
                         && (filter.Cat != null ? c.Cat == filter.Cat : true)
                         && (filter.SCat != null ? c.SCat == filter.SCat : true)
                         && (filter.SsCat != null ? c.SsCat == filter.SsCat : true)
                         && (filter.Hot != null ? c.Hot == filter.Hot : true)
                         && (filter.Skip != null ? c.Skip == filter.Skip : true)
                         && (filter.Clearance != null ? c.Clearance == filter.Clearance : true)
                         && (filter.ComingSoon != null ? Convert.ToBoolean(c.ComingSoon) == filter.ComingSoon : true)
                         && (filter.NewItem != null ? c.NewItem == filter.NewItem : true)
                         && (filter.FreeDelivery != null ? c.FreeDelivery == filter.FreeDelivery : true)
                         && (filter.KeyWord != null ? 
                         c.Name.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >=0
                         || (String.IsNullOrEmpty(c.NameCn) ? false : c.NameCn.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0)
                         || c.Code.ToString().Contains(filter.KeyWord)
                         || c.SupplierCode.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
                         || c.Brand.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
                         || c.Cat.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
                         || c.SCat.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
                         || c.SsCat.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
                         : true)
						 join sq in _context.StockQty on c.Code equals sq.Code into g
						 from sq in g
						 where sq.Qty > 0 && sq.BranchId == _isettings.getOnlineShopId()
						 //join cb in _context.CodeBranch on c.Code equals cb.Code into g
						 //from cb in g.DefaultIfEmpty()
						 //where cb.BranchId == _isettings.getOnlineShopId() && cb.Inactive == false
						 select new {
                         }).Count();

            //var list = _context.CodeBranch.Where(cb => cb.Inactive == false && cb.BranchId == _isettings.getOnlineShopId())
            //            .Select(cb => new { cb.Code, cb.Price1 })
            //            .Join(_context.CodeRelations
            //            .Where(cr => cr.IsWebsiteItem == true && cr.Skip == false
            //            && (filter.Cat != null ? cr.Cat == filter.Cat : true)
            //            && (filter.SCat != null ? cr.SCat == filter.SCat : true)
            //            && (filter.SsCat != null ? cr.SsCat == filter.SsCat : true)
            //            && (filter.Hot != null ? cr.Hot == filter.Hot : true)
            //            && (filter.Skip != null ? cr.Skip == filter.Skip : true)
            //            && (filter.Clearance != null ? cr.Clearance == filter.Clearance : true)
            //            && (filter.ComingSoon != null ? Convert.ToBoolean(cr.ComingSoon) == filter.ComingSoon : true)
            //            && (filter.NewItem != null ? cr.NewItem == filter.NewItem : true)
            //            && (filter.FreeDelivery != null ? cr.FreeDelivery == filter.FreeDelivery : true)
            //            && (filter.KeyWord != null ? cr.Name.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
            //            || (String.IsNullOrEmpty(cr.NameCn) ? false : cr.NameCn.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0)
            //             || cr.Code.ToString().Contains(filter.KeyWord)
            //             || cr.SupplierCode.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
            //             || cr.Brand.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
            //             || cr.Cat.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
            //             || cr.SCat.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
            //             || cr.SsCat.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
            //            : true))
            //            ,
            //            cb => cb.Code,
            //            c => c.Code,
            //            (cb, c) => new ItemDto
            //            {
            //                Code = c.Code,
            //                SupplierCode = c.SupplierCode,
            //                Name = c.Name,
            //                NameCn = c.NameCn,
            //                Cat = c.Cat,
            //                SCat = c.SCat,
            //                SsCat = c.SsCat,
            //                Hot = c.Hot,
            //                FreeDelivery = c.FreeDelivery,
            //                Weight = c.Weight,
            //                NewItem = c.NewItem,
            //                OuterPack = c.outer_pack,
            //                InnerPack = c.InnerPack,
            //                Moq = (c.Moq == null || c.Moq == 0) ? 1 : c.Moq ?? 1,
            //                Barcode = c.Barcode,
            //                Stock = _iitem.getItemStork(onlineshopId, c.Code),
            //                Price1 =
            //                                                            //        c.Price1, 
            //                                                            cb.Price1 ?? 0,
            //                //_iitem.getOnlineShopPrice(onlineshopId, c.Code),
            //                Barcodes = _iitem.getBarcodes(c.Code),
            //                StoreSpecial = _iitem.SpecialSetting(c.Code, _isettings.getOnlineShopId())
            //            });

          var   list = (from c in _context.CodeRelations
                        where
                        //c.IsWebsiteItem == true && 
                        c.Skip == false && c.OnLineRetail == true
                        && (filter.Cat != null ? c.Cat == filter.Cat : true)
                        && (filter.SCat != null ? c.SCat == filter.SCat : true)
                        && (filter.SsCat != null ? c.SsCat == filter.SsCat : true)
                        && (filter.Hot != null ? c.Hot == filter.Hot : true)
                                            && (filter.Skip != null ? c.Skip == filter.Skip : true)
                                             && (filter.Clearance != null ? c.Clearance == filter.Clearance : true)
                                              && (filter.ComingSoon != null ? Convert.ToBoolean(c.ComingSoon) == filter.ComingSoon : true)
                        && (filter.NewItem != null ? c.NewItem == filter.NewItem : true)
                                                         && (filter.FreeDelivery != null ? c.FreeDelivery == filter.FreeDelivery : true)
                                    && (filter.KeyWord != null ? c.Name.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
                        || (String.IsNullOrEmpty(c.NameCn) ? false : c.NameCn.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0)
                         || c.Code.ToString().Contains(filter.KeyWord)
                         || c.SupplierCode.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
                         || c.Brand.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
                         || c.Cat.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
                         || c.SCat.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
                         || c.SsCat.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
                        : true)
                        join sq in _context.StockQty on c.Code equals sq.Code into g
                        from sq in g
                        where sq.Qty > 0 && sq.BranchId == _isettings.getOnlineShopId()
                        //join cb in _context.CodeBranch on c.Code equals cb.Code
                        //into g
                        //from cb in g.DefaultIfEmpty()
                        //where cb.BranchId == _isettings.getOnlineShopId() && cb.Inactive == false
                        //orderby cb.Price1 descending
                        select new ItemDto
                        {
                            Code = c.Code,
                            SupplierCode = c.SupplierCode,
                            Site = siteName, // Common.ConvertoToMD5(siteName),
                            Name = Common.RemoveString(c.Name,"--"),//Common.ReplacePriceInDescription(c.Name, @"WAS\s|(\$)[-]?[0-9]+\.?[0-9]+"),
                            NameCn = c.NameCn,
                            Cat = c.Cat,
                            SCat = c.SCat,
                            SsCat = c.SsCat,
                            Hot = c.Hot,
                            FreeDelivery = c.FreeDelivery,
                            IsIdCheck = c.IsIdCheck,
                            Weight = c.Weight,
                            NewItem = c.NewItem,
                            OuterPack = c.outer_pack,
                            InnerPack = c.InnerPack,
                            Moq = (c.Moq == null || c.Moq == 0) ? 1 : c.Moq ?? 1,
                            Barcode = c.Barcode,
                            PicUrl = _config["Url"] + "/pi/" + c.Code + ".jpg",
                            Stock = _iitem.getItemStork(onlineshopId, c.Code),
                            Price1 = 
                                            c.Price1, 
                                            //cb.Price1 ?? 0,
                                            //_iitem.getOnlineShopPrice(_isettings.getOnlineShopId(), c.Code),
                                            // _iitem.getOnlineShopPrice(onlineshopId, c.Code),
                            Barcodes = _iitem.getBarcodes(c.Code),
                            StoreSpecial = _iitem.SpecialSetting(c.Code, _isettings.getOnlineShopId())
                        }
                        );

            var itemCount = count;
            var pageCount = (int)Math.Ceiling(itemCount / (double)pagination.PageSize);

            IQueryable<ItemDto> items =
                             //  null;
                                list
                               .OrderBy(orderExpression)
                               .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                               .Take(pagination.PageSize);

            final.Items = items;
            final.CurrentPage = pagination.PageNumber;
            final.PageSize = pagination.PageSize;
            final.PageCount = pageCount;
            final.ItemCount = itemCount;
 //         var result = list; // Mapper.Map<IEnumerable<ItemDto>>(myItemList).OrderBy(c => c.Code).ToList() ;
            return final;
        }



        [HttpGet("{code}")]
        public IActionResult item(int code)
        {
            var result = from c in _context.CodeRelations
                         where
                         c.IsWebsiteItem == true && c.Code == code && c.Skip == false && c.OnLineRetail == true
                         //join cb in _context.CodeBranch on c.Code equals cb.Code into g
                         //from cb in g.DefaultIfEmpty()
                         //where cb.BranchId == _isettings.getOnlineShopId() && cb.Inactive == false


                         //join ss in _context.StoreSpecial on c.Code equals ss.Code into sg
                         //from ss in sg.DefaultIfEmpty()
                         //where ss.BranchId == _isettings.getOnlineShopId()
                         //join pd in _context.ProductDetails on c.Code equals pd.Code into f
                         //from pd in f.DefaultIfEmpty()

                         select new ItemDto
                         {
                             Code = c.Code,
                             Supplier = c.Supplier,
                             Site = Common.ConvertoToMD5(siteName),
                             SupplierCode = c.SupplierCode,
                             SupplierPrice = c.SupplierPrice,
                             ExpireDate = c.ExpireDate,
                             RefCode = c.RefCode,
                             AverageCost = c.AverageCost,
                             Name = Common.RemoveString(c.Name, "--"),
                                //Common.ReplacePriceInDescription(c.Name, @"WAS\s|(\$)[-]?[0-9]+\.?[0-9]+"),
                             NameCn = c.NameCn,
                             Brand = c.Brand,
                             Cat = c.Cat,
                             SCat = c.SCat,
                             SsCat = c.SsCat,
                             Hot = c.Hot,
                             Skip = c.Skip,
                             Clearance = c.Clearance,
                             ComingSoon = c.ComingSoon,
                             FreeDelivery = c.FreeDelivery,
                             Weight = c.Weight,
                             NewItem = c.NewItem,
                             Inactive = c.Inactive,
                             Popular = c.Popular,
                             IsSpecial = c.IsSpecial,
                             IsMemberOnly = c.IsMemberOnly,
                             IsWebsiteItem = c.IsWebsiteItem,
                             IsIdCheck = c.IsIdCheck,
                             NoDiscount = c.NoDiscount,
                             CommissionRate = c.CommissionRate,
                             OuterPack = c.outer_pack,
                             InnerPack = c.InnerPack,
                             Moq = (c.Moq == null || c.Moq == 0) ? 1 : c.Moq ?? 1,
                             PicUrl = _config["Url"] + "/pi/" + c.Code + ".jpg",
                             //SpecialPrice = ss.Price,
                             //SpecialPriceStartDate = ss.PriceStartDate,
                             //SpecialPriceEndDate = ss.PriceEndDate,
                             //SpecialCost = ss.Cost,
                             //SpecialCostStartDate = ss.CostStartDate,
                             //SpecialCostEndDate = ss.CostEndDate,
                             //LevelRate1 = c.LevelRate1,
                             //LevelRate2 = c.LevelRate2,
                             //LevelRate3 = c.LevelRate3,
                             //LevelRate4 = c.LevelRate4,
                             //LevelRate5 = c.LevelRate5,
                             //LevelRate6 = c.LevelRate6,
                             //LevelRate7 = c.LevelRate7,
                             //LevelRate8 = c.LevelRate8,
                             //LevelRate9 = c.LevelRate9,
                             LevelPrice0 = c.LevelPrice0,
                             LevelPrice1 = c.LevelPrice1,
                             LevelPrice2 = c.LevelPrice2,
                             LevelPrice3 = c.LevelPrice3,
                             LevelPrice4 = c.LevelPrice4,
                             LevelPrice5 = c.LevelPrice5,
                             LevelPrice6 = c.LevelPrice6,
                             LevelPrice7 = c.LevelPrice7,
                             LevelPrice8 = c.LevelPrice8,
                             LevelPrice9 = c.LevelPrice9,
                             Barcode = c.Barcode,
                             Price1 = c.Price1, //_iitem.getOnlineShopPrice(_isettings.getOnlineShopId(), c.Code) ,//cb.Price1 ?? 0,
                             Price2 = c.Price2,
                             Price3 = c.Price3,
                             TaxRate = c.TaxRate,
                             TaxCode = c.TaxCode,
                             CountryOfOrigin = c.CountryOfOrigin,
                             Detail = _iitem.getItemDetail(c.Code),
                             Barcodes = _iitem.getBarcodes(c.Code),
                             Stock = _iitem.getItemStork(_isettings.getOnlineShopId(), c.Code),
                             Details = _iitem.getItemDetails(c.Code),
                             StoreSpecial = _iitem.SpecialSetting(c.Code, _isettings.getOnlineShopId())
                         };

            return Ok(result);
        }

        [HttpGet("special")]
        public IActionResult specialItems([FromQuery] string supplier, [FromQuery] string brand, [FromQuery] string cat, [FromQuery] string scat, [FromQuery] string sscat
            , [FromQuery] bool? hot, [FromQuery] bool? skip, [FromQuery] bool? clearance, [FromQuery] bool? commingsoon, [FromQuery] bool? newitem, [FromQuery] bool inactive
            , [FromQuery] bool? freedelivery, [FromQuery] bool? special
            , [FromQuery] string keyword, [FromQuery] Pagination pagination)
        {
            var myfilter = new itemFilterDto();
            myfilter.Supplier = supplier;
            myfilter.Brand = brand;
            myfilter.Cat = cat;
            myfilter.SCat = scat;
            myfilter.SsCat = sscat;
            myfilter.Hot = hot;
            myfilter.Skip = skip;
            myfilter.Clearance = clearance;
            myfilter.ComingSoon = commingsoon;
            myfilter.NewItem = newitem;
            myfilter.Inactive = inactive;
            myfilter.FreeDelivery = freedelivery;
            myfilter.Special = special;
            myfilter.KeyWord = keyword;
            var finalList = specialList(myfilter, pagination);

            return Ok(finalList);
        }

        public SpecialItemListDto specialList([FromBody] itemFilterDto filter, [FromQuery] Pagination paging)
        {
            _context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

            var orderExpression = string.Format("{0} {1}", paging.SortName, paging.SortType);

            var count = (from c in _context.CodeRelations
                         join ss in _context.StoreSpecial on c.Code equals ss.Code
                         where ss.Enabled == true && c.Skip == false
                         && ss.PriceStartDate <= DateTime.Now
                         && ss.PriceEndDate >= DateTime.Now
                         && ss.BranchId == _isettings.getOnlineShopId()
                         //&& c.IsWebsiteItem == true 
                         && c.Skip == false && c.OnLineRetail == true
                         && (filter.Supplier != null ? c.Supplier == filter.Supplier : true)
                         && (filter.Brand != null ? c.Brand == filter.Brand : true)
                         && (filter.Cat != null ? c.Cat == filter.Cat : true)
                         && (filter.SCat != null ? c.SCat == filter.SCat : true)
                         && (filter.SsCat != null ? c.SsCat == filter.SsCat : true)
                         && (filter.Hot != null ? c.Hot == filter.Hot : true)
                         && (filter.Skip != null ? c.Skip == filter.Skip : true)
                         && (filter.Clearance != null ? c.Clearance == filter.Clearance : true)
                         && (filter.ComingSoon != null ? Convert.ToBoolean(c.ComingSoon) == filter.ComingSoon : true)
                         && (filter.NewItem != null ? c.NewItem == filter.NewItem : true)
                         && (filter.FreeDelivery != null ? c.FreeDelivery == filter.FreeDelivery : true)
                         && (filter.KeyWord != null ? c.Name.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
                          || (String.IsNullOrEmpty(c.NameCn) ? false : c.NameCn.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0)
                          || c.Code.ToString().Contains(filter.KeyWord)
                          || c.SupplierCode.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
                          || c.Brand.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
                          || c.Cat.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
                          || c.SCat.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
                          || c.SsCat.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
                         : true)
                         join sq in _context.StockQty on c.Code equals sq.Code into g
                         from sq in g
                         where sq.Qty > 0 && sq.BranchId == _isettings.getOnlineShopId()
                         select new
                         {

                         }).Count();

            var result =

            (from c in _context.CodeRelations
             join ss in _context.StoreSpecial on c.Code equals ss.Code
             where ss.Enabled == true && c.Skip == false && c.OnLineRetail == true
             && ss.PriceStartDate <= DateTime.Now
             && ss.PriceEndDate >= DateTime.Now
             && ss.BranchId == _isettings.getOnlineShopId()

             //&& c.IsWebsiteItem == true 
             && c.Skip == false && c.OnLineRetail == true
             && (filter.Cat != null ? c.Cat == filter.Cat : true)
             && (filter.SCat != null ? c.SCat == filter.SCat : true)
             && (filter.SsCat != null ? c.SsCat == filter.SsCat : true)
             && (filter.Hot != null ? c.Hot == filter.Hot : true)
             && (filter.Skip != null ? c.Skip == filter.Skip : true)
             && (filter.Clearance != null ? c.Clearance == filter.Clearance : true)
             && (filter.ComingSoon != null ? Convert.ToBoolean(c.ComingSoon) == filter.ComingSoon : true)
             && (filter.NewItem != null ? c.NewItem == filter.NewItem : true)
             && (filter.FreeDelivery != null ? c.FreeDelivery == filter.FreeDelivery : true)
             && (filter.KeyWord != null ? c.Name.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
              || (String.IsNullOrEmpty(c.NameCn) ? false : c.NameCn.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0)
              || c.Code.ToString().Contains(filter.KeyWord)
              || c.SupplierCode.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
              || c.Brand.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
              || c.Cat.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
              || c.SCat.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
              || c.SsCat.IndexOf(filter.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0
             : true)
             join sq in _context.StockQty on c.Code equals sq.Code into g
             from sq in g
             where sq.Qty > 0 && sq.BranchId == _isettings.getOnlineShopId()
             select new ItemDto
             {
                 Code = c.Code,
                 SupplierCode = c.SupplierCode,
                 Site = Common.ConvertoToMD5(siteName),
                 Name = Common.RemoveString(c.Name, "--"),
                        //Common.ReplacePriceInDescription(c.Name, @"WAS\s|(\$)[-]?[0-9]+\.?[0-9]+"),
                 NameCn = c.NameCn,
                 Cat = c.Cat,
                 SCat = c.SCat,
                 SsCat = c.SsCat,
                 Hot = c.Hot,
                 FreeDelivery = c.FreeDelivery,
                 Weight = c.Weight,
                 NewItem = c.NewItem,
                 IsIdCheck = c.IsIdCheck,
                 OuterPack = c.outer_pack,
                 InnerPack = c.InnerPack,
                 Moq = (c.Moq == null || c.Moq == 0) ? 1 : c.Moq ?? 1,
                 LevelPrice0 = c.LevelPrice0,
                 LevelPrice1 = c.LevelPrice1,
                 LevelPrice2 = c.LevelPrice2,
                 LevelPrice3 = c.LevelPrice3,
                 LevelPrice4 = c.LevelPrice4,
                 LevelPrice5 = c.LevelPrice5,
                 LevelPrice6 = c.LevelPrice6,
                 LevelPrice7 = c.LevelPrice7,
                 LevelPrice8 = c.LevelPrice8,
                 LevelPrice9 = c.LevelPrice9,
                 PicUrl = _config["Url"] + "/pi/" + c.Code + ".jpg",
                 Barcode = c.Barcode,
                 Stock = _iitem.getItemStork(_isettings.getOnlineShopId(), c.Code),
                 Price1 = c.Price1, //_iitem.getOnlineShopPrice(_isettings.getOnlineShopId(), c.Code),
                 Barcodes = _iitem.getBarcodes(c.Code),
                 StoreSpecial = _iitem.SpecialSetting(c.Code, _isettings.getOnlineShopId())
             })
            .OrderBy(orderExpression).ToList();

            var itemCount = count;
            var pageCount = (int)Math.Ceiling(itemCount / (double)paging.PageSize);
            var toSkip = (paging.PageNumber - 1) * paging.PageSize;
            var itemsOnSpecial =
                    result
                    .Skip(toSkip > 0 ? toSkip : 0)
                    .Take(paging.PageSize);

            var final = new SpecialItemListDto
            {
                Items = itemsOnSpecial,
                CurrentPage = paging.PageNumber,
                PageSize = paging.PageSize,
                PageCount = pageCount,
                ItemCount = itemCount
            };
            //         var result = list; // Mapper.Map<IEnumerable<ItemDto>>(myItemList).OrderBy(c => c.Code).ToList() ;
            return final;
            
        }

    }
}