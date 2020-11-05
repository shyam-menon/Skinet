using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace API.Infrastructure
{
    public class TrackActionPerformanceFilter : IActionFilter
    {
        private Stopwatch _timer;
        private readonly ILogger<TrackActionPerformanceFilter> _logger;
        private readonly IScopeInformation _scopeInfo;
        private IDisposable _userScope;
        private IDisposable _hostScope;

        public TrackActionPerformanceFilter(
            ILogger<TrackActionPerformanceFilter> logger,
            IScopeInformation scopeInfo)
        {
            _logger = logger;
            _scopeInfo = scopeInfo;
        }
        public void OnActionExecuting(ActionExecutingContext context)
        {
            _timer = new Stopwatch();

            var userDict = new Dictionary<string, string>
            {
                { "UserName", context.HttpContext.User.Claims.FirstOrDefault(a => a.Type.Contains("name"))?.Value },
                { "Email", string.Join(",",
                        context.HttpContext.User.Claims.FirstOrDefault(a => a.Type.Contains("email"))?.Value ) }
            };
            _userScope = _logger.BeginScope(userDict);
            _hostScope = _logger.BeginScope(_scopeInfo.HostScopeInfo);

            _timer.Start();
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _timer.Stop();
            if (context.Exception == null)
            {
                _logger.LogRoutePerformance(context.HttpContext.Request.Path,
                    context.HttpContext.Request.Method,
                    _timer.ElapsedMilliseconds);
            }
            _userScope?.Dispose();
            _hostScope?.Dispose();
        }
    }
}
