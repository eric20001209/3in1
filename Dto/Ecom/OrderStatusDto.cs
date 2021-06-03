using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace eCommerce_API_RST.Dto
{
	public class OrderStatusDto
	{
		public int Id { get; set; }
		public string PO_number { get; set; }
		public string Status { get; set; }
		public string Reciever { get; set; }
		[DataType(DataType.PhoneNumber)]
		public string Phone{ get; set; }

	}
}
