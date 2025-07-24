namespace ReverseProxy;

/// <summary>Exception thrown when an environment is not specified for a service.</summary>
public class EnvironmentNotSpecifiedException : Exception
{
    /// <summary>The constructor.</summary>
    /// <param name="service">The service that is missing the environment configuration.</param>
    /// <remarks>Put this in an environment variable. For coding, it goes in launchSettings.json.</remarks>
    public EnvironmentNotSpecifiedException(string service) : base($"Environment for {service} not specified.")
    {
    }
}