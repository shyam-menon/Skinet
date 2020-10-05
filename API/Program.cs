using System;
using System.Threading.Tasks;
using Core.Entities.Identity;
using Infrastructure.Data;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
           var host = CreateHostBuilder(args).Build();
           using (var scope = host.Services.CreateScope())
           {
               var services = scope.ServiceProvider;
               var loggerFactory = services.GetRequiredService<ILoggerFactory>();
               try
               {
                   var context = services.GetRequiredService<StoreContext>();
                   //Apply pending migrations and create the database if not existing
                   await context.Database.MigrateAsync();

                   //Seed the database
                   await StoreContextSeed.SeedAsync(context, loggerFactory);

                   //Set up Identity. 167 course item
                   var userManager = services.GetRequiredService<UserManager<AppUser>>();
                   var identityContext = services.GetRequiredService<AppIdentityDbContext>(); 

                   await identityContext.Database.MigrateAsync();
                   await AppIdentityDbContextSeed.SeedUserAsync(userManager);
               }
               catch(Exception ex)
               {
                   var logger = loggerFactory.CreateLogger<Program>();
                   logger.LogError(ex, "An error occured during migration");
               }
           }

           host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
