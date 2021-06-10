using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System.Linq.Dynamic.Core;
using Sync.Data;
using Sync.Dtos;
using Sync.Models;
using Sync.Services;
using AutoMapper;
using System.Net;
using eCommerce_API_RST_Multi.Models.Sync;
using eCommerce_API_RST_Multi.Dto.Sync;
using ceTe.DynamicPDF.HtmlConverter;
using Microsoft.AspNetCore.JsonPatch;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Sync.Controllers
{
	//[AllowAnonymous]
	[Authorize(Policy = "HostIdAndAuthCodeMustMatch")]
	[Route("{hostId}/api/sync")]
	[ApiController]
	public class SyncController : ControllerBase
	{
		private readonly IConfiguration _config;
		private readonly Sync.Data.AppDbContext _context;
		private ILogger<SyncController> _logger;
		private readonly IItem _item;
		private readonly IButton _button;
		private readonly ICard _card;
//		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IMapper _mapper;
		public SyncController(IConfiguration config, 
			Data.AppDbContext context, 
			IItem item, IButton button, ICard card, ILogger<SyncController> logger
								, IMapper mapper
							//, IHttpContextAccessor httpContextAccessor
								)
		{
			_config = config;
			_context = context;
			_item = item;
			_button = button;
			_logger = logger;
			_card = card;
			_mapper = mapper;

		}
		[HttpGet("{auth}/status")]
		public async Task<IActionResult> getBranchUpdateStatus(string auth, [FromQuery] int branch)
		{
			//if (auth != _config["Auth"])
			//	return BadRequest("Wrong authentication code!");

			var apiSyncBranchList = await _context.Branch.Where(b => b.Activated == true && b.ApiSync == true)
									.Select(c => c.Id ).ToListAsync();
			if (!apiSyncBranchList.Contains(branch))
			{
				return BadRequest("Sorry, this branch is not a api-sync branch!");
			}

			var syncStatus = new SyncStatusDto()
			{
				AllProcessed = false
			};
			try
			{
				var allrecords = await _context.UpdatedBranch
					.Join(_context.Branch.Where(b => b.Activated == true && b.ApiSync == true),
							ub => ub.BranchId,
							b => b.Id,
							(ub, b) => new { ub.BranchId, ub.HasCreated, ub.HasProcessed, ub.hasUpdated, b.ApiSync }).ToListAsync();

				// if there is any updates, set to true
				syncStatus.HasUpdates = allrecords.FirstOrDefault(b => b.BranchId == branch).hasUpdated;
				syncStatus.CurrentProcessed = allrecords.FirstOrDefault(b => b.BranchId == branch).HasProcessed;

				if (allrecords.All(a => a.HasProcessed == false))
				{
					syncStatus.AllProcessed = true;
				}
				if (await _context.UpdatedBranch.AnyAsync(ub => ub.BranchId == branch))
				{
					var records = await _context.UpdatedBranch
							.Where(ub => ub.BranchId == branch).ToListAsync();
					foreach (var i in records)
					{
						i.HasCreated = false;
						i.HasProcessed = true;
						i.hasUpdated = false;
						_context.UpdatedBranch.Update(i);
					}
					await _context.SaveChangesAsync();


				}



				if (allrecords.TrueForAll(i => i.HasProcessed)  && allrecords.All(a=>a.ApiSync == true))
				{
					syncStatus.AllProcessed = true;
				}
			}
			catch (Exception)
			{
				throw;
			}
			return Ok(syncStatus);
		}

		[HttpPost("{auth}/status")]
		public async Task<IActionResult> setBranchUpdateStatus(string auth, [FromQuery] int branch)
		{
			
			return Ok();
		}

		[HttpGet("{auth}/getbuttons/{branchId}")]
		public async Task<IActionResult> getButtons(int? branchId)
		{
			var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
			MessageDto messageDto = new MessageDto();
			try
			{

				if (branchId == null)
				{
					messageDto.Code = "1";
					messageDto.Msg = $"failed! parameter branchId needed!";
					return NotFound(messageDto);
				}

				var branchExists = await _context.UpdatedButton.AnyAsync(u => u.BranchId == branchId);
				if (!branchExists)
				{
					messageDto.Code = "1";
					messageDto.Msg = $"failed! branch doesn't exist!";
					return NotFound(messageDto);
				}

				var buttons = await
							_context.Button.Where(b => b.Id < 7 || b.Id > 18).Select(
				b => new ButtonWithItemDto
				{
					Id = b.Id,
					Name = b.Name,
					NameEn = b.NameEn,
					IsIndivisual = b.IsIndivisual,
					Barcode = _button.getBarcodeForIndiButton(b.Id),
					ButtonItems = _button.getButtonItems(b.Id)
				})
				.ToListAsync();
				messageDto.Code = "0";
				messageDto.Msg = "success!";
				messageDto.Data = buttons;
				return Ok(messageDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}

		}

		[HttpPost("{auth}/resetbuttons/{branchId}")]
		public async Task<IActionResult> resetButton(int? branchId)
		{
			var messageDto = new MessageDto();
			var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
			try
			{
				if (branchId == null)
				{
					messageDto.Code = "1";
					messageDto.Msg = $"failed! parameter branchId needed!";
					return NotFound(messageDto);
				}
				var buttons = await _context.UpdatedButton.FirstOrDefaultAsync(ui => ui.BranchId == branchId);
				if (buttons == null)
				{
					messageDto.Code = "1";
					messageDto.Msg = $"failed! button doesn't exist!";
					return NotFound(messageDto);
				}

				else
				{
					_context.UpdatedButton.Remove(buttons);
					await _context.SaveChangesAsync();
				}
				messageDto.Code = "0";
				messageDto.Msg = "success!";
				return Ok(messageDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}

		}

		[HttpGet("{auth}/getcategory/{branchId}")]
		public async Task<IActionResult> getCategory(int? branchId)
		{
			var messageDto = new MessageDto();

			try
			{
				if (branchId == null)
				{
					messageDto.Code = "1";
					messageDto.Msg = $"failed! parameter branchId needed!";
					return NotFound(messageDto);
				}
				var branchExists = await _context.UpdatedCategory.AnyAsync(u => u.BranchId == branchId);
				if (!branchExists)
				{
					messageDto.Code = "1";
					messageDto.Msg = $"failed! branch doesn't exist!";
					return NotFound(messageDto);
				}


				var list = await _context
							.Category
				.Where(c => !String.IsNullOrEmpty(c.Cat))

				.Select(c => new
				{
					c.Seq,
					c.Cat,
					c.SCat,
					c.SSCat
				}).Distinct().ToListAsync();

				messageDto.Code = "0";
				messageDto.Msg = "success!";
				messageDto.Data = list;
				return Ok(messageDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}		

		}

		[HttpPost("{auth}/resetcategory/{branchId}")]
		public async Task<IActionResult> resetCategory(int branchId)
		{
			var messageDto = new MessageDto();
			var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
			try
			{
				var category = await _context.UpdatedCategory.FirstOrDefaultAsync(ui => ui.BranchId == branchId);
				if (category == null)
				{
					messageDto.Code = "1";
					messageDto.Msg = $"failed! category is null!";
					return NotFound(messageDto);
				}
				else
				{
					_context.UpdatedCategory.Remove(category);
					await _context.SaveChangesAsync();
					messageDto.Code = "0";
					messageDto.Msg = "success!";
					return Ok(messageDto);
				}

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Failed to reset category sync.");
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}



		}

		[HttpGet("{auth}/getcard/{branchId}",Name = "getcard")]
		public async Task<IActionResult> getCard(int branchId, [FromQuery] Pagination pagination)
		{
			var messageDto = new MessageDto();
			var orderExpression = string.Format("{0} {1}", pagination.SortName, pagination.SortType);
			try
			{
				var updatedCards = _context.UpdatedCard.Where(uc => uc.BranchId == branchId)
						.Join(_context.Cards
						.Select(c => new {
							Id = c.Id,
							Email = c.Email,
							Password = c.Password,
							Type = c.Type,
							Name = c.Name,
							ShortName = c.ShortName,
							TradingName = c.TradingName,
							Company = c.Company,
							Address1 = c.Address1,
							Address2 = c.Address2,
							Address3 = c.Address3,
							City = c.City,
							Country = c.Country,
							Phone = c.Phone,
							Fax = c.Fax,
							Contact = c.Contact,
							AccessLevel = c.AccessLevel,
							Barcode = c.Barcode,
							Discount = c.Discount,
							PriceLevel = c.DealerLevel,
							c.MDiscountRate,
							c.Points,
							c.Language,
							Mobile = c.Mobile
							
						}), uc => uc.CardId, c => c.Id, (uc, c) => new CardDto
						{
							Id = c.Id,
							Email = c.Email,
							Password = c.Password,
							Type = c.Type,
							Name = c.Name,
							ShortName = c.ShortName,
							TradingName = c.TradingName,
							Company = c.Company,
							Address1 = c.Address1,
							Address2 = c.Address2,
							Address3 = c.Address3,
							City = c.City,
							Country = c.Country,
							Phone = c.Phone,
							Fax = c.Fax,
							Contact = c.Contact,
							AccessLevel = c.AccessLevel,
							Barcode = c.Barcode,
							Discount = c.Discount,
							PriceLevel = c.PriceLevel,
							Points = c.Points,
							Language = c.Language,
							MDiscountRate = c.MDiscountRate,
							Mobile = c.Mobile,
							TimeStamp = uc.TimeStamp
						});
				var cardsToDelete = _context.UpdatedCard.Where(uc => uc.BranchId == branchId && uc.Delete)
									.Select(c => new CardDeletedDto
									{
										CardId = c.CardId,
										Email = c.Email,
										Barcode = c.Barcode,
										TimeStamp = c.TimeStamp
									}).ToList();

				if (updatedCards == null && cardsToDelete == null)
				{
					messageDto.Code = "1";
					messageDto.Msg = $"failed! card list is null!";
					return NotFound(messageDto);
				}
				//var final = _mapper.Map<List<CardDto>>(updatedCards);
				var cardCount = updatedCards.Count();
				var pageCount = (int)Math.Ceiling(cardCount / (double)pagination.PageSize);
				List<CardDto> items =
						 await updatedCards
						.OrderBy(orderExpression)
						.Skip((pagination.PageNumber - 1) * pagination.PageSize)
						.Take(pagination.PageSize).ToListAsync();
				CardListDto final = new CardListDto();

				final.Cards = items;
				final.CurrentPage = pagination.PageNumber;
				final.PageSize = pagination.PageSize;
				final.PageCount = pageCount;
				final.ItemCount = cardCount;
				final.CardsToDelete = cardsToDelete;
				messageDto.Code = "0";
				messageDto.Msg = $"success!";
				messageDto.Data = final;
				return Ok(messageDto);
			}
			catch (Exception ex)
			{

				_logger.LogError(ex.ToString());
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		[HttpPost("{auth}/resetcard/{branchId}")]
		public async Task<IActionResult> resetcards(int? branchId, [FromBody] IEnumerable<ResetCardSyncDto> resetCards)
		{
			var messageDto = new MessageDto();
			var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
			try
			{
				var cards = await _context.UpdatedCard.Where(ui => ui.BranchId == branchId).ToListAsync();
				if (cards == null)
				{
					messageDto.Code = "1";
					messageDto.Msg = $"failed! cards is null!";
					return NotFound(messageDto);
				}

				var cardsToReset = resetCards.Count();

				foreach (var i in resetCards)
				{
					var card = await _context.UpdatedCard.FirstOrDefaultAsync(uc => uc.CardId == i.Id && uc.TimeStamp == i.TimeStamp);
					if (card != null)
					{
					//	item.Updated = false;
						_context.UpdatedCard.Remove(card);
						cardsToReset = cardsToReset - 1;
						messageDto.Processed.Add(i.Id.ToString() + " "+ i.Email);
					}
					else
					{
						messageDto.Skipped.Add(i.Id.ToString() + " " + i.Email);

					}
				}
				await _context.SaveChangesAsync();
				messageDto.Code = "0";
				messageDto.Msg = "success!";
				return Ok(messageDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}

		}

		[HttpGet("{auth}/getItem/{branchId}")]
		public async Task<IActionResult> getSyncItems(int branchId, [FromQuery] Pagination pagination)
		{
			var messageDto = new MessageDto();
			try
			{
				var orderExpression = string.Format("{0} {1}", pagination.SortName, pagination.SortType);
				//			var syncPath = Path.Combine(_config["Sync:IniPath"], "sync.ini");
				var updatedItems = _context.UpdatedItem.Where(ui => ui.BranchId == branchId)
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
										TimeStamp = ui.TimeStamp, //BitConverter.ToString(ui.TimeStamp),
																  //ui.DateUpdated,
										TimeStampS = ui.TimeStampS,
										Code = ui.ItemCode,
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
									})
									;
				var deletedItems = _context.UpdatedItem.Where(uc => uc.BranchId == branchId && uc.Delete)
									.Select(c => new ItemDeletedDto
									{
										Code = c.ItemCode,
										TimeStamp = c.TimeStamp
									}).ToList();

				if (updatedItems == null && deletedItems == null)
				{
					messageDto.Code = "1";
					messageDto.Msg = $"failed! updatedItems is null!";
					return NotFound(messageDto);
				}
				var itemCount = updatedItems.Count();
				var pageCount = (int)Math.Ceiling(itemCount / (double)pagination.PageSize);
				List<ItemForSyncDto> items =
						 await updatedItems
						.OrderBy(orderExpression)
						.Skip((pagination.PageNumber - 1) * pagination.PageSize)
						.Take(pagination.PageSize).ToListAsync();
				ItemListDto final = new ItemListDto();

				final.Items = items;
				final.CurrentPage = pagination.PageNumber;
				final.PageSize = pagination.PageSize;
				final.PageCount = pageCount;
				final.ItemCount = itemCount;
				final.ItemsToDelete = deletedItems;
				messageDto.Code = "0";
				messageDto.Msg = "success!";
				messageDto.Data = final;
				return Ok(messageDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}

		}

		[HttpPost("{auth}/resetItem/{branchId}")]
		public async Task<IActionResult> resetSyncItems(int? branchId, [FromBody] IEnumerable<ResetItemSyncDto> resetItems)
		{
			var messageDto = new MessageDto();
			var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

			try
			{
				if (branchId == null)
					return NotFound();
				var items = await _context.UpdatedItem.Where(ui => ui.BranchId == branchId).ToListAsync();
				if (items == null || resetItems == null)
				{
					messageDto.Code = "1";
					messageDto.Msg = $"failed! the posted object is null!";
					return NotFound(messageDto);
				}
				var itemsToReset = resetItems.Count();
				foreach (var i in resetItems)
				{
					var item = await _context.UpdatedItem.FirstOrDefaultAsync(ui => ui.ItemCode == i.Code && ui.BranchId == branchId && ui.TimeStamp == i.TimeStamp);
					if (item != null)
					{
						_context.UpdatedItem.Remove(item);
						itemsToReset = itemsToReset - 1;
						messageDto.Processed.Add(i.Code.ToString());
					}
					else
					{
						messageDto.Skipped.Add(i.Code.ToString());
					}
				}
				await _context.SaveChangesAsync();
				messageDto.Code = "0";
				messageDto.Msg = "success!";
				return Ok(messageDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}

		}

		[HttpGet("{auth}/getPromotion/{branchId}")]
		public async Task<IActionResult> getPromotion(int? branchId)
		{
			var messageDto = new MessageWithTimeStampDto();
			try
			{
				var branchExists = await _context.UpdatedPromotion.FirstOrDefaultAsync(u => u.BranchId == branchId);
				if (branchExists == null)
				{
					messageDto.Code = "1";
					messageDto.Msg = $"failed! cannot find this branch!";
					return NotFound(messageDto);
				}
				var promoList = await _context.PromotionLists
						.Join(_context.PromotionBranches.Where(b=>b.BranchId == branchId),pl=>pl.PromoId, pb => pb.PromoId,(pl, pb)=> new
						{
						pl.PromoId, 
						pl.PromoDesc,
						pl.	PromoType,
						pl.	PromoStartDate,
						pl.PromoEndDate,
						pl.PromoActive,
						pl.PromoMemberOnly,
						pl.PromoDay1,
						pl.PromoDay2,
						pl.PromoDay3,
						pl.PromoDay4,
						pl.PromoDay5,
						pl.PromoDay6,
						pl.PromoDay7,
						pl.SpecialPrice,
						pl.DiscountPercentage,
						pl.FreeItemRequiredQty,
						pl.FreeItemRequiredItemCode,
						pl.FreeQtyRequiredQty,
						pl.FreeQtyRewardQty,
						pl.VolumnDiscountQty,
						pl.VolumnDiscountPriceTotal,
						pl.FreeItemRewardQty,
						pl.PromoCreateDate,
						pl.PromoCreateBy,
						PromoBranchId = pb.BranchId,
						pl.Limit
						})
						.Join(_context.Enums.Where(e=>e.Class == "promotion_type"), p => p.PromoType, e => e.Id, (p,e) => new {

							PromoId = p.PromoId,
							PromoDesc = p.PromoDesc,
							PromoType = p.PromoType,
							PromoStartDate = p.PromoStartDate,
							PromoEndDate = p.PromoEndDate,
							PromoActive = p.PromoActive,
							PromoMemberOnly = p.PromoMemberOnly,
							PromoDay1 = p.PromoDay1,
							PromoDay2 = p.PromoDay2,
							PromoDay3 = p.PromoDay3,
							PromoDay4 = p.PromoDay4,
							PromoDay5 = p.PromoDay5,
							PromoDay6 = p.PromoDay6,
							PromoDay7 = p.PromoDay7,
							SpecialPrice = p.SpecialPrice,
							DiscountPercentage = p.DiscountPercentage,
							FreeItemRequiredQty = p.FreeItemRequiredQty,
							FreeItemRequiredItemCode = p.FreeItemRequiredItemCode,
							FreeQtyRequiredQty = p.FreeQtyRequiredQty,
							FreeQtyRewardQty = p.FreeQtyRewardQty,
							VolumnDiscountQty = p.VolumnDiscountQty,
							VolumnDiscountPriceTotal = p.VolumnDiscountPriceTotal,
							p.FreeItemRewardQty,
							p.PromoCreateDate,
							p.PromoCreateBy,
							p.PromoBranchId,
							Limit = p.Limit,
							PromoTypeName  = e.Name
						})
						.Select(p => new PromotionDto
				{
					PromoId = p.PromoId,
					PromoDesc = p.PromoDesc,
					PromoType = p.PromoType,
					PromoTypeName = p.PromoTypeName,
					PromoStartDate = p.PromoStartDate,
					PromoEndDate = p.PromoEndDate,
					PromoActive = p.PromoActive,
					PromoMemberOnly = p.PromoMemberOnly,
					PromoDay1 = p.PromoDay1,
					PromoDay2 = p.PromoDay2,
					PromoDay3 = p.PromoDay3,
					PromoDay4 = p.PromoDay4,
					PromoDay5 = p.PromoDay5,
					PromoDay6 = p.PromoDay6,
					PromoDay7 = p.PromoDay7,
					SpecialPrice = p.SpecialPrice,
					DiscountPercentage = p.DiscountPercentage,
					FreeItemRequiredQty = p.FreeItemRequiredQty,
					FreeItemRequiredItemCode = p.FreeItemRequiredItemCode,
					FreeQtyRequiredQty = p.FreeQtyRequiredQty,
					FreeQtyRewardQty = p.FreeQtyRewardQty,
					VolumnDiscountQty = p.VolumnDiscountQty,
					VolumnDiscountPriceTotal = p.VolumnDiscountPriceTotal,
					PromoCreateDate = p.PromoCreateDate,
					PromoCreateBy = p.PromoCreateBy,
					PromoBranchId = p.PromoBranchId,
					AffactedItems = _item.getItemsByPromotionId(p.PromoId, p.PromoType),
					Limit = p.Limit


				}).ToListAsync();
				messageDto.Code = "0";
				messageDto.Msg = "success!";
				messageDto.TimeStamp = branchExists.TimeStamp;
				messageDto.Data = promoList;
				return Ok(messageDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}

		}

		[HttpPost("{auth}/resetpromo/{branchId}")]
		public async Task<IActionResult> resetPromo(int? branchId, [FromBody] ResetPromoSyncDto resetPromoSync)
		{
			var messageDto = new MessageDto();
			var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
			try
			{
				if (branchId == null)
				{
					messageDto.Code = "1";
					messageDto.Msg = $"failed! parameter branchId needed!";
					return NotFound(messageDto);
				}
				var buttons = await _context.UpdatedPromotion.FirstOrDefaultAsync(ui => ui.BranchId == branchId && ui.TimeStamp == resetPromoSync.TimeStamp);
				if (buttons == null)
				{
					messageDto.Code = "1";
					messageDto.Msg = $"failed! cannot find promotion to sync!";
					return NotFound(messageDto);
				}

				else
				{
					_context.UpdatedPromotion.Remove(buttons);
					await _context.SaveChangesAsync();
				}
				messageDto.Code = "0";
				messageDto.Msg = "success!";
				return Ok(messageDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}

		}

		[HttpPost("{auth}/invoice/{branchId}")]
		public async Task<IActionResult> uploadInvoices(int? branchId, [FromBody] List<InvoiceForSyncDto> invoices)
		{
			var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
			var messageDto = new MessageDto();

			if (invoices == null || branchId == null)
			{
				messageDto.Code = "1";
				messageDto.Msg = "failed! no invoice in post body!";
				return NotFound(messageDto);
			}

			var orderList = new List<Orders>();
			try
			{
				foreach (var invoice in invoices)
				{
					var invoiceExists = await _context.Orders.AnyAsync(o => o.PoNumber == invoice.InvoiceNumber.ToString() && o.Branch == branchId);
					if (invoiceExists)
					{
						messageDto.Skipped.Add(new InvoiceResetDto { Msg = "This invoice exists", InvoiceNumber = invoice.InvoiceNumber ?? 0 });
		//				((invoice.InvoiceNumber ?? 0).ToString());
						logger.Info("Invoice exists : " + invoice.InvoiceNumber.ToString());
						continue;
					}

				//	msgUploadedInvoices += invoice.InvoiceNumber.ToString() + ", ";

					var newOrder = new Orders();
					newOrder.CardId = _card.getIdFromBarcode(invoice.Barcode) ?? 0;// invoice.CardId;
					newOrder.PoNumber = invoice.InvoiceNumber.ToString();
					newOrder.Branch = branchId ?? 1;
					newOrder.Freight = invoice.Freight;
					newOrder.OrderTotal = invoice.Price;
					newOrder.Sales = _card.getIdFromBarcode(invoice.SalesBarcode);
					newOrder.RecordDate = invoice.RecordDate;
					newOrder.DateShipped = invoice.DateShipped;
					newOrder.SalesNote = invoice.SalesNote;
					newOrder.ShippingMethod = invoice.ShippingMethod;
					newOrder.CustomerGst = invoice.CustomerGst;
					newOrder.StationId = invoice.StationId;
					newOrder.Status = 1;

					using (var dbContextTransaction = _context.Database.BeginTransaction())
					{
						try
						{
							await _context.Orders.AddAsync(newOrder);
							await _context.SaveChangesAsync();

							var newOrderId = newOrder.Id;
							var totalGstInc = invoice.Total;
							////							await _context.Entry(newOrder).ReloadAsync();
							await inputOrderItem(invoice.OrderItems.ToList(), newOrderId, invoice.CustomerGst);
							await CreateInvoiceAsync(invoice, newOrderId);
							await updateStock(invoice.OrderItems.ToList(), branchId);
							await _context.SaveChangesAsync();

							dbContextTransaction.Commit();
							messageDto.Processed.Add(new InvoiceResetDto { Msg = "Invoice processed successfully! ", InvoiceNumber = invoice.InvoiceNumber ?? 0 });
							//		((invoice.InvoiceNumber ?? 0).ToString());
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
				messageDto.Code = "0";
				messageDto.Msg = "success!";
				return Ok(messageDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
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
			newInvoice.CardId = _card.getIdFromBarcode(invoice.Barcode) ?? 0;//invoice.CardId;
			newInvoice.Price = invoice.Price;
			newInvoice.ShippingMethod = shippingMethod;
			newInvoice.Tax = invoice.Tax;
			newInvoice.Freight = invoice.Freight;
			newInvoice.Paid = invoice.Paid;
			newInvoice.Total = invoice.Total;
			newInvoice.CommitDate = invoice.CommitDate;
			newInvoice.ShippingMethod = invoice.ShippingMethod;
			newInvoice.Sales = invoice.SalesName;
			newInvoice.StationId = invoice.StationId;
			newInvoice.Barcode = invoice.Barcode;
			newInvoice.Points = invoice.Points;
			await _context.AddAsync(newInvoice);
			await _context.SaveChangesAsync();

			var invoiceNumber = newInvoice.Id;
			int points = invoice.Points;// Convert.ToInt32(invoice.Total);
			currentOrder.InvoiceNumber = invoiceNumber;
			newInvoice.InvoiceNumber = invoiceNumber;
			await _context.SaveChangesAsync();
//			await _context.Entry(newInvoice).ReloadAsync();
			IActionResult a = await inputSalesItem(invoice.OrderItems.ToList(), invoiceNumber, customerGst);
			if (invoice.Paid)
			{
				//if paid, update points
				await updatePoints(points, invoice.Barcode);
			}
			//	return Ok(new { orderid, invoiceNumber, newInvoice.Total });
			return Ok();
		}
		private async Task<IActionResult> inputOrderItem(List<OrderItemDto> orderItems, int? orderId, double? customerGst)
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
			await _context.SaveChangesAsync();
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
				newSales.CommitPrice = Convert.ToDecimal(item.CommitPrice);
				newSales.SupplierPrice = item.SupplierPrice;
				newSales.Cat = item.Cat;    // _item.getCat("cat", newSales.Code); //_context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().Cat;
				newSales.SCat = item.SCat; // _item.getCat("scat", newSales.Code); //_context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().SCat;
				newSales.SsCat = item.SsCat; // _item.getCat("sscat", newSales.Code); //_context.CodeRelations.Where(c => c.Code == Convert.ToInt32(item.code)).FirstOrDefault().SsCat;
				await _context.AddAsync(newSales);
			}
			await _context.SaveChangesAsync();
			return Ok();
		}

		private async Task<IActionResult> updatePoints(int? points, string barcode)
		{
			if (points == 0 || points == null || barcode == null || barcode == "")
				return BadRequest();
			var card = await _context.Cards.FirstOrDefaultAsync(c => c.Barcode == barcode);
			if (card == null)
				return BadRequest();
			card.Points = card.Points + points ?? 0;
			_context.Cards.Update(card);
			await _context.SaveChangesAsync();
			return Ok();
		}
		private async Task<IActionResult> updateStock(IEnumerable<OrderItemDto> orderItems, int? branchId)
		{
			if (orderItems == null || branchId == null)
				return BadRequest();
			foreach (var item in orderItems)
			{
				var currentStock = await _context.StockQty.Where(sq => sq.Code == item.Code && sq.BranchId == branchId).FirstOrDefaultAsync();
				if (currentStock == null)
				{
					var newStock = new StockQty()
					{
						Qty = 0 - item.Quantity,
						BranchId = branchId ?? 1,
						Code = item.Code,
						WarningStock = 0
					};
					await _context.AddAsync(newStock);
				}
				else
				{
					var newStock = currentStock.Qty - item.Quantity;
					currentStock.Qty = newStock;
					_context.Update(currentStock);
				}

			}
			await _context.SaveChangesAsync();
			return Ok();
		}

		[HttpPost("{auth}/trans/{branchId}")]
		public async Task<IActionResult> uploadTrans(int? branchId, [FromBody] IEnumerable<TransForSyncDto> trans)
		{
			var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
			MessageDto messageDto = new MessageDto();


			if (trans == null || branchId == null)
			{
				messageDto.Code = "1";
				messageDto.Msg = "failed! no trans in post body!";
				return NotFound(messageDto);
			}

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
						messageDto.Skipped.Add(new TransResetDto { Msg = "Cannot find this invoice ",  InvoiceNumber = tran.InvoiceNumber});
						//(tran.InvoiceNumber.ToString() + " (not exists)");
						logger.Info("Cannot find invoices : " + tran.InvoiceNumber.ToString());
						continue;
					}
					//var invoice = _context.Orders.Where(o => o.Branch == branchId && o.PoNumber == tran.InvoiceNumber.ToString()).FirstOrDefault().InvoiceNumber;
					//var invoiceTotal = _context.Invoice.Where(i => i.InvoiceNumber == invoice).FirstOrDefault().Total;
					//var invoicePaid = _context.Invoice.Where(i => i.InvoiceNumber == invoice).FirstOrDefault().AmountPaid;
					//var balance = (invoiceTotal - invoicePaid) ?? 0;
					//var AbsBalance = Math.Abs(balance);

					//if (AbsBalance <= (decimal)0.05)
					//{
					//	messageDto.Skipped.Add(new TransResetDto { Msg = "This invoice has processed already ",  InvoiceNumber = tran.InvoiceNumber });
					//	//(tran.InvoiceNumber.ToString() + " (pocessed)");
					//	logger.Info("Processed invoices : " + tran.InvoiceNumber.ToString());
					//	continue;
					//}

					// 2. Process storeprocedure

					var invoiceNumber = _context.Orders.Where(o => o.Branch == branchId && o.PoNumber == tran.InvoiceNumber.ToString()).FirstOrDefault().InvoiceNumber;
					foreach (var payment in tran.Payments)
					{
						//check if trans already processed
						var processedTrans = await _context.TranDetails.AnyAsync(t => t.PaymentRef == payment.PaymentRef);
						if (processedTrans)
						{
							messageDto.Skipped.Add(new TransResetDto { Msg = "This trans has processed already ", TranId=payment.TranId, InvoiceNumber = tran.InvoiceNumber });
							logger.Info("Processed trans : " + payment.TranId.ToString());
							continue;
						}
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

							var station_id = dbCommand.CreateParameter();
							station_id.ParameterName = "@station_id";
							station_id.DbType = System.Data.DbType.Int32;
							station_id.Value = tran.StationId;

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
							dbCommand.Parameters.Add(station_id);
							dbCommand.Parameters.Add(payment_method);
							dbCommand.Parameters.Add(invoice_number);
							dbCommand.Parameters.Add(amountList);
							dbCommand.Parameters.Add(return_tran_id);
							//dbCommand.Parameters.Add(return_exist_trans);
							dbCommand.CommandText = "eznz_payment";
							dbCommand.CommandType = System.Data.CommandType.StoredProcedure;
							var obj = await dbCommand.ExecuteNonQueryAsync();
							messageDto.Processed.Add(new TransResetDto { Msg = "Trans successfully processed! ", TranId = payment.TranId, InvoiceNumber = tran.InvoiceNumber });
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
		
		//			(tran.InvoiceNumber.ToString());
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
			finally
			{
				connect.Close();
				connect.Dispose();
			}
			messageDto.Code = "0";
			messageDto.Msg = "success!";
			return Ok(messageDto);
		}

		[HttpPost("{auth}/punch/{branchId}")]
		public async Task<IActionResult> uploadPunchRecord(int? branchId, [FromBody] IEnumerable<PunchRecordDto> punches)
		{
			var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
			MessageDto messageDto = new MessageDto();
			if (punches == null)
			{
				messageDto.Code = "1";
				messageDto.Msg = "failed! no record in post body!";
				return NotFound(messageDto);
			}
			try
			{
				foreach (var p in punches)
				{
					var recordExists = await _context.WorkTimes.AnyAsync(w => w.Barcode == p.Barcode && w.RecordTime == p.RecordTime);
					if (recordExists)
					{
						messageDto.Skipped.Add(new PunchResetDto { Msg = "This record exist ",
																	Id = p.Id,
																	Barcode = p.Barcode,
																	Name = p.Name});
//						("This record exist, Id:" + p.Id + " Barcode:" + p.Barcode + " Name:" + p.Name);
						continue;
					}

					var workTime = new WorkTime()
					{ 
						CardId = _card.getIdFromBarcode(p.Barcode) ?? 0,
						Name = p.Name,
						Hours = p.Hours,
						IsCheckin = p.IsCheckin,
						RecordTime = p.RecordTime,
						Barcode = p.Barcode,
						BranchId = branchId ?? 1
					};
					if (workTime.CardId == 0)
					{
						messageDto.Skipped.Add(new PunchResetDto
						{
							Msg = "Cannot find this staff ",
							Id = p.Id,
							Barcode = p.Barcode,
							Name = p.Name
						});
						//			("Cannot find this staff, Id:" + p.Id + "Barcode:" + p.Barcode + " Name:" + p.Name);
						continue;
					}
					using (var dbContextTransaction = _context.Database.BeginTransaction())
					{
						try
						{
							await _context.WorkTimes.AddAsync(workTime);
							await _context.SaveChangesAsync();
							dbContextTransaction.Commit();
							messageDto.Processed.Add(new PunchResetDto
							{
								Msg = "Processed successfully ",
								Id = p.Id,
								Barcode = p.Barcode,
								Name = p.Name
							});
							//				("Processed successfully, Id:" + p.Id + " Barcode:" + p.Barcode + " Name:" + p.Name);
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
				messageDto.Code = "0";
				messageDto.Msg = "success!";
				return Ok(messageDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
				
			return Ok();
		}

		/****************/
		[HttpPost("{auth}/card/new")]
		public async Task<IActionResult> newCard([FromBody] CardCreateDto input)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			try
			{
				if (await emailExists(input.Email))
					return BadRequest("Email Exists!");
				if (await barcodeExists(input.Barcode))
					return BadRequest("Barcode Exists!");
				if (input.Barcode == null || input.Barcode == "")
				{
					input.Barcode = Guid.NewGuid().ToString();
				}
				var newCard = new Card
				{
					Email = input.Email,
					Name = input.Name,
					//					Password = input.Password,
					Phone = input.Phone,
					Barcode = input.Barcode,
					Type = input.Type
				};

				await _context.Cards.AddAsync(newCard);
				await _context.SaveChangesAsync();

				/*	insert into updated_card*/


				var cardDto = _mapper.Map<CardDto>(newCard);
				return Ok(cardDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}
		private async Task<bool> emailExists(string email)
		{
			var exists = await _context.Cards.AnyAsync(c => c.Email == email);
			if (exists)
				return true;
			else
				return false;
		}
		private async Task<bool> barcodeExists(string barcode)
		{
			var exists = await _context.Cards.AnyAsync(c => c.Barcode == barcode);
			if (exists)
				return true;
			else
				return false;
		}
		private async Task<bool> categoryExists(Catalog catalog)
		{
			var result = await _context.Category.AnyAsync(c => c == catalog);
			return result;
		}

		private async Task<bool> itemBarcodeExists(string barcode)
		{
			var exists = await _context.Barcode.AnyAsync(c => c.Barcode1 == barcode);
			if (exists)
				return true;
			else
				return false;
		}

		[HttpPost("{auth}/urltoPdf")]
		public async Task<IActionResult> pdfCreate([FromBody] PdfDto input)
		{
			ConversionOptions options = new ConversionOptions(PageSize.A4, PageOrientation.Portrait, 5.0f);
			var directory = _config["RootPath"] + "//invoice//" + input.InvoiceNumber + ".pdf";
			try
			{
				// Set Metadata for the PDF
				options.Author = "Myself";
				options.Title = "My Webpage";
				// Set Header and Footer text
				options.Header = "";
				// "<div style=\"text-align:center;display:inline-block;width:100%;font-size:12px;\">" +
				//"<span class=\"date\"></span></div>";
				options.Footer = "";
				//"<div style=\"text-align:center;display:inline-block;width:100%;font-size:12px;\">" +
				//                    "Page <span class=\"pageNumber\"></span> of <span class=\"totalPages\"></span></div>";
				// Convert with Options
				Converter.Convert(new Uri(input.Url), directory, options);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.ToString());
			}
			return Ok("Successfully created PDF document.");
		}
		private async Task<bool> pdfCreate(string invoice_number, string url)
		{
			try
			{
				ConversionOptions options = new ConversionOptions(PageSize.A4, PageOrientation.Portrait, 5.0f);
				var directory = _config["RootPath"] + "//invoice//" + invoice_number + ".pdf";
				options.Author = "Myself";
				options.Title = "My Webpage";
				options.Header = "";
				options.Footer = "";
				Converter.Convert(new Uri(url), directory, options);
				return true;
			}
			catch (Exception)
			{
				return false;
			}

		}

		[HttpPost("{auth}/createItem")]
		public async Task<IActionResult> createItem([FromBody] CreateItemDto createItemDto)
		{
			if (!ModelState.IsValid)
				return BadRequest();

			var cloudMaxCode = await _context.CodeRelations.MaxAsync(c => c.Code);
			int newCode = cloudMaxCode + 1;

			//if localMaxCode is greater than cloudMaxCode, then newCode equals to Local one 
			newCode = newCode < createItemDto.LocalMaxCode ? createItemDto.LocalMaxCode : newCode;

			var cat = (createItemDto.CatNew != null && createItemDto.CatNew != "") ? createItemDto.CatNew : createItemDto.Cat;
			var scat = (createItemDto.SCatNew != null && createItemDto.SCatNew != "") ? createItemDto.SCatNew : createItemDto.SCat;
			var sscat = (createItemDto.SSCatNew != null && createItemDto.SSCatNew != "") ? createItemDto.SSCatNew : createItemDto.SSCat;
			var brand = (createItemDto.BrandNew != null && createItemDto.BrandNew != "") ? createItemDto.BrandNew : createItemDto.Brand;

			using (var dbTransaction = await _context.Database.BeginTransactionAsync())
			{
				try
				{
					if (createItemDto.SupplierCode == null || createItemDto.SupplierCode == "")
					{
						createItemDto.SupplierCode = newCode.ToString();
					}
					/*	1. add to code_relations */
					CodeRelations codeRelations = new CodeRelations()
					{
						Id = createItemDto.Id.ToString(),
						Code = newCode,
						Name = createItemDto.Name,
						NameCn = createItemDto.Description,
						SupplierCode = createItemDto.SupplierCode,
						Brand = brand,
						Cat = cat,
						SCat = scat,
						SsCat = sscat,
						Price1 = createItemDto.Price ?? 0,
						ManualCostFrd = createItemDto.Cost ?? 0,
						SupplierPrice = createItemDto.Cost ?? 0,
						AverageCost = createItemDto.Cost ?? 0,
						Barcode = createItemDto.Barcode,
						HasScale = createItemDto.AutoWeigh ?? false,
						IsBarcodeprice = createItemDto.PriceBarcode ?? false,
						IsIdCheck = createItemDto.IdCheck ?? false
					};
					var addCodeRelationsResult = await _context.CodeRelations.AddAsync(codeRelations);

					/*	2. add to product	*/
					Product product = new Product
					{
						Code = newCode,
						Name = createItemDto.Name,
						NameCn = createItemDto.Description,
						SupplierCode = createItemDto.SupplierCode,
						Brand = brand,
						Cat = cat,
						SCat = scat,
						SSCat = sscat,
						Hot = false,
						Price = createItemDto.Price ?? 0,
						SupplierPrice = createItemDto.Cost ?? 0
					};
					var addProductResult = await _context.Products.AddAsync(product);

					/*	3. add to barcode	*/
					if (!await itemBarcodeExists(createItemDto.Barcode))
					{
						Barcode barcode = new Barcode
						{
							ItemCode = newCode,
							Barcode1 = createItemDto.Barcode
						};
						var addBarcodeResult = await _context.Barcode.AddAsync(barcode);
					}

					/*	4. add to stock_qty	*/
					StockQty stockQty = new StockQty
					{
						Code = newCode,
						Qty = 0,
						BranchId = createItemDto.BranchId,
						SupplierPrice = createItemDto.Cost ?? 0,
						AllocatedStock = 0,
						AverageCost = 0,
						QposPrice = 0,
						WarningStock = 0,
						LastStock = 0,
						SpStartDate = DateTime.Now,
						SpEndDate = DateTime.Now
					};
					var addStockQtyResult = await _context.StockQty.AddAsync(stockQty);

					/*	5. add to code_branch	*/
					CodeBranch codeBranch = new CodeBranch
					{
						Inactive = false,
						Code = newCode,
						BranchId = createItemDto.BranchId,
						Price1 = createItemDto.Price,
						Price2 = 0

					};
					var addCodeBranchResult = await _context.CodeBranch.AddAsync(codeBranch);

					/*	6. add to catalog	*/
					if (createItemDto.BrandNew != null && createItemDto.BrandNew != "")
					{
						Catalog catalog = new Catalog
						{
							Seq = "99",
							Cat = "Brands",
							SCat = createItemDto.BrandNew,
							SSCat = ""
						};
						if (await categoryExists(catalog))
							await _context.Category.AddAsync(catalog);
					}
					if (createItemDto.CatNew != null && createItemDto.CatNew != "")
					{
						Catalog catalog = new Catalog
						{
							Seq = "99",
							Cat = createItemDto.CatNew,
							SCat = "",
							SSCat = ""
						};
						if (await categoryExists(catalog))
							await _context.Category.AddAsync(catalog);
					}

					if (createItemDto.CatNew != null && createItemDto.CatNew != "" && createItemDto.SCatNew != null && createItemDto.SCatNew != "")
					{
						Catalog catalog = new Catalog
						{
							Seq = "99",
							Cat = createItemDto.CatNew ?? "",
							SCat = createItemDto.SCatNew ?? "",
							SSCat = ""
						};
						if (await categoryExists(catalog))
							await _context.Category.AddAsync(catalog);
					}

					if (createItemDto.CatNew != null && createItemDto.CatNew != "" && createItemDto.SCatNew != null && createItemDto.SCatNew != "" && createItemDto.SSCatNew != null && createItemDto.SSCatNew != "")
					{
						Catalog catalog = new Catalog
						{
							Seq = "99",
							Cat = createItemDto.CatNew ?? "",
							SCat = createItemDto.SCatNew ?? "",
							SSCat = createItemDto.SSCatNew ?? ""
						};
						if (await categoryExists(catalog))
							await _context.Category.AddAsync(catalog);
					}

					/*	add to dbcontext	*/
					var addToContextResult = await _context.SaveChangesAsync();
					dbTransaction.Commit();

					MessageDto messageDto = new MessageDto();
					messageDto.Processed.Add(new
					{
						Msg = "Processed successfully ",
						Code = newCode
					});
					messageDto.Code = "0";
					messageDto.Msg = "success!";
					return Ok(messageDto);
				}
				catch (Exception ex)
				{
					dbTransaction.Rollback();
					_logger.LogError(ex.Message + "\r\n" + $"Error, cannot add new item");
					return BadRequest(ex);
				}
				finally
				{

				}
			}
		}

		[HttpPatch("{auth}/editItem/{branchId}/{code}/{timestamp}")]
		public async Task<IActionResult> editItem(int branchId, int code, int timestamp, [FromBody] JsonPatchDocument<EditItemDto> patchDoc)
		{


			if (!ModelState.IsValid)
			{
				MessageDto message = new MessageDto();
				message.Processed.Add(new
				{
					Msg = "Process fail, model validation fail! ",
					Code = code
				});
				message.Code = "1";
				message.Msg = "fail!";
				return BadRequest(message);
			}

			if (patchDoc == null)
			{
				MessageDto message = new MessageDto();
				message.Processed.Add(new
				{
					Msg = "Process fail, model validation fail! ",
					Code = code
				});
				message.Code = "1";
				message.Msg = "fail!";
				return BadRequest(message);
			}
            try
            {
				//code_relations table to update
				var codeRelationsToUpdate = _context.CodeRelations.Where(c => c.Code == code)
					.FirstOrDefault();

				if (codeRelationsToUpdate == null)
				{
					_logger.LogError($"cannot find item ,code {code}");
					MessageDto message = new MessageDto();
					message.Processed.Add(new
					{
						Msg = "Cannot find this item! ",
						Code = code
					});
					message.Code = "1";
					message.Msg = "fail!";
					return NotFound(message);
				}

				var itemFromCloud = await _context.UpdatedItem.FirstOrDefaultAsync(c => c.ItemCode == code && c.BranchId == branchId);
				if (itemFromCloud != null)
				{
					int? timeStampFromDb = int.Parse(itemFromCloud.TimeStampS);
					if (timeStampFromDb != null)
					{
						if (timestamp < timeStampFromDb)
						{
							MessageDto messageDto = new MessageDto();
							messageDto.Processed.Add(new
							{
								Msg = "Process fail, cloud data is newer than local ",
								Code = code
							});
							messageDto.Code = "1";
							messageDto.Msg = "fail!";
							return Ok(messageDto);
						}
					}
				}

				//stock_qty table to update
				var stockQtyToUpdate = _context.StockQty.Where(c => c.Code == code && c.BranchId == branchId).FirstOrDefault();
				//if (stockQtyToUpdate == null)
				//{
				//	_logger.LogError($"cannot find code in stock_qty ,code {code}, branch {branchId}");
				//	return NotFound($"cannot find code in stock_qty ,code {code}, branch {branchId}");
				//}

				//code_branch to update
				var codeBranchToUpdate = _context.CodeBranch.Where(c => c.Code == code && c.BranchId == branchId).FirstOrDefault();
				//if (codeBranchToUpdate == null)
				//{
				//	_logger.LogError($"cannot find code in code_branch ,code {code}, branch {branchId}");
				//	return NotFound($"cannot find code in code_branch ,code {code}, branch {branchId}");
				//}

				using (var dbTransaction = await _context.Database.BeginTransactionAsync())
				{
					try
					{
						/*	1. update code_relations */
						var codeRelationsToPatch = new EditItemDto()
						{
							Name = codeRelationsToUpdate.Name,
							Description = codeRelationsToUpdate.NameCn,
							Price = codeRelationsToUpdate.Price1,
							Cost = codeRelationsToUpdate.ManualCostFrd,
							Cat = codeRelationsToUpdate.Cat,
							SCat = codeRelationsToUpdate.SCat,
							SSCat = codeRelationsToUpdate.SsCat,
							SupplierCode = codeRelationsToUpdate.SupplierCode,
							AutoWeigh = codeRelationsToUpdate.HasScale,
							PriceBarcode = codeRelationsToUpdate.IsBarcodeprice,
							IdCheck = codeRelationsToUpdate.IsIdCheck,
							IsSpecial = codeRelationsToUpdate.IsSpecial,
							SpecialPrice = codeRelationsToUpdate.SpecialPrice,
							SpecialStartTime = codeRelationsToUpdate.SpecialPriceStartDate ?? DateTime.MaxValue,
							SpecialEndTime = codeRelationsToUpdate.SpecialPriceEndDate ?? DateTime.MinValue,
							PromoId = codeRelationsToUpdate.PromoId,
							Barcode = codeRelationsToUpdate.Barcode,
							LevelPrice1 = codeRelationsToUpdate.LevelPrice1,
							LevelPrice2 = codeRelationsToUpdate.LevelPrice2,
							LevelPrice3 = codeRelationsToUpdate.LevelPrice3,
							LevelPrice4 = codeRelationsToUpdate.LevelPrice4,
							LevelPrice5 = codeRelationsToUpdate.LevelPrice5,
							LevelPrice6 = codeRelationsToUpdate.LevelPrice6
						};

						patchDoc.ApplyTo(codeRelationsToPatch, ModelState);
						if (!ModelState.IsValid)
							return BadRequest(ModelState);

						codeRelationsToUpdate.Name = codeRelationsToPatch.Name;
						codeRelationsToUpdate.NameCn = codeRelationsToPatch.Description;
						codeRelationsToUpdate.Price1 = codeRelationsToPatch.Price ?? 0;
						codeRelationsToUpdate.ManualCostFrd = codeRelationsToPatch.Cost ?? 0;
						codeRelationsToUpdate.AverageCost = codeRelationsToPatch.Cost ?? 0;
						codeRelationsToUpdate.Cat = codeRelationsToPatch.Cat;
						codeRelationsToUpdate.SCat = codeRelationsToPatch.SCat;
						codeRelationsToUpdate.SsCat = codeRelationsToPatch.SSCat;
						codeRelationsToUpdate.SupplierCode = codeRelationsToPatch.SupplierCode;
						codeRelationsToUpdate.HasScale = codeRelationsToPatch.AutoWeigh ?? false;
						codeRelationsToUpdate.IsBarcodeprice = codeRelationsToPatch.PriceBarcode ?? false;
						codeRelationsToUpdate.IsIdCheck = codeRelationsToPatch.IdCheck ?? false;
						codeRelationsToUpdate.IsSpecial = codeRelationsToPatch.IsSpecial;
						codeRelationsToUpdate.SpecialPrice = codeRelationsToPatch.SpecialPrice;
						codeRelationsToUpdate.SpecialPriceStartDate = codeRelationsToPatch.SpecialStartTime;
						codeRelationsToUpdate.SpecialPriceEndDate = codeRelationsToPatch.SpecialEndTime;
						codeRelationsToUpdate.PromoId = codeRelationsToPatch.PromoId;
						codeRelationsToUpdate.Barcode = codeRelationsToPatch.Barcode;
						codeRelationsToUpdate.LevelPrice1 = codeRelationsToPatch.LevelPrice1 ?? 0;
						codeRelationsToUpdate.LevelPrice2 = codeRelationsToPatch.LevelPrice2 ?? 0;
						codeRelationsToUpdate.LevelPrice3 = codeRelationsToPatch.LevelPrice3 ?? 0;
						codeRelationsToUpdate.LevelPrice4 = codeRelationsToPatch.LevelPrice4 ?? 0;
						codeRelationsToUpdate.LevelPrice5 = codeRelationsToPatch.LevelPrice5 ?? 0;
						codeRelationsToUpdate.LevelPrice6 = codeRelationsToPatch.LevelPrice6 ?? 0;

						/*	update code_branch	*/
						if (codeBranchToUpdate != null)
						{
							var codeBranchToPatch = new EditItemDto()
							{
								BranchId = branchId,
								Price = codeBranchToUpdate.Price1
							};

							patchDoc.ApplyTo(codeBranchToPatch, ModelState);
							if (!ModelState.IsValid)
								return BadRequest(ModelState);

							codeBranchToUpdate.Price1 = codeBranchToPatch.Price;
							codeBranchToUpdate.Inactive = false;
						}

						/*	update stock_qty	*/
						if (stockQtyToUpdate != null)
						{
							var stockQtyToPatch = new EditItemDto
							{
								Qty = stockQtyToUpdate.Qty
							};
							patchDoc.ApplyTo(stockQtyToPatch, ModelState);
							if (!ModelState.IsValid)
								return BadRequest(ModelState);
							stockQtyToUpdate.Qty = stockQtyToPatch.Qty;
						}
						var updateResult = await _context.SaveChangesAsync();
						dbTransaction.Commit();

						MessageDto messageDto = new MessageDto();
						messageDto.Processed.Add(new
						{
							Msg = "Processed successfully ",
							Code = code
						});
						messageDto.Code = "0";
						messageDto.Msg = "success!";
						return Ok(messageDto);
					}
					catch (Exception ex)
					{

						dbTransaction.Rollback();
						_logger.LogError(ex.Message + "\r\n" + $"Error, cannot edit item");
						return BadRequest(ex);
					}
				}
			}
			catch (Exception ex)
            {

				_logger.LogError(ex.Message + "\r\n" + $"Error, cannot edit item");
				return BadRequest(ex);
			}
		}

		[HttpPost("{auth}/editBarcodeAndSpecial/{branchId}/{code}/{timestamp}")]
		public async Task<IActionResult> editBarcodeAndSpecial(int branchId, int code, int timestamp, [FromBody] EditItemDto editItemDto)
		{
			if (!ModelState.IsValid)
				return BadRequest();

			var itemFromCloud = await _context.UpdatedItem.FirstOrDefaultAsync(c => c.ItemCode == code && c.BranchId == branchId);
			if (itemFromCloud != null)
			{
				int? timeStampFromDb = int.Parse(itemFromCloud.TimeStampS);
				if (timeStampFromDb != null)
				{
					if (timestamp < timeStampFromDb)
					{
						MessageDto messageDto = new MessageDto();
						messageDto.Processed.Add(new
						{
							Msg = "Process fail, cloud data is newer than local ",
							Code = code
						});
						messageDto.Code = "1";
						messageDto.Msg = "fail!";
						return Ok(messageDto);
					}
				}
			}

			using (var dbTransaction = await _context.Database.BeginTransactionAsync())
			{
				try
				{
					var barcodesInCloud = await _context.Barcode.Where(b => b.ItemCode == code).Select(c => c.Barcode1).ToListAsync();
					var barcodes = editItemDto.Barcodes;
					if (barcodes.Count > 0)
					{
						foreach (var barcode in barcodes)
						{
							if (!barcodesInCloud.Contains(barcode.Barcode))
							{
								Barcode newBarcode = new Barcode
								{
									ItemCode = code,
									ItemQty = barcode.ItemQty,
									Barcode1 = barcode.Barcode,
									PackagePrice = barcode.PackagePrice
								};
								var addBarcodeResult = await _context.Barcode.AddAsync(newBarcode);
							}
						}
					}


					var storeSpecialInCloud = await _context.StoreSpecial.Where(b => b.Code == code && b.BranchId == branchId).FirstOrDefaultAsync();

					StoreSpecial storespecial = new StoreSpecial
					{
						Code = code,
						BranchId = branchId,
						Enabled = editItemDto.IsSpecial,
						Price = editItemDto.SpecialPrice ?? 0,
						PriceStartDate = editItemDto.SpecialStartTime,
						PriceEndDate = editItemDto.SpecialEndTime
					};

					if (storeSpecialInCloud == null)
					{
						var addStoreSpecialResult = await _context.StoreSpecial.AddAsync(storespecial);
					}
					else
					{
						storeSpecialInCloud.BranchId = storespecial.BranchId;
						storeSpecialInCloud.Enabled = storespecial.Enabled;
						storeSpecialInCloud.Price = storespecial.Price;
						storeSpecialInCloud.PriceStartDate = storespecial.PriceStartDate;
						storeSpecialInCloud.PriceEndDate = storespecial.PriceEndDate;
						var updateStoreSpecialResult = _context.StoreSpecial.Update(storeSpecialInCloud);
					}

					var addToContextResult = await _context.SaveChangesAsync();
					dbTransaction.Commit();
					MessageDto messageDto = new MessageDto();
					messageDto.Processed.Add(new
					{
						Msg = "Processed successfully ",
						Code = code
					});
					messageDto.Code = "0";
					messageDto.Msg = "success!";
					return Ok(messageDto);
				}
				catch (Exception ex)
				{
					dbTransaction.Rollback();
					_logger.LogError(ex.Message + "\r\n" + $"Error");
					return BadRequest(ex);
				}
			}

		}

		[HttpPost("{auth}/uploadItems/{branchId}")]
		public async Task<IActionResult> uploadItems(int branchId, [FromBody] IEnumerable<UploadItemDto> uploadItemList)
		{
			MessageDto messageDto = new MessageDto();
			if (!ModelState.IsValid)
			{
				messageDto.Code = "1";
				messageDto.Msg = "model validation fail!";
				return BadRequest(messageDto);
			}
			if (uploadItemList == null)
			{
				messageDto.Code = "1";
				messageDto.Msg = "no item found in item list!";
				return BadRequest(messageDto);
			}
			try
			{
				foreach (var createItemDto in uploadItemList)
				{
					var code = createItemDto.Code;
					if (await _context.CodeRelations.AnyAsync(c => c.Code == code)) //item exists! then pass
					{
						messageDto.Skipped.Add(new { Msg = "This item exists on cloud ", Code = code });
						continue;
					}

					//if item does not exist, insert into cloud db
					using (var dbTransaction = await _context.Database.BeginTransactionAsync())
					{
						try
						{
							if (createItemDto.SupplierCode == null || createItemDto.SupplierCode == "")
							{
								createItemDto.SupplierCode = code.ToString();
							}
							/*	1. add to code_relations */
							CodeRelations codeRelations = new CodeRelations()
							{
								Id = createItemDto.Id.ToString(),
								Code = code,
								Name = createItemDto.Name,
								NameCn = createItemDto.Description,
								SupplierCode = createItemDto.SupplierCode,
								Brand = createItemDto.Brand,
								Cat = createItemDto.Cat,
								SCat = createItemDto.SCat,
								SsCat = createItemDto.SSCat,
								Price1 = createItemDto.Price ?? 0,
								ManualCostFrd = createItemDto.Cost ?? 0,
								SupplierPrice = createItemDto.Cost ?? 0,
								AverageCost = createItemDto.Cost ?? 0,
								Barcode = createItemDto.Barcode,
								HasScale = createItemDto.AutoWeigh ?? false,
								IsBarcodeprice = createItemDto.PriceBarcode ?? false,
								IsIdCheck = createItemDto.IdCheck ?? false
							};
							var addCodeRelationsResult = await _context.CodeRelations.AddAsync(codeRelations);

							/*	2. add to product	*/
							Product product = new Product
							{
								Code = code,
								Name = createItemDto.Name,
								NameCn = createItemDto.Description,
								SupplierCode = createItemDto.SupplierCode,
								Brand = createItemDto.Brand,
								Cat = createItemDto.Cat,
								SCat = createItemDto.SCat,
								SSCat = createItemDto.SSCat,
								Hot = false,
								Price = createItemDto.Price ?? 0,
								SupplierPrice = createItemDto.Cost ?? 0
							};
							var addProductResult = await _context.Products.AddAsync(product);

							/*	3. add to barcode	*/
							if (!await itemBarcodeExists(createItemDto.Barcode))
							{
								Barcode barcode = new Barcode
								{
									ItemCode = code,
									Barcode1 = createItemDto.Barcode
								};
								var addBarcodeResult = await _context.Barcode.AddAsync(barcode);
							}

							/*	4. add to stock_qty	*/
							StockQty stockQty = new StockQty
							{
								Code = code,
								Qty = 0,
								BranchId = createItemDto.BranchId,
								SupplierPrice = createItemDto.Cost ?? 0,
								AllocatedStock = 0,
								AverageCost = 0,
								QposPrice = 0,
								WarningStock = 0,
								LastStock = 0,
								SpStartDate = DateTime.Now,
								SpEndDate = DateTime.Now
							};
							var addStockQtyResult = await _context.StockQty.AddAsync(stockQty);

							/*	5. add to code_branch	*/
							CodeBranch codeBranch = new CodeBranch
							{
								Inactive = false,
								Code = code,
								BranchId = createItemDto.BranchId,
								Price1 = createItemDto.Price,
								Price2 = 0

							};
							var addCodeBranchResult = await _context.CodeBranch.AddAsync(codeBranch);

							/*	6. add to catalog	*/
							if (createItemDto.Brand != null && createItemDto.Brand != "")
							{
								Catalog catalog = new Catalog
								{
									Seq = "99",
									Cat = "Brands",
									SCat = createItemDto.Brand,
									SSCat = ""
								};
								if (await categoryExists(catalog))
									await _context.Category.AddAsync(catalog);
							}
							if (createItemDto.Cat != null && createItemDto.Cat != "")
							{
								Catalog catalog = new Catalog
								{
									Seq = "99",
									Cat = createItemDto.Cat,
									SCat = "",
									SSCat = ""
								};
								if (await categoryExists(catalog))
									await _context.Category.AddAsync(catalog);
							}

							if (createItemDto.Cat != null && createItemDto.Cat != "" && createItemDto.SCat != null && createItemDto.SCat != "")
							{
								Catalog catalog = new Catalog
								{
									Seq = "99",
									Cat = createItemDto.Cat ?? "",
									SCat = createItemDto.SCat ?? "",
									SSCat = ""
								};
								if (await categoryExists(catalog))
									await _context.Category.AddAsync(catalog);
							}

							if (createItemDto.Cat != null && createItemDto.Cat != "" && createItemDto.SCat != null && createItemDto.SCat != "" && createItemDto.SSCat != null && createItemDto.SSCat != "")
							{
								Catalog catalog = new Catalog
								{
									Seq = "99",
									Cat = createItemDto.Cat ?? "",
									SCat = createItemDto.SCat ?? "",
									SSCat = createItemDto.SSCat ?? ""
								};
								if (await categoryExists(catalog))
									await _context.Category.AddAsync(catalog);
							}

							/*	add to dbcontext	*/
							var addToContextResult = await _context.SaveChangesAsync();
							dbTransaction.Commit();

							messageDto.Processed.Add(new
							{
								Msg = "Processed successfully ",
								Code = code
							});
							//messageDto.Code = "0";
							//messageDto.Msg = "success!";
							//return Ok(messageDto);
						}
						catch (Exception ex)
						{
							dbTransaction.Rollback();
							_logger.LogError(ex.Message + "\r\n" + $"Error, cannot add new item");
							return BadRequest(ex);
						}
						finally
						{

						}
					}
				}
				messageDto.Code = "0";
				messageDto.Msg = "success!";
				return Ok(messageDto);

			}
			catch (Exception ex)
			{

				_logger.LogError(ex.Message + "\r\n" + $"Error, cannot add new item");
				return BadRequest(ex);
			}

		}
	}
}
