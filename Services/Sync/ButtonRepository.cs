using Sync.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Sync.Services
{
	public interface IButton
	{
		ICollection<ButtonItemDto> getButtonItems(int button_id);
		ButtonItemDto getButtonItem(int button_id);
		string getBarcodeForIndiButton(int button_id);
	}
	public class ButtonRepository : IButton
	{
		private readonly Sync.Data.AppDbContext _context;
		public ButtonRepository(Data.AppDbContext context)
		{
			_context = context;
		}
		public ICollection<ButtonItemDto> getButtonItems(int button_id)
		{
			var buttonItems = _context.ButtonItem.Where(b => b.ButtonId == button_id).ToList();
			if (buttonItems == null)
				return null;
			var final = buttonItems	.Select(b => new ButtonItemDto
								 {
									 Id = b.Id,
									 ButtonId = b.ButtonId,
									 Code = b.Code,
									 Name = b.Name,
									 NameEn = b.NameEn,
									 Location = b.Location

								 }).ToList();

			return final;
		}

		public ButtonItemDto getButtonItem(int button_id)
		{
			var buttonItem = _context.ButtonItem.Where(b => b.ButtonId == button_id).FirstOrDefault();
			if (buttonItem == null)
				return null;
			var final = new ButtonItemDto
			{
				//Id = buttonItem.Id
				//ButtonId = buttonItem.ButtonId,
				//Code = buttonItem.Code,
				//Name = buttonItem.Name,
				//NameEn = buttonItem.NameEn,
				//Location = buttonItem.Location

			};

			return final;
		}

		public string getBarcodeForIndiButton(int button_id)
		{
			var button = _context.Settings.Where(s => s.Name == "button_id_" + button_id).FirstOrDefault();
			if (button == null)
			{
				return "";
			}
			return button.Value;
		}
	}
}
