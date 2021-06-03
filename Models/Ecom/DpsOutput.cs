using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce_API_RST.Models
{
	public class DpsOutput
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id{ get; set; }

		public string Valid { get; set; }
		public string AmountSettlement { get; set; }
		public string AuthCode { get; set; }
		public string CardName { get; set; }
		public string CardNumber { get; set; }
		public string DateExpiry { get; set; }
		public string DpsTxnRef { get; set; }
		public string Success { get; set; }
		public string ResponseText { get; set; }
		public string DpsBillingId { get; set; }
		public string CardHolderName { get; set; }
		public string CurrencySettlement { get; set; }
		public string PaymentMethod { get; set; }
		public string TxnData1 { get; set; }
		public string TxnData2 { get; set; }
		public string TxnData3 { get; set; }
		public string TxnType { get; set; }
		public string CurrencyInput { get; set; }
		public string MerchantReference { get; set; }
		public string ClientInfo { get; set; }
		public string TxnId { get; set; }
		public string EmailAddress { get; set; }
		public string BillingId { get; set; }
		public string TxnMac { get; set; }
		public string CardNumber2 { get; set; }
		public string Cvc2ResultCode { get; set; }
		public int OrderId { get; set; }
		public bool OrderSent { get; set; } = false;
	}
}
