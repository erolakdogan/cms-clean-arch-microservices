using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Diagnostics;


namespace Shared.Web.Middleware
{
    public sealed class CorrelationIdMiddleware(RequestDelegate next)
    {
        public const string HeaderName = "X-Correlation-Id";

        public async Task Invoke(HttpContext ctx)
        {
            var correlationId = ctx.Request.Headers.TryGetValue(HeaderName, out var h) && !string.IsNullOrWhiteSpace(h)
                ? h.ToString()
                : Activity.Current?.Id ?? Guid.NewGuid().ToString("N");

            ctx.Response.Headers[HeaderName] = correlationId;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await next(ctx);
            }
        }
    }
}
