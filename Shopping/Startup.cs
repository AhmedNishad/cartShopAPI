using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopping.Business;
using Shopping.Business.Entities;
using Shopping.Business.Services;
using Shopping.Data;
using Shopping.Data.Access;
using Shopping.Data.Interfaces;

using Shopping.ViewModels;

namespace Shopping
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public Mapper Mapper { get; set; }
        public IConfiguration Configuration { get; }
        public MapperConfiguration MapperConfig { get; set; }
        public ProductService productService { get; }
        public CustomerService CustomerService { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAutoMapper(new MappingProfile().GetType().Assembly);

            services.AddDbContext<ShoppingContext>(options =>
                    options.UseSqlServer(
                        Configuration.GetConnectionString("ShoppingContextConnection"), x => x.MigrationsAssembly("Shopping.Data")));

            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<ShoppingContext>();

            services.AddTransient<IProductService,ProductService>();
            services.AddTransient<ICustomerService,CustomerService>();
            services.AddTransient<IOrderService,OrderService>();

            services.AddScoped<ICustomerRepository,CustomerRepository>();
            services.AddScoped<IProductRepository,ProductRepository>();
            services.AddScoped<IOrderRepository,OrderRepository>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles( new StaticFileOptions()
            {
                OnPrepareResponse = context =>
                {
                    if (env.IsDevelopment())
                    {
                        context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
                        context.Context.Response.Headers.Add("Expires", "-1");
                    }
                }
            });
            app.UseAuthentication();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
