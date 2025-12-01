using PortfolioAPI.Models;
using System.Text;

namespace PortfolioAPI.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly PortfolioDbContext _dbContext;

        private const int MaxBodyLogLength = 1024 * 2; // 2 KB

        public RequestLoggingMiddleware(RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger,
            PortfolioDbContext dbContext)
        {
            _next = next;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip logging for EventLog endpoint.
            if (context.Request.Path.StartsWithSegments("/eventlogs", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            context.Request.EnableBuffering();

            string requestBody = "";
            int originalBodyLength = 0;
            if (context.Request.ContentLength > 0 &&
                context.Request.Body.CanSeek)
            {
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                originalBodyLength = requestBody.Length;

                if (requestBody.Length > MaxBodyLogLength)
                {
                    requestBody = requestBody.Substring(0, MaxBodyLogLength)
                        + $"... truncated. Original length: {originalBodyLength}";
                }

                context.Request.Body.Position = 0;
            }
            string requestParameters = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : "";

            _logger.LogInformation($"Request: " +
                $"\nMethod: {context.Request.Method} " +
                $"\nPath: {context.Request.Path} " +
                $"\nQuery Parameters: {requestParameters} " +
                $"\nBody: {requestBody}");

            Guid correlationGuid = new Guid();

            if (context.Items.TryGetValue("X-Correlation-ID", out object? CorrelationId))
            {
                string correlationIdString = string.Empty;
                if (CorrelationId != null)
                {
                    correlationIdString = CorrelationId.ToString() ?? string.Empty;
                }
                Guid.TryParse(correlationIdString, out correlationGuid);
            }

            // Log to database.
            var eventLog = new EventLog
            {
                CorrelationId = correlationGuid,
                Direction = "Request",
                Level = "INFO",
                Method = context.Request.Method,
                Path = context.Request.Path,
                QueryParameters = requestParameters,
                Body = requestBody,
                BodySize = originalBodyLength,
                Timestamp = DateTime.UtcNow
            };

            try
            {
                await _dbContext.EventLogs.AddAsync(eventLog);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to log request to database: {e.Message}");
            }

            await _next(context);
        }
    }
}
