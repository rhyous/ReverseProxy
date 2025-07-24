namespace ReverseProxy;

/// <summary>
/// The Reverse proxy service settings. Each service is named and has it's configuration.
/// </summary>
public class ReverseProxySettings : Dictionary<string, ProxiedService>
{
}
