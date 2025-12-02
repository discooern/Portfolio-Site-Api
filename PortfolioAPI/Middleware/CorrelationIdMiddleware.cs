using PortfolioAPI.Models;
using System.Text;

namespace PortfolioAPI.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        private const string CorrelationIdHeader = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (LogExcludeList.ExcludedPaths.Contains<string>(context.Request.Path, StringComparer.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            string correlationId = Guid.NewGuid().ToString();

            context.Items.Add(CorrelationIdHeader, correlationId);

            context.Response.OnStarting(() =>
            {
                if (!context.Items.ContainsKey(CorrelationIdHeader))
                {
                    context.Items.Add(CorrelationIdHeader, correlationId);
                }
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
