namespace ReverseProxy;


/// <summary>Represents configuration settings for a proxied service.</summary>
public record ProxiedService
{
    /// <summary>The original path of the proxied service.</summary>
    /// <remarks>
    /// This is the original path that comes into the proxy, usually from a browser
    /// or other http client.
    /// </remarks>
    public string OriginalPath { get; init; } = "";

    /// <summary>The proxied path of the service.</summary>
    /// <remarks>
    /// Leave this blank or not set to clear the path. This is useful when the proxied service
    /// is based on a sub-path but the sub-path should not be included in the proxied URL.
    /// </remarks>
    public string ProxiedPath { get; init; } = "";

    /// <summary>Additional headers to add to the proxied <see cref="HttpRequest"/>.</summary>
    /// <remarks>This is a great place to put X-Forwarded-Host and X-Forwarded-Proto.</remarks>
    public Dictionary<string, string> AdditionalHeaders { get; init; } = [];

    /// <summary>Environment-specific urls to proxy to.</summary>
    public Dictionary<string, string> EnvironmentUrls { get; init; } = [];

    /// <summary>The log level for the proxied service.</summary>
    public string LogLevel { get; init; } = "Information";
};