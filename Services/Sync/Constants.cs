using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sync.Services
{
	public class Constants
	{
		//system claim
		public const string SUPER_ADMIN = "Super Admin";
		public const string TENANT_ADMIN = "Tenant Admin";
		public const string END_USER = "End User";

		//policies
		public const string CURRENT_USER = "Current User";
		public const string ORDER_BELONG_TO_USER = "Order Belong To User";

		//Common
		public const string USER_ID = "User Id";
		public const string ORDER_ID = "Order Id";
		public const string TENANT_ID = "Tenant Id";
		public const string TENANTS = "Tenants";
		public const string TRUE = "True";
		public const string FALSE = "False";
	}
}
