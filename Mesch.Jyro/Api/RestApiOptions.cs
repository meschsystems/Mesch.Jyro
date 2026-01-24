using System.Text.RegularExpressions;

namespace Mesch.Jyro;

/// <summary>
/// Configuration options for REST API functionality in Jyro scripts.
/// Provides security controls including URL filtering, rate limiting, and size restrictions.
/// </summary>
public sealed class RestApiOptions
{
    /// <summary>
    /// Gets or sets the list of regex patterns for URLs that are explicitly allowed.
    /// If this list is not empty, only URLs matching at least one pattern will be permitted.
    /// </summary>
    public List<Regex> AllowedUrlPatterns { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of regex patterns for URLs that are explicitly denied.
    /// URLs matching any deny pattern will be blocked, even if they match an allow pattern.
    /// Deny rules take precedence over allow rules.
    /// </summary>
    public List<Regex> DeniedUrlPatterns { get; set; } = new();

    /// <summary>
    /// Gets or sets the maximum size in bytes that can be sent in a single request body.
    /// Default is 1MB (1,048,576 bytes).
    /// </summary>
    public long MaxRequestBodySize { get; set; } = 1_048_576; // 1MB

    /// <summary>
    /// Gets or sets the maximum size in bytes that can be received in a single response.
    /// Default is 10MB (10,485,760 bytes).
    /// </summary>
    public long MaxResponseSize { get; set; } = 10_485_760; // 10MB

    /// <summary>
    /// Gets or sets the maximum number of concurrent REST API calls that can be in flight.
    /// Default is 5.
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 5;

    /// <summary>
    /// Gets or sets the timeout for individual HTTP requests.
    /// Default is 30 seconds.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets whether to follow HTTP redirects automatically.
    /// Default is true.
    /// </summary>
    public bool AllowRedirects { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of redirects to follow.
    /// Default is 10.
    /// </summary>
    public int MaxRedirects { get; set; } = 10;

    /// <summary>
    /// Gets or sets the maximum delay in milliseconds that scripts can request between API calls.
    /// Default is 10000ms (10 seconds). Set to 0 to disable the rate limiting parameter.
    /// </summary>
    public int MaxRequestDelayMs { get; set; } = 10_000; // 10 seconds

    /// <summary>
    /// Gets or sets the list of allowed HTTP methods.
    /// If empty, all methods are allowed. Default allows GET, POST, PUT, PATCH, DELETE.
    /// </summary>
    public HashSet<string> AllowedHttpMethods { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET", "POST", "PUT", "PATCH", "DELETE"
    };

    /// <summary>
    /// Validates a URL against the configured allow and deny patterns.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>True if the URL is permitted, false otherwise.</returns>
    public bool IsUrlAllowed(string url)
    {
        // Check deny list first (deny takes precedence)
        foreach (var denyPattern in DeniedUrlPatterns)
        {
            if (denyPattern.IsMatch(url))
            {
                return false;
            }
        }

        // If allow list is empty, allow all (except those denied above)
        if (AllowedUrlPatterns.Count == 0)
        {
            return true;
        }

        // Check if URL matches at least one allow pattern
        foreach (var allowPattern in AllowedUrlPatterns)
        {
            if (allowPattern.IsMatch(url))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validates an HTTP method against the configured allowed methods.
    /// </summary>
    /// <param name="method">The HTTP method to validate.</param>
    /// <returns>True if the method is permitted, false otherwise.</returns>
    public bool IsMethodAllowed(string method)
    {
        if (AllowedHttpMethods.Count == 0)
        {
            return true;
        }

        return AllowedHttpMethods.Contains(method);
    }

    /// <summary>
    /// Creates a default RestApiOptions instance with safe defaults.
    /// </summary>
    public static RestApiOptions CreateDefault()
    {
        return new RestApiOptions();
    }

    /// <summary>
    /// Creates a RestApiOptions instance configured for local development.
    /// Allows localhost and local network addresses.
    /// </summary>
    public static RestApiOptions CreateForLocalDevelopment()
    {
        return new RestApiOptions
        {
            AllowedUrlPatterns = new List<Regex>
            {
                new Regex(@"^https?://localhost(:\d+)?/", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                new Regex(@"^https?://127\.0\.0\.1(:\d+)?/", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                new Regex(@"^https?://\[::1\](:\d+)?/", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                new Regex(@"^https?://192\.168\.\d+\.\d+(:\d+)?/", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                new Regex(@"^https?://10\.\d+\.\d+\.\d+(:\d+)?/", RegexOptions.IgnoreCase | RegexOptions.Compiled)
            }
        };
    }

    /// <summary>
    /// Creates a RestApiOptions instance that blocks private/internal network access.
    /// Useful for production environments to prevent SSRF attacks.
    /// </summary>
    public static RestApiOptions CreateWithPrivateNetworkBlocking()
    {
        return new RestApiOptions
        {
            DeniedUrlPatterns = new List<Regex>
            {
                // Localhost
                new Regex(@"^https?://localhost(:\d+)?/", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                new Regex(@"^https?://127\.\d+\.\d+\.\d+(:\d+)?/", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                new Regex(@"^https?://\[::1\](:\d+)?/", RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Private networks (RFC 1918)
                new Regex(@"^https?://10\.\d+\.\d+\.\d+(:\d+)?/", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                new Regex(@"^https?://172\.(1[6-9]|2\d|3[01])\.\d+\.\d+(:\d+)?/", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                new Regex(@"^https?://192\.168\.\d+\.\d+(:\d+)?/", RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Link-local
                new Regex(@"^https?://169\.254\.\d+\.\d+(:\d+)?/", RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Metadata services (cloud providers)
                new Regex(@"^https?://169\.254\.169\.254/", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                new Regex(@"^https?://metadata\.google\.internal/", RegexOptions.IgnoreCase | RegexOptions.Compiled)
            }
        };
    }
}
