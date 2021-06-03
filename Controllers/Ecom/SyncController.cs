using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using eCommerce_API.Data;
using eCommerce_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using eCommerce_API_RST.Dto;
using Microsoft.EntityFrameworkCore;
using eCommerce_API.Models;
using NLog;
using Microsoft.Extensions.Logging;
using NLog.Web;
using eCommerce_API.Dto;

namespace eCommerce_API_RST.Controllers
{
	[Route("api/sync1")]
	[ApiController]
	public class SyncController : ControllerBase
	{
		private readonly rst374_cloud12Context _context;
		private readonly IConfiguration _config;
		private readonly IItem _item;
		private ILogger<SyncController> _logger;
		public SyncController(rst374_cloud12Context context, IConfiguration config, IItem item, ILogger<SyncController> logger)
		{
			_context = context;
			_config = config;
			_item = item;
			_logger = logger;
		}

		[HttpGet("getItem/{branchId}")]
		public async Task<IActionResult> getSyncItems(int branchId)
		{
			if (branchId == null)
				return NotFound();
			//			var syncPath = Path.Combine(_config["Sync:IniPath"], "sync.ini");
			var updatedItems = await _context.UpdatedItem.Where(ui => ui.BranchId == branchId)
								.Join(_context.CodeRelations.Where(c => c.Skip == false)
								.Select(c => new
								{
									c.Id,
									c.Code,
									c.SupplierCode,
									c.Name,
									c.NameCn,
									c.Price1,
									c.AverageCost,
									c.Cat,
									c.SCat,
									c.SsCat,
									c.Brand,
									c.PromoId,
									c.HasScale,
									c.IsBarcodeprice,
									c.IsIdCheck,
									c.LevelPrice1,
									c.LevelPrice2,
									c.LevelPrice3,
									c.LevelPrice4,
									c.LevelPrice5,
									c.LevelPrice6
								}), ui => ui.ItemCode, c => c.Code, (ui, c) => new ItemForSyncDto
								{
									Id = c.Id,
									BranchId = ui.BranchId,
									TimeStamp = BitConverter.ToString(ui.TimeStamp) ,
									//ui.DateUpdated,
									Code = c.Code,
									SupplierCode = c.SupplierCode,
									Name = c.Name,
									NameCn = c.NameCn,
									Price1 = c.Price1,
									AverageCost = c.AverageCost,
									Cat = c.Cat,
									SCat = c.SCat,
									SsCat = c.SsCat,
									Brand = c.Brand,
									PromoId = c.PromoId,
									HasScale = c.HasScale,
									IsBarcodeprice = c.IsBarcodeprice,
									IsIdCheck = c.IsIdCheck,
									LevelPrice1 = c.LevelPrice1,
									LevelPrice2 = c.LevelPrice2,
									LevelPrice3 = c.LevelPrice3,
									LevelPrice4 = c.LevelPrice4,
									LevelPrice5 = c.LevelPrice5,
									LevelPrice6 = c.LevelPrice6,
									Barcodes = _item.getBarcodesWithQtyAndPrice(ui.ItemCode),
									BranchPrice = _item.getOnlineShopPrice(branchId, ui.ItemCode),
									Stock = _item.getItemStork(branchId, ui.ItemCode),
									StoreSpecial = _item.SpecialSetting(ui.ItemCode, branchId),
									ItemDetails = _item.getItemDetails(ui.ItemCode)
								}).ToListAsync();					
								
			return Ok(updatedItems);
		}

		[HttpPost("resetItem/{branchId}")]
		public async Task<IActionResult> resetSyncItems(int? branchId, [FromBody] IEnumerable<ResetItemSyncDto> resetItems)
		{
			var message = "Item shipped : ";
			var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
			if (branchId == null)
				return NotFound();
			var items = await _context.UpdatedItem.Where(ui => ui.BranchId == branchId).ToListAsync();
			if (items == null || resetItems == null)
			{
				return NotFound();
			}
			var itemsToReset = resetItems.Count();
			try
			{
				foreach (var i in resetItems)
				{
					var item = await _context.UpdatedItem.FirstOrDefaultAsync(ui => ui.ItemCode == i.Code && ui.BranchId == branchId  && BitConverter.ToString(ui.TimeStamp) == i.TimeStamp);
					if (item != null)
					{
						_context.UpdatedItem.Remove(item);
						itemsToReset = itemsToReset - 1;
					}
					else
					{
						message = message + i.Code + ", ";
					}
				}
				if (itemsToReset > 0)
				{
					message = itemsToReset + " item(s) skipped!" + "\r\n" + message;
				}
				else
				{
					message = "All Done!";
				}
			}
			catch (Exception ex)
			{
				logger.Info(ex.ToString());
			//	throw;
			}
			await _context.SaveChangesAsync();
			return Ok(message);
		}

		[HttpPost("invoice/{branchId}")]
		public async Task<IActionResult> uploadInvoices(int? branchId, [FromBody] IEnumerable<InvoiceForSyncDto> invoices)
		{
			var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
			var msgUploadedInvoices = " Invoices uploaded : ";
			var msgSkippedInvoices = " Invoices skipped : ";
			if (invoices == null || branchId == null)
				return NotFound("Invoices not found!");
			var orderList = new List<Orders>();
			try
			{
				foreach (var invoice in invoices)
				{
					var invoiceExists = await _context.Orders.AnyAsync(o => o.PoNumber == invoice.InvoiceNumber.ToString());
					if (invoiceExists)
					{
						msgSkippedInvoices += invoice.InvoiceNumber.ToString() + ", ";
						logger.Info("Invoice exists : " + invoice.InvoiceNumber.ToString());
						continue;
					}

					msgUploadedInvoices += invoice.InvoiceNumber.ToString() + ", ";

					var newOrder = new Orders();
					newOrder.CardId = invoice.CardId;
					newOrder.PoNumber = invoice.InvoiceNumber.ToString();
					newOrder.Branch = branchId ?? 1;
					newOrder.Freight = invoice.Freight;
					newOrder.OrderTotal = invoice.Price;
					newOrder.Sales = invoice.SalesId;
					newOrder.RecordDate = invoice.RecordDate;
					newOrder.DateShipped = invoice.DateShipped;
					newOrder.SalesNote = invoice.SalesNote;
					newOrder.ShippingMethod = invoice.ShippingMethod;
					newOrder.CustomerGst = invoice.CustomerGst;
					newOrder.Status = 1;

					using (var dbContextTransaction = _context.Database.BeginTransaction())
					{
						try
						{
							await _context.Orders.AddAsync(newOrder);
							await _context.SaveChangesAsync();
							var newOrderId = newOrder.Id;
							var totalGstInc = invoice.Total;
							await inputOrderItem(invoice.OrderItems, newOrderId, invoice.CustomerGst);
							await CreateInvoiceAsync(invoice, newOrderId);

		//					await ClearShoppingCart(cart.card_id);
		//					await inputShippingInfo(cart.shippingInfo, newOrderId);

							await _context.SaveChangesAsync();
							dbContextTransaction.Commit();

						}
						catch (Exception ex)
						{
							dbContextTransaction.Rollback();
							logger.Error(ex.ToString());
							return BadRequest(ex.ToString());
						}
						finally
						{
							NLog.LogManager.Shutdown();
						}

					}
				}
				return Ok(msgSkippedInvoices + "\r\n"  + msgUploadedInvoices);
			}
			catch (Exception ex)
			{

				throw;
			}
		}
		private async Task<IActionResult> CreateInvoiceAsync(InvoiceForSyncDto invoice, int? orderid)
		{
			if (invoice == null || orderid == null) 
			{
				return NotFound();
			}

			var customerGst = invoice.CustomerGst;
			var currentOrder = _context.Orders.Where(o => o.Id == orderid).FirstOrDefault();
			var branch = _context.Orders.Where(o => o.Id == orderid).FirstOrDefault().Branch;
			var shippingMethod = _context.Orders.Where(o => o.Id == orderid).FirstOrDefault().ShippingMethod;
			var freightTax = invoice.Freight - Math.Round((decimal)(invoice.Freight / (1 + (decimal?)customerGst)), 4);

			var newInvoice = new Invoice();
			newInvoice.Branch = branch;
			newInvoice.CardId = invoice.CardId;
			newInvoice.Price = invoice.Price;
			newInvoice.ShippingMethod = shippingMethod;
			newInvoice.Tax = invoice.Tax;
			newInvoice.Freight = invoice.Freight;
			newInvoice.Paid = invoice.Paid;
			newInvoice.Total = invoice.Total;
			newInvoice.CommitDate = invoice.CommitDate;
			newInvoice.ShippingMethod = invoice.ShippingMethod;
			await _context.AddAsync(newInvoice);
			await _context.SaveChangesAsync();

			var invoiceNumber = newInvoice.Id;

			currentOrder.InvoiceNumber = invoiceNumber;
			newInvoice.InvoiceNumber = invoiceNumber;
			_context.SaveChanges();

			IActionResult a = await inputSalesItem(invoice.OrderItems.ToList(), invoiceNumber, customerGst);

			return Ok(new { orderid, invoiceNumber, newInvoice.Total });
		}
		private async Task<IActionResult> inputOrderItem(IEnumerable<OrderItemDto> orderItems, int? orderId, double? customerGst)
		{

			if (orderItems == null || orderId == null)
			{
				return NotFound("Nothing in shopping cart!");
			}
			foreach (var item in orderItems)
			{
				var newOrderItem = new OrderItem();
				newOrderItem.Id = orderId.GetValueOrDefault();
				newOrderItem.Code = Convert.ToInt32(item.Code);
				newOrderItem.ItemName = item.ItemName;
				newOrderItem.Note = item.Note;
				newOrderItem.Quantity = Convert.ToDouble(item.Quantity);
				newOrderItem.SupplierCode = item.SupplierCode ?? "";
				newOrderItem.Supplier = "";
				newOrderItem.CommitPrice = item.CommitPrice;

				newOrderItem.Cat = item.Cat; // _context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().Cat;
				newOrderItem.SCat = item.SCat; //_context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().SCat;
				newOrderItem.SsCat = item.SsCat; //_context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().SsCat;
				await _context.AddAsync(newOrderItem);
			}
//          await _context.SaveChangesAsync();
			return Ok();
		}
		private async Task<IActionResult> inputSalesItem(IEnumerable<OrderItemDto> orderItems, int? inoviceNumber, double? customerGst)
		{

			if (orderItems == null || inoviceNumber == null)
			{
				return NotFound("Cannot find inoivce!");
			}
			foreach (var item in orderItems)
			{
				var newSales = new Sales();
				newSales.InvoiceNumber = inoviceNumber.GetValueOrDefault();
				newSales.Code = Convert.ToInt32(item.Code);
				newSales.Name = item.ItemName;
				newSales.Note = item.Note;
				newSales.Quantity = Convert.ToDouble(item.Quantity);
				newSales.SupplierCode = item.SupplierCode ?? "";
				newSales.Supplier = "";
				newSales.CommitPrice = Convert.ToDecimal(item.CommitPrice) ;

				newSales.Cat = _item.getCat("cat", newSales.Code); //_context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().Cat;
				newSales.SCat = _item.getCat("scat", newSales.Code); //_context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().SCat;
				newSales.SsCat = _item.getCat("sscat", newSales.Code); //_context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().SsCat;
				await _context.AddAsync(newSales);
			}
			//          await _context.SaveChangesAsync();
			return Ok();
		}

		[HttpPost("trans/{branchId}")]
		public async Task<IActionResult> uploadTrans(int? branchId, [FromBody] IEnumerable<TransForSyncDto> trans)
		{
			var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
			var msgUploadedTrans = " Trans uploaded : ";
			var msgSkippedTrans = " Trans skipped : ";

			if (trans == null || branchId == null)
				return NotFound("Cannot find transactions!");

			//open database
			var connect = _context.Database.GetDbConnection();
			var connectstring = _context.Database.GetDbConnection().ConnectionString;
			connect.Open();
			System.Data.Common.DbCommand dbCommand = connect.CreateCommand();

			try
			{
				foreach (var tran in trans)
				{

					var transExists = await _context.Orders.AnyAsync(o => o.Branch == branchId && o.PoNumber == tran.InvoiceNumber.ToString());
					
					// 1. if cannot find this invoice in cloud, skip this trans
					if (!transExists)
					{
						msgSkippedTrans += tran.InvoiceNumber.ToString() + " (not found), ";
						logger.Info("Cannot find invoices : " + tran.InvoiceNumber.ToString());
						continue;
					}
					var invoice = _context.Orders.Where(o => o.Branch == branchId && o.PoNumber == tran.InvoiceNumber.ToString()).FirstOrDefault().InvoiceNumber;
					var invoiceTotal = _context.Invoice.Where(i => i.InvoiceNumber == invoice).FirstOrDefault().Total;
					var invoicePaid = _context.Invoice.Where(i => i.InvoiceNumber == invoice).FirstOrDefault().AmountPaid;
					var balance = (invoiceTotal - invoicePaid) ?? 0;
					var AbsBalance = Math.Abs(balance);

					if (AbsBalance <= (decimal)0.05)
					{
						msgSkippedTrans += tran.InvoiceNumber.ToString() + " (pocessed), ";
						logger.Info("Processed invoices : " + tran.InvoiceNumber.ToString());
						continue;
					}

					// 2. Process storeprocedure

					var invoiceNumber = _context.Orders.Where(o => o.Branch == branchId && o.PoNumber == tran.InvoiceNumber.ToString()).FirstOrDefault().InvoiceNumber;
					foreach (var payment in tran.Payments)
					{
						//clear parameters
						dbCommand.Parameters.Clear();

						try
						{
							var note = dbCommand.CreateParameter();
							note.ParameterName = "@note";
							note.DbType = System.Data.DbType.String;
							note.Value = payment.Note;             //insert dps ref to tran_detail tables

							var Payment_Ref = dbCommand.CreateParameter();
							Payment_Ref.ParameterName = "@payment_ref";
							Payment_Ref.DbType = System.Data.DbType.String;
							Payment_Ref.Value = payment.PaymentRef;

							var shop_branch = dbCommand.CreateParameter();
							shop_branch.ParameterName = "@shop_branch";
							shop_branch.DbType = System.Data.DbType.Int32;
							shop_branch.Value = branchId;

							var Amount = dbCommand.CreateParameter();
							Amount.ParameterName = "@Amount";
							Amount.DbType = System.Data.DbType.String;
							Amount.Value = payment.AmountApplied;


							var nDest = dbCommand.CreateParameter();
							nDest.ParameterName = "@nDest";
							nDest.DbType = System.Data.DbType.Int32;
							nDest.Value = "1116";

							var staff_id = dbCommand.CreateParameter();
							staff_id.ParameterName = "@staff_id";
							staff_id.DbType = System.Data.DbType.Int32;
							staff_id.Value = tran.StaffId.ToString();

							var card_id = dbCommand.CreateParameter();
							card_id.ParameterName = "@card_id";
							card_id.DbType = System.Data.DbType.Int32;
							card_id.Value = tran.CardId.ToString();

							var payment_method = dbCommand.CreateParameter();
							payment_method.ParameterName = "@payment_method";
							payment_method.DbType = System.Data.DbType.Int32;
							payment_method.Value = payment.PaymentMethod;

							var invoice_number = dbCommand.CreateParameter();
							invoice_number.ParameterName = "@invoice_number";
							invoice_number.DbType = System.Data.DbType.Int32;
							invoice_number.Value = Convert.ToInt32(invoiceNumber);   //get invoice number from orders table

							var amountList = dbCommand.CreateParameter();
							amountList.ParameterName = "@amountList";
							amountList.DbType = System.Data.DbType.String;
							amountList.Value = payment.AmountApplied;


							var return_tran_id = dbCommand.CreateParameter();
							return_tran_id.ParameterName = "@return_tran_id";
							return_tran_id.Direction = System.Data.ParameterDirection.Output;
							return_tran_id.DbType = System.Data.DbType.Int32;

							//var return_exist_trans = dbCommand.CreateParameter();
							//return_exist_trans.ParameterName = "@return_exist_trans";
							//return_exist_trans.Direction = System.Data.ParameterDirection.Output;
							//return_exist_trans.DbType = System.Data.DbType.Boolean;

							dbCommand.Parameters.Add(note);
							dbCommand.Parameters.Add(Payment_Ref);
							dbCommand.Parameters.Add(shop_branch);
							dbCommand.Parameters.Add(Amount);
							dbCommand.Parameters.Add(staff_id);
							dbCommand.Parameters.Add(card_id);
							dbCommand.Parameters.Add(payment_method);
							dbCommand.Parameters.Add(invoice_number);
							dbCommand.Parameters.Add(amountList);
							dbCommand.Parameters.Add(return_tran_id);
							//dbCommand.Parameters.Add(return_exist_trans);
							dbCommand.CommandText = "eznz_payment";
							dbCommand.CommandType = System.Data.CommandType.StoredProcedure;
							var obj = await dbCommand.ExecuteNonQueryAsync();
						}
						catch (Exception ex)
						{

							_logger.LogError(ex.Message + "\r\n" + $"Insert transaction into Db unsuccessful, invoice number: {tran.InvoiceNumber}.");
							return BadRequest(ex);
						}
						finally
						{
							//							connect.Close();
							//							connect.Dispose();
						}

					}
					msgUploadedTrans += tran.InvoiceNumber + ", ";
				}
			}
			catch (Exception)
			{

				throw;
			}
			finally
			{
				connect.Close();
				connect.Dispose();
			}
			return Ok(msgSkippedTrans + "\r\n" + msgUploadedTrans);
		}
	}
}
