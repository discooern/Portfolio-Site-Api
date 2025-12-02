namespace PortfolioAPI
{
    public class LogExcludeList
    {
        public static readonly List<string> ExcludedPaths = new()
        {
            "/scalar/scalar.js",
            "/scalar/scalar.aspnetcore.js",
            "/scalar/v1",
            "/openapi/v1.json",
            "/favicon.ico",
            "/eventlogs"
        };
    }
}
