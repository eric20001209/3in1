using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using eCommerce_API_RST_Multi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;

namespace eCommerce_API_RST_Multi.Services.Sync
{
    public class HostIdAndAuthCodeMustMatchRequirement :IAuthorizationRequirement
    {

    }
}
