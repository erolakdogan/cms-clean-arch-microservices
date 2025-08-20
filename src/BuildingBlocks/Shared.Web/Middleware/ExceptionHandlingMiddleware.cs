using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Shared.Web.Middleware
{
    public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task Invoke(HttpContext ctx)
        {
            try
            {
                await next(ctx);
            }
            catch (ValidationException vex)
            {
                var pd = Create(ctx, StatusCodes.Status400BadRequest, "Validation failed", vex.Message);
                pd.Extensions["errors"] = vex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                await Write(ctx, pd);
            }
            catch (KeyNotFoundException kex)
            {
                await Write(ctx, Create(ctx, StatusCodes.Status404NotFound, "Not found", kex.Message));
            }
            catch (InvalidOperationException ioex)
            {
                await Write(ctx, Create(ctx, StatusCodes.Status409Conflict, "Conflict", ioex.Message));
            }
            catch (UnauthorizedAccessException uex)
            {
                await Write(ctx, Create(ctx, StatusCodes.Status401Unauthorized, "Unauthorized", uex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception");
                await Write(ctx, Create(ctx, StatusCodes.Status500InternalServerError, "Server error",
                                        "An unexpected error occurred."));
            }
        }

        static ProblemDetails Create(HttpContext ctx, int status, string title, string detail)
            => new()
            {
                Status = status,
                Title = title,
                Detail = detail,
                Type = $"https://httpstatuses.com/{status}",
                Instance = ctx.Request.Path
            };

        static async Task Write(HttpContext ctx, ProblemDetails pd)
        {
            pd.Extensions["traceId"] = ctx.TraceIdentifier;
            if (ctx.Response.Headers.TryGetValue(CorrelationIdMiddleware.HeaderName, out var cid))
                pd.Extensions["correlationId"] = cid.ToString();

            ctx.Response.ContentType = "application/problem+json";
            ctx.Response.StatusCode = pd.Status ?? StatusCodes.Status500InternalServerError;
            await ctx.Response.WriteAsJsonAsync(pd);
        }
    }
}
