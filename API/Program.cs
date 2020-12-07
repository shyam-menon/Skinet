using System;
using System.IO;
using System.Threading.Tasks;
using Core.Entities.Identity;
using Infrastructure.Data;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Json;

namespace API
{  

    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
           .AddEnvironmentVariables()
           .Build();

        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
               .ReadFrom.Configuration(Configuration)
               .WriteTo.File(new JsonFormatter(), @"c:\temp\logs\skinet.json", shared: true)
               .WriteTo.EventCollector("http://localhost:8088/services/collector", "45bc694e-2f75-4d24-bc01-a8a85891afd6")
               //.WriteTo.Seq("http://localhost:5342")
               .CreateLogger();

            Log.Information("Starting Skinet web host");
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
                   Log.Fatal(ex, "Host terminated unexpectedly");
               }
           }

           host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
            .UseSerilog();
    }
}
