using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eCommerce_API.Data;
using eCommerce_API.Dto;

namespace eCommerce_API_RST.Services.Utility
{
	public  class PdfInvoiceDataStorage
	{
		private readonly IOrder _order;
		public PdfInvoiceDataStorage(IOrder order)
		{
			_order = order;
		}
		public InvoiceDto getInvoice(int orderId)
		{
			return _order.getOrderDetail(orderId);
		}
	}
}
