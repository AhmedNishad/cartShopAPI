using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopping.Models;

[assembly: HostingStartup(typeof(Shopping.Areas.Identity.IdentityHostingStartup))]
namespace Shopping.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<ShoppingContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("ShoppingContextConnection")));

                services.AddDefaultIdentity<IdentityUser>()
                    .AddEntityFrameworkStores<ShoppingContext>();
            });
        }
    }
}