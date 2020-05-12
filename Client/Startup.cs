using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Client
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(config =>
                    {
                        // check the cookie to confirm we are authenticated
                        config.DefaultAuthenticateScheme = "ClientCookie";

                        // cookie generate after sign in
                        config.DefaultSignInScheme = "ClientCookie";

                        // use this to check if user are allowed to access
                        config.DefaultChallengeScheme = "AuthServer";
                    })
                    .AddCookie("ClientCookie")
                    .AddOAuth("AuthServer",
                              config =>
                              {
                                  config.ClientId = "client_id";
                                  config.ClientSecret = "client_secret";
                                  config.CallbackPath = "/oauth/callback";

                                  // server endpoint
                                  config.AuthorizationEndpoint = "https://localhost:44345/oauth/auth";
                                  config.TokenEndpoint = "https://localhost:44345/oauth/token";
                                  config.SaveTokens = true; // told the client to save the token so that can use it

                                  config.Events = new OAuthEvents
                                  {
                                      OnCreatingTicket = context =>
                                      {
                                          var accessToken = context.AccessToken;
                                          var base64payload = accessToken.Split('.')[1];
                                          var bytes = Convert.FromBase64String(base64payload);
                                          var jsonPayload = Encoding.UTF8.GetString(bytes);

                                          var claims =
                                              JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonPayload);

                                          foreach (var claim in claims)
                                          {
                                              context.Identity.AddClaim(new Claim(claim.Key, claim.Value));
                                          }

                                          return Task.CompletedTask;
                                      }
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
