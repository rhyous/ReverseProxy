using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Http;

namespace ReverseProxy;

/// <summary>Service that handles reverse proxying HTTP requests to proxied services.</summary>
public class ReverseProxyService
{
    private static readonly HashSet<string> HopByHopRequestHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Connection",
        "Keep-Alive",
        "Proxy-Authenticate",
        "Proxy-Authorization",
        "TE",
        "Trailers",
        "Transfer-Encoding",
        "Upgrade",
    };

    private readonly string _serviceName;
    private readonly ProxiedService _proxiedService;

    /// <summary>The constructor.</summary>
    /// <param name="serviceName">The name of the service to be proxied. This should match the key in the <see cref="ReverseProxySettings"/> dictionary.</param>
    /// <param name="proxiedService">The configuration settings for the proxied service, including environment URLs and headers.</param>
    public ReverseProxyService(string serviceName, ProxiedService proxiedService)
    {
        _serviceName = serviceName;
        _proxiedService = proxiedService;
    }

    /// <summary>Proxies an HTTP request to the configured proxied service URL.</summary>
    /// <param name="request">The <see cref="HttpRequest"/>.</param>
    /// <returns>An <see cref="HttpResponseMessage"/>.</returns>
    public async Task<HttpResponseMessage> ProxyHttpRequest(HttpRequest request)
    {
        try
        {
            var proxiedUrl = GetProxiedUrl(request.Path);
            var flurlRequest = proxiedUrl
                .SetQueryParams(request.Query)
                .WithHeader("X-Forwarded-Path", request.Path)
                .AllowAnyHttpStatus()
                .WithAutoRedirect(false);

            foreach (var header in request.Headers)
            {
                if (HopByHopRequestHeaders.Contains(header.Key))
                {
                    continue;
                }

                // Flurl stringifies the value, so a string[] would be forwarded as "System.String[]".
                flurlRequest = flurlRequest.WithHeader(header.Key, header.Value.ToString());
            }

            // The original Host is forwarded as-is: upstream builds absolute URLs from it, including the
            // OIDC redirect_uri, which must match the registered one. Keep a client-supplied
            // X-Forwarded-Host (in-network callers use it to select a tenant); otherwise derive it.
            var clientForwardedHost = request.Headers["X-Forwarded-Host"].ToString();
            var forwardedHost = string.IsNullOrEmpty(clientForwardedHost) ? request.Host.Value : clientForwardedHost;
            if (!string.IsNullOrEmpty(forwardedHost))
            {
                flurlRequest = flurlRequest.WithHeader("X-Forwarded-Host", forwardedHost);
            }

            foreach (var (key, value) in _proxiedService.AdditionalHeaders)
            {
                flurlRequest = flurlRequest.WithHeader(key, value);
            }

            HttpContent? body = null;
            if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
            {
                body = new StreamContent(request.Body);
            }

            var flurResponse = await flurlRequest.SendAsync(new HttpMethod(request.Method), body);
            return flurResponse.ResponseMessage;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception encountered: {e.Message}");
            throw;
        }
    }

    private string GetProxiedUrl(string requestUrl)
    {
        var selectedEnv = Environment.GetEnvironmentVariable($"PROXY_{_serviceName.ToUpper()}_ENV") ?? throw new EnvironmentNotSpecifiedException(_serviceName);

        var baseUrl = _proxiedService.EnvironmentUrls[selectedEnv];

        var proxiedUrl = new Uri(new Uri(baseUrl), requestUrl).ToString();
        if (!_proxiedService.OriginalPath.Equals(_proxiedService.ProxiedPath))
        {
            proxiedUrl = proxiedUrl.RegexReplace(_proxiedService.OriginalPath, _proxiedService.ProxiedPath, 1);
        }
        return proxiedUrl;
    }
}
