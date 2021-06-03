using eCommerce_API.Dto;
using eCommerce_API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce_API_RST.Services
{
	public class TestRepository : ISetting
	{
		public decimal getDealerFreight(decimal total, int dfid)
		{
			throw new NotImplementedException();
		}

		public string getDealerRegion(int id)
		{
			throw new NotImplementedException();
		}

		public List<FreightInfoDto> getFreightInfo(int invoice_number)
		{
			throw new NotImplementedException();
		}

		public List<FreightInfoDto> getFreightInfo(string po_number)
		{
			throw new NotImplementedException();
		}

		public List<FreightInfoDto> getFreightInfoByPo(string po_number)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<FreightDto> getFreightSetting()
		{
			throw new NotImplementedException();
		}

		public int getIdByPaymentMethod(string paymentMethod)
		{
			throw new NotImplementedException();
		}

		public int getOnlineShopId()
		{
			throw new NotImplementedException();
		}

		public string getOrderStatus(int id)
		{
			throw new NotImplementedException();
		}

		public string getPaymentMethodById(int id)
		{
			throw new NotImplementedException();
		}

		public FreightDto getRegionFreightSetting(int id)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ShippingMethodDto> getShippingMethod()
		{
			throw new NotImplementedException();
		}
	}
}
