using Sync.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Services
{
	public interface ICard
	{
		int? getIdFromBarcode(string barcode);
	}
	public class CardRepository : ICard
	{
		private readonly AppDbContext _context;
		public CardRepository(AppDbContext context)
		{
			_context = context;
		}
		public int? getIdFromBarcode(string barcode)
		{

			var cardExists = _context.Cards.Any(c => c.Barcode == barcode);
			if (!cardExists)
				return null;
			var card = _context.Cards.Select(c=> new { c.Barcode, c.Id}).Where(c => c.Barcode == barcode).FirstOrDefault();
			if (card == null)
				return null;
			return card.Id;
		}
	}
}
