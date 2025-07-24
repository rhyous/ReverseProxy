using Flurl;
using Flurl.Http;

namespace ReverseProxy;

/// <summary>Service that handles reverse proxying HTTP requests to proxied services.</summary>
public class ReverseProxyService
{
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
            var flurResponse = await proxiedUrl
                .SetQueryParams(request.Query)
                .WithHeaders(request.Headers)
                .WithHeaders(_proxiedService.AdditionalHeaders)
                .WithHeader("X-Forwarded-Path", request.Path)
                .AllowAnyHttpStatus()
                .WithAutoRedirect(false)
                .SendAsync(new HttpMethod(request.Method), new StreamContent(request.Body));
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
