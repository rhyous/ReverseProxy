namespace ReverseProxy
{
    /// <summary>Extensions for <see cref="HttpRequest"/> to log diagnostics information.</summary>
    public static class HttpRequestExtensions
    {
        public static void Log(this HttpRequest request, HttpResponse response, string logLevel)
        {
            var logMsg = "";

            Console.WriteLine(logMsg);
            if (logLevel == "Information")
            {
                logMsg += $"Incoming request at: '{request.Path}'"
                        + Environment.NewLine
                        + $"\tResponse Status Code: {response.StatusCode}";
            }
            if (logLevel == "Debug")
            {
                logMsg += $"\tTimestamp: {DateTime.Now}"
                        + Environment.NewLine
                        + "\tHeaders: {separator}{string.Join(separator, request.Headers.Where(header => header.Key != \"Cookie\").Select(header => $\"{header.Key}: {header.Value}\"))}\r\n"
                        + Environment.NewLine
                        + "Cookies: {separator}{string.Join(separator, request.Cookies.OrderBy(cookie => cookie.Key).Select(cookie => $\"{cookie.Key}: {cookie.Value}\"))}";
            }
            Console.WriteLine(logMsg);
        }
    }
}
