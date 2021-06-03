using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Dtos
{
	public class CardDto
	{
		public int Id { get; set; }
		public string Email { get; set; }
		public string Password{ get; set; }
		public int	Type{ get; set; }
		public string Name { get; set; }
		public string ShortName { get; set; }
		public string TradingName { get; set; }
		public string Company { get; set; }
		public string Address1 { get; set; }
		public string Address2 { get; set; }
		public string Address3 { get; set; }
		public string City { get; set; }
		public string Country { get; set; }
		public string Phone { get; set; }
		public string Fax { get; set; }
		public string Contact { get; set; }
		public int AccessLevel { get; set; }
		public string Barcode { get; set; }
		public  double Discount { get; set; }
		public double? MDiscountRate { get; set; }
		public double Points { get; set; }
//		public bool HasExpired { get; set; }
		public byte Language { get; set; }
		public int PriceLevel{ get; set; }
//		public byte MemberGroup { get; set; }
		public string Mobile { get; set; }
//		public bool Delete { get; set; }
		public byte[] TimeStamp { get; set; }
		
//		contact, access_level, barcode, discount, m_discount_rate*100 AS m_discount_rate, has_expired, language, price_level, member_group, mobile
	}
}
