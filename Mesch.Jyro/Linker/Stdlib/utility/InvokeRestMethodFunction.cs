using System.Text;
using System.Text.Json;

namespace Mesch.Jyro;

/// <summary>
/// Executes HTTP REST API calls from Jyro scripts.
/// This function must be explicitly enabled via WithRestApi().
/// </summary>
public sealed class InvokeRestMethodFunction : JyroFunctionBase
{
    private static readonly HttpClient SharedHttpClient = new();
    private readonly RestApiOptions _options;
    private int _currentConcurrentRequests;
    private readonly object _concurrencyLock = new();

    public InvokeRestMethodFunction(RestApiOptions options) : base(
        new JyroFunctionSignature(
            "InvokeRestMethod",
            [
                new Parameter("url", ParameterType.String),
                new Parameter("method", ParameterType.String, isOptionalParameter: true),
                new Parameter("headers", ParameterType.Object, isOptionalParameter: true),
                new Parameter("body", ParameterType.Any, isOptionalParameter: true)
            ],
            ParameterType.Object))
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public override JyroValue Execute(
        IReadOnlyList<JyroValue> arguments,
        JyroExecutionContext executionContext)
    {
        // Check execution time limits
        executionContext.Limiter.CheckExecutionTime();

        // Parse arguments
        var url = GetStringArgument(arguments, 0);
        var method = arguments.Count > 1 && !arguments[1].IsNull
            ? GetStringArgument(arguments, 1).ToUpperInvariant()
            : "GET";
        var headers = arguments.Count > 2 && !arguments[2].IsNull
            ? GetObjectArgument(arguments, 2)
            : null;
        var body = arguments.Count > 3 && !arguments[3].IsNull
            ? arguments[3]
            : null;

        // Validate URL
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            throw new JyroRuntimeException($"Invalid URL: '{url}'");
        }

        if (uri.Scheme != "http" && uri.Scheme != "https")
        {
            throw new JyroRuntimeException(
                $"Only HTTP and HTTPS protocols are supported. Got: '{uri.Scheme}'");
        }

        // Security check: URL filtering
        if (!_options.IsUrlAllowed(url))
        {
            throw new JyroRuntimeException(
                $"URL '{url}' is not allowed by the configured security policy");
        }

        // Security check: HTTP method filtering
        if (!_options.IsMethodAllowed(method))
        {
            throw new JyroRuntimeException(
                $"HTTP method '{method}' is not allowed by the configured security policy");
        }

        // Check concurrent request limit
        lock (_concurrencyLock)
        {
            if (_currentConcurrentRequests >= _options.MaxConcurrentRequests)
            {
                throw new JyroRuntimeException(
                    $"Maximum concurrent requests limit ({_options.MaxConcurrentRequests}) reached");
            }
            _currentConcurrentRequests++;
        }

        try
        {
            // Execute the HTTP request synchronously
            return ExecuteRequestSync(
                uri,
                method,
                headers,
                body,
                executionContext);
        }
        finally
        {
            lock (_concurrencyLock)
            {
                _currentConcurrentRequests--;
            }
        }
    }

    private JyroObject ExecuteRequestSync(
        Uri uri,
        string method,
        JyroObject? headers,
        JyroValue? body,
        JyroExecutionContext executionContext)
    {
        try
        {
            // Create request message
            var request = new HttpRequestMessage(new HttpMethod(method), uri);

            // Add headers if provided
            if (headers != null)
            {
                foreach (var kvp in headers)
                {
                    var headerValue = kvp.Value switch
                    {
                        JyroString s => s.Value,
                        JyroNumber n => n.ToString(),
                        JyroBoolean b => b.Value.ToString().ToLowerInvariant(),
                        _ => kvp.Value.ToString()
                    };

                    // Try to add as request header, fallback to content header
                    if (!request.Headers.TryAddWithoutValidation(kvp.Key, headerValue))
                    {
                        // Will be added to content headers later if there's content
                    }
                }
            }

            // Add body if provided
            if (body != null && !body.IsNull)
            {
                string? requestContentType = null;

                // Try to get Content-Type from headers
                if (headers?.TryGet("Content-Type", out var ctValue) == true &&
                    ctValue is JyroString ctString)
                {
                    requestContentType = ctString.Value;
                }

                var bodyContent = SerializeBody(body, ref requestContentType);

                // Check request size limit
                if (bodyContent.Length > _options.MaxRequestBodySize)
                {
                    throw new JyroRuntimeException(
                        $"Request body size ({bodyContent.Length} bytes) exceeds maximum allowed " +
                        $"({_options.MaxRequestBodySize} bytes)");
                }

                request.Content = new StringContent(bodyContent, Encoding.UTF8, requestContentType ?? "application/json");

                // Add custom headers to content if needed
                if (headers != null)
                {
                    foreach (var kvp in headers)
                    {
                        if (kvp.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var headerValue = kvp.Value switch
                        {
                            JyroString s => s.Value,
                            JyroNumber n => n.ToString(),
                            JyroBoolean b => b.Value.ToString().ToLowerInvariant(),
                            _ => kvp.Value.ToString()
                        };

                        request.Content.Headers.TryAddWithoutValidation(kvp.Key, headerValue);
                    }
                }
            }

            // Configure timeout based on remaining execution time and configured timeout
            var timeout = _options.RequestTimeout;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(executionContext.CancellationToken);
            cts.CancelAfter(timeout);

            // Execute request synchronously
            var response = SharedHttpClient.Send(request, cts.Token);

            // Read response
            using var responseStream = response.Content.ReadAsStream(cts.Token);

            // Check response size limit
            if (response.Content.Headers.ContentLength.HasValue &&
                response.Content.Headers.ContentLength.Value > _options.MaxResponseSize)
            {
                throw new JyroRuntimeException(
                    $"Response size ({response.Content.Headers.ContentLength.Value} bytes) exceeds " +
                    $"maximum allowed ({_options.MaxResponseSize} bytes)");
            }

            // Read response with size check
            using var memoryStream = new MemoryStream();
            var buffer = new byte[8192];
            int bytesRead;
            long totalBytesRead = 0;

            while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                totalBytesRead += bytesRead;
                if (totalBytesRead > _options.MaxResponseSize)
                {
                    throw new JyroRuntimeException(
                        $"Response size exceeds maximum allowed ({_options.MaxResponseSize} bytes)");
                }
                memoryStream.Write(buffer, 0, bytesRead);
            }

            var responseBody = Encoding.UTF8.GetString(memoryStream.ToArray());

            // Build result object
            var result = new JyroObject();
            result.SetProperty("statusCode", new JyroNumber((int)response.StatusCode));
            result.SetProperty("statusDescription", new JyroString(response.ReasonPhrase ?? ""));
            result.SetProperty("isSuccessStatusCode", JyroBoolean.FromBoolean(response.IsSuccessStatusCode));

            // Parse response body
            var contentType = response.Content.Headers.ContentType?.MediaType?.ToLowerInvariant();
            JyroValue parsedBody;

            if (contentType?.Contains("json") == true)
            {
                try
                {
                    parsedBody = ParseJsonResponse(responseBody);
                }
                catch
                {
                    // If JSON parsing fails, return as string
                    parsedBody = new JyroString(responseBody);
                }
            }
            else
            {
                parsedBody = new JyroString(responseBody);
            }

            result.SetProperty("content", parsedBody);
            result.SetProperty("contentType", new JyroString(contentType ?? ""));

            // Add response headers
            var responseHeaders = new JyroObject();
            foreach (var header in response.Headers)
            {
                responseHeaders.SetProperty(
                    header.Key,
                    new JyroString(string.Join(", ", header.Value)));
            }
            foreach (var header in response.Content.Headers)
            {
                responseHeaders.SetProperty(
                    header.Key,
                    new JyroString(string.Join(", ", header.Value)));
            }
            result.SetProperty("headers", responseHeaders);

            return result;
        }
        catch (OperationCanceledException)
        {
            throw new JyroRuntimeException(
                $"REST API call to '{uri}' was cancelled or timed out");
        }
        catch (HttpRequestException ex)
        {
            throw new JyroRuntimeException(
                $"REST API call to '{uri}' failed: {ex.Message}");
        }
        catch (JyroRuntimeException)
        {
            throw; // Re-throw Jyro exceptions as-is
        }
        catch (Exception ex)
        {
            throw new JyroRuntimeException(
                $"Unexpected error during REST API call: {ex.Message}");
        }
    }

    private string SerializeBody(JyroValue body, ref string? contentType)
    {
        // If already a string, use as-is
        if (body is JyroString stringBody)
        {
            return stringBody.Value;
        }

        // Otherwise, serialize to JSON
        contentType ??= "application/json";

        if (body is JyroObject obj)
        {
            return SerializeObjectToJson(obj);
        }

        if (body is JyroArray array)
        {
            return SerializeArrayToJson(array);
        }

        if (body is JyroNumber number)
        {
            return number.ToString() ?? "0";
        }

        if (body is JyroBoolean boolean)
        {
            return boolean.Value ? "true" : "false";
        }

        if (body.IsNull)
        {
            return "null";
        }

        return body.ToString() ?? string.Empty;
    }

    private string SerializeObjectToJson(JyroObject obj)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var kvp in obj)
        {
            dict[kvp.Key] = ConvertToJsonValue(kvp.Value);
        }
        return JsonSerializer.Serialize(dict);
    }

    private string SerializeArrayToJson(JyroArray array)
    {
        var list = new List<object?>();
        foreach (var item in array)
        {
            list.Add(ConvertToJsonValue(item));
        }
        return JsonSerializer.Serialize(list);
    }

    private object? ConvertToJsonValue(JyroValue value)
    {
        return value switch
        {
            JyroString s => s.Value,
            JyroNumber n => n.Value,
            JyroBoolean b => b.Value,
            JyroNull => null,
            JyroObject obj => obj.ToDictionary(
                kvp => kvp.Key,
                kvp => ConvertToJsonValue(kvp.Value)),
            JyroArray arr => arr.Select(ConvertToJsonValue).ToList(),
            _ => value.ToString()
        };
    }

    private JyroValue ParseJsonResponse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return ConvertJsonElement(document.RootElement);
    }

    private JyroValue ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => ConvertJsonObject(element),
            JsonValueKind.Array => ConvertJsonArray(element),
            JsonValueKind.String => new JyroString(element.GetString() ?? ""),
            JsonValueKind.Number => new JyroNumber(element.GetDouble()),
            JsonValueKind.True => JyroBoolean.True,
            JsonValueKind.False => JyroBoolean.False,
            JsonValueKind.Null => JyroNull.Instance,
            _ => JyroNull.Instance
        };
    }

    private JyroObject ConvertJsonObject(JsonElement element)
    {
        var obj = new JyroObject();
        foreach (var property in element.EnumerateObject())
        {
            obj.SetProperty(property.Name, ConvertJsonElement(property.Value));
        }
        return obj;
    }

    private JyroArray ConvertJsonArray(JsonElement element)
    {
        var array = new JyroArray();
        foreach (var item in element.EnumerateArray())
        {
            array.Add(ConvertJsonElement(item));
        }
        return array;
    }
}
