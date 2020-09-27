using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using API.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API.Middleware
{
    //Middle ware to handle exception
    public class ExceptionMiddleware
    {
        //Handle exception based on the environment - Development vs production
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger,       
        IHostEnvironment env)
        {
            _env = env;
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
               _logger.LogError(ex, ex.Message);

               //Send error to the client
               context.Response.ContentType = "application/json";
               context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                //Only send stack trace in development environment and not production
               var response = _env.IsDevelopment()
                        ? new ApiException((int)HttpStatusCode.InternalServerError, ex.Message,
                         ex.StackTrace.ToString())
                         : new ApiException((int)HttpStatusCode.InternalServerError);
            
                //Convert json response to Camel case instead of Pascal case
                var options = new JsonSerializerOptions{PropertyNamingPolicy = 
                    JsonNamingPolicy.CamelCase};

                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
}
}