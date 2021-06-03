using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using eCommerce_API.Data;
using eCommerce_API.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.IO;
using eCommerce_API_RST.Services;
using Sync.Services;
using DinkToPdf;
using DinkToPdf.Contracts;
using Sync.Data;
using FarroAPI.Entities;
using eCommerce_API_RST_Multi.Data;
using Microsoft.AspNetCore.Http;
using eCommerce_API_RST_Multi.Services.Sync;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;

namespace eCommerce_API
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            //Log.Logger = new LoggerConfiguration().CreateLogger();
            Configuration =  configuration ;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "http//wanfangapi.gpos.co.nz:81", // "http//localhost:8088",
                    ValidAudience = "http//wanfangapi.gpos.co.nz:81",   //"http//localhost:8088",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Startup.Configuration["TokenSecretKey"]))
                };
            });

            /* host dbcontext */
            services.AddDbContext<HostDbContext>(options =>
                        options.UseSqlServer(Configuration.GetConnectionString("hostDbContext")));

            /*  ecom dbcontext*/
            services.AddDbContext<rst374_cloud12Context>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("rst374_cloud12Context")));
            services.AddDbContext<FreightContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("rst374_cloud12Context")));
            /* Sync dbcontext*/
            services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("rst374_cloud12Context")), ServiceLifetime.Transient);
            /* mreport dbcontext*/
            services.AddDbContext<farroContext>(o => o.UseSqlServer(Configuration.GetConnectionString("rst374_cloud12Context")));

            //register configuration
            services.AddSingleton(provider => Configuration);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //services.AddSingleton<HttpContext>();
            services.AddHttpContextAccessor();


            /* automapper */
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapping());
            });

            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            services.AddCors();
//          string connectionString = @"Server=192.168.1.204\sql2012;Database=wanfang_cloud14;User Id=eznz;password=9seqxtf7";
            services.AddScoped<rst374_cloud12Context>();
            services.AddTransient<rst374_cloud12Context>();
            services.AddScoped<FreightContext>();

 //         services.AddScoped<AppDbContext>();

            services.AddScoped<ISetting, SettingsRepository>();
            services.AddScoped<eCommerce_API.Services.IItem, eCommerce_API.Services.ItemRepository>();
            services.AddScoped<IOrder, OrderRepository>();
            services.AddTransient<FreightContext>();
            services.AddTransient<iMailService, MailService>();
            Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll");
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));   //DinkToPdf DI

            /*  Authorize Sync  */
            services.AddScoped<IAuthorizationHandler, HostIdAndAuthCodeMustMatchRequirementHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("HostIdAndAuthCodeMustMatch", policy => policy.Requirements.Add(new HostIdAndAuthCodeMustMatchRequirement()));
            });
            /*  */

            //config automapper
            //services.AddAutoMapper(typeof(Startup));

            /*  Sync services */
            services.AddScoped<Sync.Services.IItem, Sync.Services.ItemRepository>();
            services.AddScoped<IButton, ButtonRepository>();
            services.AddScoped<ICard, CardRepository>();
            services.AddTransient<IImageService, ImageService>();



            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1",
                new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "eCom API",
                    Description = " eCom API Swagger",
                    Version = "v1"
                });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,  rst374_cloud12Context context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseAuthentication();
            app.UseStatusCodePages();
            app.UseDefaultFiles();
            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(options => {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "eCom_Management");
            });

            app.UseCors(builder => builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials());

            //AutoMapper.Mapper.Initialize(cfg => {
            //    cfg.CreateMap<Models.CodeRelations, Dto.ItemDto>();
            //    cfg.CreateMap<Models.OrderItem, Dto.OrderItemDto>();
            //});
            //app.UsePathBase("/{hostId}");
            app.UseMvc(
                //routes=> {
                //    routes.MapRoute(
                //    name: "default",
                //    template: "{area:hostId}/api/{controller=sync}/{auth}/{action=getcard}/{branchId?}");
                //}
                );
        }
    }
}
