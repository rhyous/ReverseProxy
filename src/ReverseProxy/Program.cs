using ReverseProxy;

// 1. Build the App
var app = WebApplication.CreateBuilder(args).Build();
app.UseHttpsRedirection();

// 2. Get the settings for the proxied services
var reverseProxySettings = app.Configuration.GetSection(nameof(ReverseProxySettings))
                                            .Get<ReverseProxySettings>()!;

// 3. Create a reverse proxy service for each proxied service
foreach (var (serviceName, proxiedService) in reverseProxySettings)
{
    var reverseProxyService = new ReverseProxyService(serviceName, proxiedService);

    app.Map($"{proxiedService.OriginalPath}/{{*relativePath}}", async context =>
        {
            var responseMessage = await reverseProxyService.ProxyHttpRequest(context.Request);
            await context.Response.CopyFromAsync(responseMessage);
        });
}

// 4. Run the app
await app.RunAsync();