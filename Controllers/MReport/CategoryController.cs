using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FarroAPI.Entities;
using FarroAPI.Models;

namespace FarroAPI.Controllers
{
    public class SalesCategoryController : Controller
    {
        private readonly farroContext _context;

        public SalesCategoryController(farroContext context)
        {
            this._context = context;
        }
        [HttpGet("{hostId}/mreport/api/Categories/Item")]
        public async Task<IActionResult> GetItemCategories()
        {
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var categoryList = await _context.Sales
                .Where(c => c.Code > 1020 || c.Code < 1001)
                .Select(c => c.Cat)
                .Distinct()
                .ToListAsync();

            var catgoryDtoList = new List<CategoryDto>();
            categoryList.ForEach(c => catgoryDtoList.Add(new CategoryDto { Name = c }));

            return Ok(catgoryDtoList);
        }

        [HttpGet("{hostId}/api/Categories/Sales")]
        public async Task<IActionResult> GetSalesCategories()
        {
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var categoryList = await _context.Sales
                .Select(c => c.Cat)
                .Distinct()
                .ToListAsync();

            var catgoryDtoList = new List<CategoryDto>();
            categoryList.ForEach(c => catgoryDtoList.Add(new CategoryDto { Name = c }));

            return Ok(catgoryDtoList);
        }

        [HttpGet("{hostId}/api/SubSubCategories/Item")]
        public async Task<IActionResult> GetItemSubSubCategories()
        {
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var categoryList = await _context.Sales
                .Select(c => new { c.Cat, c.SCat, c.SsCat })
                .Distinct()
                .ToListAsync();

            var resultList = new List<SubSubCategoryDto>();

            categoryList.ForEach(c => resultList.Add(new SubSubCategoryDto
            {
                    CategoryName = c.Cat,
                    SubCategoryName = c.SCat,
                    SubSubCategoryName = c.SsCat
                })
            );

            return Ok(resultList);
        }

        [HttpGet("{hostId}/api/SubSubCategories/Sales")]
        public async Task<IActionResult> GetSalesSubSubCategories()
        {
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var categoryList = await _context.Sales
                .Select(c => new { c.Cat, c.SCat, c.SsCat })
                .Distinct()
                .ToListAsync();

            var resultList = new List<SubSubCategoryDto>();

            categoryList.ForEach(c => resultList.Add(new SubSubCategoryDto
            {
                CategoryName = c.Cat,
                SubCategoryName = c.SCat,
                SubSubCategoryName = c.SsCat
            })
            );

            return Ok(resultList);
        }
    }
}
