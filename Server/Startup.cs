using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication("OAuth")
                    .AddJwtBearer("OAuth",
                                  config =>
                                  {
                                      // told server what key we are using and the valid issuer Audiance
                                      // to validate the token
                                      config.TokenValidationParameters = new TokenValidationParameters
                                      {
                                          ValidIssuer = ConfigConstants.Issuer,
                                          ValidAudience = ConfigConstants.Audiance,
                                          IssuerSigningKey = ConfigConstants.GetKey()
                                      };
                                  });

            services.AddControllersWithViews();
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
