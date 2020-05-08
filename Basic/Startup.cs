using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Basic.AuthReq;
using Basic.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Basic
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication("CookieAuth")
                    .AddCookie("CookieAuth",
                               config =>
                               {
                                   config.Cookie.Name = "MyCookie";

                                   // if no auth -> redirect to login path
                                   config.LoginPath = "/Home/Auth";
                               });

            services.AddAuthorization(config =>
            {
                config.AddPolicy("Admin",
                                 builder =>
                                 {
                                     builder.RequireClaim(ClaimTypes.Role, "Admin");
                                 });
                config.AddPolicy("Claim.DoB", builder => builder.RequireCustomClaim(ClaimTypes.DateOfBirth));
            });

            services.AddScoped<IAuthorizationHandler, CustomRequireClaimHandler>();
            services.AddScoped<IAuthorizationHandler, CookieJarAuthorizationHandler>();
            services.AddControllersWithViews(config =>
            {
               
                var defaultAuthBuilder=new AuthorizationPolicyBuilder();

                var defaultPolicy = defaultAuthBuilder
                                    .RequireAuthenticatedUser()
                                    .RequireClaim(ClaimTypes.DateOfBirth)
                                    .Build(); 
                // set global authorization filter
              //  config.Filters.Add(new AuthorizeFilter(defaultPolicy));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) { app.UseDeveloperExceptionPage(); }

            app.UseRouting();

            // who r u
            app.UseAuthentication();

            // ask if the request is allow to the page
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });
        }
    }
}
