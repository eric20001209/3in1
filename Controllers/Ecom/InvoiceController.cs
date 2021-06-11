using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using eCommerce_API.Controllers;
using eCommerce_API.Data;
using eCommerce_API.Dto;
using eCommerce_API.Models;
using eCommerce_API.Services;
using eCommerce_API_RST.Services;
using eCommerce_API_RST.Services.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace eCommerce_API_RST.Controllers
{
    [Route("{hostId}/ecom/api/invoice")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        rst374_cloud12Context _context; //= new rst374_cloud12Context();
        private ILogger<OrderController> _logger;
        private readonly IOrder _iorder;
        private readonly ISetting _isetting;
        private readonly IConfiguration _config;
        private readonly iMailService _mail;
        private IConverter _converter;
        public InvoiceController(ILogger<OrderController> logger
                                    , rst374_cloud12Context context
                                    , FreightContext contextf
                                    ,ISetting isetting
                                    , IOrder iorder
                                    , IConfiguration config
                                    , iMailService mail
                                    , IConverter converter
                                    )
        {
            _logger = logger;
            _context = context;
            _iorder = iorder;
            _isetting = isetting;
            _config = config;
            _mail = mail;
            _converter = converter;
        }

        [HttpGet("pdf/{orderId}")]
        public IActionResult createPDF(int orderId)
        {
            PdfInvoiceTemplateGenerator pdfGenerator = new PdfInvoiceTemplateGenerator(_iorder, _config);
            try
            {
                var globalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 10 },
                    DocumentTitle = "PDF Report",
                    Out = _config["PdfPath"] + orderId + ".pdf"
                };
                var objectSettings = new ObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = pdfGenerator.GetHTMLString(orderId),
                    // WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "styles.css") },
                    WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(_config["PdfPath"], "Assets", "styles.css") },
                    HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                    FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "Visit   http://dollaritem.co.nz/ecom/terms-and-conditions   for full return terms and conditions." }
                };
                var pdf = new HtmlToPdfDocument()
                {
                    GlobalSettings = globalSettings,
                    Objects = { objectSettings }
                };
                _converter.Convert(pdf);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.ToString());
            }

            return Ok("Successfully created PDF document.");
        }


        [HttpGet("{orderId}")]
        public IActionResult invoiceDetail(int orderId)
        {
            return Ok(_iorder.getOrderDetail(orderId));
        }

        [HttpGet("freightInfo/{orderId}")]
        public IActionResult freightInfo(int orderId)
        {
            var manage = _config["Management"];
            var po = "";
            if (manage == "true")
            {
                po = "eCom_Managment_" + orderId;

                var freight = _isetting.getFreightInfoByPo(po);
                return Ok(freight);
            }
            else
            {
                int invoice_number = 0;
                var order = _iorder.getOrderDetail(orderId);
                if (order != null)
                    invoice_number = order.inovice_number ?? 0;
                var freight = _isetting.getFreightInfo(invoice_number);
                return Ok(freight);
            }
        }
    }
}