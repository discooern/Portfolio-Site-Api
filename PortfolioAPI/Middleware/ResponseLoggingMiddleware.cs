using PortfolioAPI.Models;

namespace PortfolioAPI.Middleware
{
    public class ResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ResponseLoggingMiddleware> _logger;
        private readonly PortfolioDbContext _dbContext;

        private const int MaxBodyLogLength = 1024 * 2; // 2 KB

        public ResponseLoggingMiddleware(RequestDelegate next,
            ILogger<ResponseLoggingMiddleware> logger,
            PortfolioDbContext dbContext)
        {
            _next = next;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (LogExcludeList.ExcludedPaths.Contains<string>(context.Request.Path, StringComparer.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var originalBodyStream = context.Response.Body;
            await using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            var sw = System.Diagnostics.Stopwatch.StartNew();

            await _next(context);

            sw.Stop();

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            string bodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();

            int originalBodyLength = bodyText.Length;

            if (bodyText.Length > MaxBodyLogLength)
            {
                bodyText = bodyText.Substring(0, MaxBodyLogLength)
                    + $"... truncated. Original length: {originalBodyLength}";
            }

            context.Response.Body.Seek(0, SeekOrigin.Begin);

            string logMessage = $"Response: " +
                $"\nMethod: {context.Request.Method} " +
                $"\nPath: {context.Request.Path} " +
                $"\nStatusCode: {context.Response.StatusCode} " +
                $"\nBody: {bodyText} " +
                $"\nElapsed: {sw.ElapsedMilliseconds} ms.";

            string logLevel = string.Empty;

            switch (context.Response.StatusCode)
            {
                case >= 200 and < 300:
                    _logger.LogInformation(logMessage);
                    logLevel = "INFO";
                    break;
                case >= 400 and < 500:
                    _logger.LogWarning(logMessage);
                    logLevel = "WARN";
                    break;
                case >= 500:
                    _logger.LogError(logMessage);
                    logLevel = "ERROR";
                    break;
                default:
                    break;
            }

            await responseBody.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;

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
                Direction = "Response",
                Level = logLevel,
                Method = context.Request.Method,
                Path = context.Request.Path,
                QueryParameters = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : "",
                StatusCode = context.Response.StatusCode,
                Body = bodyText,
                BodySize = originalBodyLength,
                ElapsedMilliseconds = sw.ElapsedMilliseconds,
                Timestamp = DateTime.UtcNow
            };

            try
            {
                await _dbContext.EventLogs.AddAsync(eventLog);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to log response to database: {e.Message}");
            }

            //await _next(context);
        }
    }
}
