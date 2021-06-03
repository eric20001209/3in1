using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eCommerce_API.Dto;

namespace eCommerce_API_RST.Dto
{
	public class InvoiceForSyncDto
	{
		//orders
		public int Branch { get; set; }
		public int CardId { get; set; }
		public string PoNumber { get; set; }
		public string Status { get; set; }
		public int? InvoiceNumber { get; set; }
		public DateTime RecordDate { get; set; }
		public DateTime DateShipped { get; set; }
		public int? SalesId { get; set; }
		public string SalesNote { get; set; }
		public byte ShippingMethod { get; set; }
		public int Type{ get; set; }
		public decimal OrderTotal { get; set; }
		public double CustomerGst { get; set; }

		//invoice
		public decimal Price { get; set; }
		public decimal Tax { get; set; }
		public decimal Freight { get; set; }
		public decimal Total{ get; set; }
		public decimal AmountPaid { get; set; }
		public DateTime CommitDate { get; set; }
		public bool Paid { get; set; }
		public int Points { get; set; }

		//orders and invoice
		//public int PaymentType { get; set; }
		public string SalesName { get; set; }
		public int StationId { get; set; }

		public virtual IEnumerable<OrderItemDto> OrderItems { get; set; }  = new List<OrderItemDto>();


	}
}
