using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebAdvert.Web.Services.FileUploader;
using WebAdvert.Web.Services.FileUploader.Interface;
using AutoMapper;
using WebAdvert.Web.ServiceClients;
using Polly;
using System.Net.Http;
using Polly.Extensions.Http;

namespace WebAdvert.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddCognitoIdentity( config =>
            {
                config.Password = new Microsoft.AspNetCore.Identity.PasswordOptions
                {
                    RequireDigit = false,
                    RequiredLength = 6,
                    RequiredUniqueChars = 0,
                    RequireLowercase = false,
                    RequireNonAlphanumeric = false,
                    RequireUppercase = false
                };
            });

           
            services.ConfigureApplicationCookie(option =>
            {
                option.LoginPath = "/Accounts/Login";
            });

            services.AddAutoMapper();
            services.AddTransient<IFileUploader, FileUploader>();
            services.AddHttpClient<IAdvertApiClient, AdvertApiClient>().AddPolicyHandler(GetRetryPolicy())
                                                                        .AddPolicyHandler(GetCircuitBreakerPatternPolicy());

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            //adri
            services.Configure<IISServerOptions>(options =>
            {
                options.AutomaticAuthentication = false;
            });
            services.Configure<IISOptions>(options =>
            {
                options.ForwardClientCertificate = false;
            });
        }

        private IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPatternPolicy()
        {
            return HttpPolicyExtensions.HandleTransientHttpError().CircuitBreakerAsync(3, TimeSpan.FromSeconds(30));
        }

        private IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions.HandleTransientHttpError().OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                                            .WaitAndRetryAsync(retryCount: 5, sleepDurationProvider: retryAttemp => TimeSpan.FromSeconds(Math.Pow(2, retryAttemp)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

           
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Accounts}/{action=SignUp}/{id?}");
                routes.MapRoute(
                   name: "Confirm",
                   template: "{controller=Accounts}/{action=Confirm}");
            });
        }
    }
}
