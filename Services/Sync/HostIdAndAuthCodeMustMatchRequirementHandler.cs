using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using eCommerce_API_RST_Multi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using System.Text;
using Sync.Data;
using Microsoft.Extensions.Configuration;

namespace eCommerce_API_RST_Multi.Services.Sync
{
    public class HostIdAndAuthCodeMustMatchRequirementHandler: AuthorizationHandler<HostIdAndAuthCodeMustMatchRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HostDbContext _context;
        private readonly IConfiguration _configuration;
        public HostIdAndAuthCodeMustMatchRequirementHandler(IHttpContextAccessor httpContextAccessor, HostDbContext context, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _configuration = configuration;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HostIdAndAuthCodeMustMatchRequirement requirement)
        {
            //var hostUser = _context.HostTenants.Select(h => new { h.Id, h.AuthCode }).ToDictionary(c => c.Id.ToString(), c => c.AuthCode.ToString());

            var hostId = _httpContextAccessor.HttpContext.GetRouteValue("hostId").ToString();
            var authCode = _httpContextAccessor.HttpContext.GetRouteValue("auth").ToString();
            var branchId = _httpContextAccessor.HttpContext.GetRouteValue("branchId").ToString();

            var webapp = _context.webApps.FirstOrDefault(c => c.Id.ToString() == hostId);
            if (webapp == null)
            {
                string authorizeResult = $"fail, cannot find recored in host db, id: {hostId}.";
                byte[] bytes;
                var httpContext = _httpContextAccessor.HttpContext;
                bytes = Encoding.UTF8.GetBytes(authorizeResult);
                httpContext.Response.StatusCode = 405;
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                return Task.CompletedTask;
            }

            var dbid = webapp.DbId;
            var dbConnectionString = _context.HostTenants.Where(ht => ht.Id == dbid).Select(c=> new { c.Id,c.Connstr,c.Name}).FirstOrDefault();
            if(dbConnectionString != null)
            {
                DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                                                            .UseSqlServer(dbConnectionString.Connstr).Options;
                    
                    //new DbContextOptions<AppDbContext>();
                using AppDbContext appDbContext = new AppDbContext(options, _configuration, _context, _httpContextAccessor);

                var branchExists = appDbContext.Branch.Any(b => b.Id.ToString() == branchId && b.Activated == true && b.AuthCode == authCode);
                if (branchExists)
                    context.Succeed(requirement);
                else
                {
                    string authorizeResult = "fail HostIdAndAuthCodeMatch, please contact your administrator.";
                    byte[] bytes;
                    var httpContext = _httpContextAccessor.HttpContext;
                    bytes = Encoding.UTF8.GetBytes(authorizeResult);
                    httpContext.Response.StatusCode = 405;
                    httpContext.Response.ContentType = "application/json";
                    httpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                }
            }

            //var hostIdAndauthCodePair = new KeyValuePair<string, string>(hostId, authCode);
            ///*  if hostTenant database contains this keyvalue pair, then pass   */
            //if (hostUser.Contains(hostIdAndauthCodePair))
            //    context.Succeed(requirement);
            else
            {
                string authorizeResult = $"fail, cannot find recored in host db, id: {hostId}.";
                byte[] bytes;
                var httpContext = _httpContextAccessor.HttpContext;
                bytes = Encoding.UTF8.GetBytes(authorizeResult);
                httpContext.Response.StatusCode = 405;
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
            }

            return Task.CompletedTask;
        }
    }
}
