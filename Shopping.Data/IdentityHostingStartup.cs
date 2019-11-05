using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopping.Data.Access;

[assembly: HostingStartup(typeof(Shopping.Data.IdentityHostingStartup))]
namespace Shopping.Data
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {

                services.AddDbContext<ShoppingContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("ShoppingContextConnection")));

                //services.AddDefaultIdentity<IdentityUser>()
                //    .AddEntityFrameworkStores<ShoppingContext>();

                //services.AddTransient<OrderRepository>();
                //services.AddTransient<CustomerRepository>();
                //services.AddTransient<ProductRepository>();
            });
            
        }
    }
}