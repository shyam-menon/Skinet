using API.Extensions;
using API.Helpers;
using API.Middleware;
using AutoMapper;
using Infrastructure.Data;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace API
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
            services.AddAutoMapper(typeof(MappingProfiles));
            services.AddControllers();
            services.AddDbContext<StoreContext>(x => x.UseSqlite
            (Configuration.GetConnectionString("DefaultConnection")));
            //DB Context for Identity
            services.AddDbContext<AppIdentityDbContext>(x => 
            {
                x.UseSqlite(Configuration.GetConnectionString("IdentityConnection"));
            });

            //Add Redis connection as Singleton as it is shared and reused among callers            
            services.AddSingleton<IConnectionMultiplexer>(c => {
                var configuration = ConfigurationOptions.Parse(Configuration.GetConnectionString("Redis"),
                true);
                return ConnectionMultiplexer.Connect(configuration);
            });          

            //Add the services that we need using an extension method
            services.AddApplicationServices();

            //Add Identity service and authentication. 166 course item.
            services.AddIdentityServices(Configuration);

            //Add Swagger using the extention method for it
            services.AddSwaggerDocumentation();

            //Add CORS support by adding a CORS header "Access-control-Allow-Origin" that will 
            //allow it to display information if the client is request from a secured port i.e.
            //the origin from where we are allowed to access the resource from
            services.AddCors(opt => 
            {
                opt.AddPolicy("CorsPolicy", policy => 
                {
                    policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200");
                });
            });
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //Use custom exception middleware instead of the default
            app.UseMiddleware<ExceptionMiddleware>();

            //When request comes to API server for which there is no endpoint that matches the request
            //redirect to errors controller passing in the status code and in errors controller return
            //new object result along with the status code which will result in the right response
            app.UseStatusCodePagesWithReExecute("/errors/{0}");

            app.UseHttpsRedirection();

            app.UseRouting();

            //To serve static content
            app.UseStaticFiles();

            //Set up CORS policy
            app.UseCors("CorsPolicy");

            //Set up authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            //Configure Swagger
            app.UseSwaggerDocumentation();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
