using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace eCommerce_API_RST_Multi.Services
{
    interface IDirectRouteProvider
    {
        IReadOnlyList<RouteEntry> GetDirectRoutes(
        HttpControllerDescriptor controllerDescriptor,
        IReadOnlyList<HttpActionDescriptor> actionDescriptors,
        IInlineConstraintResolver constraintResolver);
    }
}
