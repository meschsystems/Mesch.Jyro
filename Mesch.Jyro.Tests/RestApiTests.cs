using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace Mesch.Jyro.Tests;

/// <summary>
/// Tests for the InvokeRestMethod function and REST API functionality.
/// These tests use real HTTP endpoints (jsonplaceholder.typicode.com) for integration testing.
/// </summary>
public class RestApiTests
{
    private readonly ITestOutputHelper _output;

    public RestApiTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region Basic GET Requests

    [Fact]
    public void InvokeRestMethod_SimpleGetRequest_ReturnsSuccessResponse()
    {
        // In C# verbatim strings (@""), double quotes inside Jyro strings must be escaped as ""
        var script = @"
            var response = InvokeRestMethod(""https://jsonplaceholder.typicode.com/posts/1"", ""GET"")
            Data.statusCode = response.statusCode
            Data.isSuccess = response.isSuccessStatusCode
            Data.title = response.content.title
        ";

        var result = ExecuteWithRestApi(script);

        var data = (JyroObject)result.Data;
        Assert.Equal(200.0, ((JyroNumber)data.GetProperty("statusCode")).Value);
        Assert.True(((JyroBoolean)data.GetProperty("isSuccess")).Value);

        // Verify content was parsed
        var title = data.GetProperty("title");
        Assert.IsType<JyroString>(title);
        Assert.False(string.IsNullOrEmpty(((JyroString)title).Value));
    }

    [Fact]
    public void InvokeRestMethod_GetWithDefaultMethod_UsesGet()
    {
        var script = @"
            var response = InvokeRestMethod(""https://jsonplaceholder.typicode.com/users/1"")
            Data.statusCode = response.statusCode
            Data.hasContent = response.content != null
        ";

        var result = ExecuteWithRestApi(script);

        var data = (JyroObject)result.Data;
        Assert.Equal(200.0, ((JyroNumber)data.GetProperty("statusCode")).Value);
        var hasContent = data.GetProperty("hasContent");
        // Check if it's not null (may be JyroBoolean or other type)
        Assert.False(hasContent.IsNull);
    }

    [Fact]
    public void InvokeRestMethod_GetReturnsArray_ParsesJsonArray()
    {
        var script = @"
            var response = InvokeRestMethod(""https://jsonplaceholder.typicode.com/posts?userId=1"")
            Data.statusCode = response.statusCode
            Data.content = response.content
        ";

        var result = ExecuteWithRestApi(script);

        var data = (JyroObject)result.Data;
        Assert.Equal(200.0, ((JyroNumber)data.GetProperty("statusCode")).Value);

        // Verify content is an array
        var content = data.GetProperty("content");
        Assert.IsType<JyroArray>(content);
        var array = (JyroArray)content;
        Assert.True(array.Length > 0);
    }

    #endregion

    #region POST Requests

    [Fact]
    public void InvokeRestMethod_PostWithJsonBody_CreatesResource()
    {
        var script = @"
            var newPost = {
                ""title"": ""Test Post"",
                ""body"": ""This is a test post from Jyro"",
                ""userId"": 1
            }

            var headers = {
                ""Content-Type"": ""application/json""
            }

            var response = InvokeRestMethod(
                ""https://jsonplaceholder.typicode.com/posts"",
                ""POST"",
                headers,
                newPost
            )

            Data.statusCode = response.statusCode
            Data.isSuccess = response.isSuccessStatusCode
            Data.createdTitle = response.content.title
        ";

        var result = ExecuteWithRestApi(script);

        var data = (JyroObject)result.Data;
        Assert.Equal(201.0, ((JyroNumber)data.GetProperty("statusCode")).Value);
        Assert.True(((JyroBoolean)data.GetProperty("isSuccess")).Value);
        Assert.Equal("Test Post", ((JyroString)data.GetProperty("createdTitle")).Value);
    }

    #endregion

    #region Error Handling

    [Fact]
    public void InvokeRestMethod_NotFoundError_Returns404()
    {
        var script = @"
            var response = InvokeRestMethod(""https://jsonplaceholder.typicode.com/posts/999999"")
            Data.statusCode = response.statusCode
            Data.isSuccess = response.isSuccessStatusCode
        ";

        var result = ExecuteWithRestApi(script);

        var data = (JyroObject)result.Data;
        Assert.Equal(404.0, ((JyroNumber)data.GetProperty("statusCode")).Value);
        Assert.False(((JyroBoolean)data.GetProperty("isSuccess")).Value);
    }

    [Fact]
    public void InvokeRestMethod_InvalidUrl_ThrowsException()
    {
        var script = @"
            var response = InvokeRestMethod(""not-a-valid-url"")
            Data.result = response
        ";

        var result = TestHelpers.Execute(script, new JyroObject(), _output, withRestApi: true);

        Assert.False(result.IsSuccessful);
        Assert.Contains(result.Messages, m =>
            m.Severity == MessageSeverity.Error);
    }

    #endregion

    #region Security - URL Filtering

    [Fact]
    public void InvokeRestMethod_UrlNotInAllowList_ThrowsException()
    {
        var script = @"
            var response = InvokeRestMethod(""https://jsonplaceholder.typicode.com/posts/1"")
            Data.result = response
        ";

        var options = new RestApiOptions
        {
            AllowedUrlPatterns = new List<Regex>
            {
                new Regex(@"^https://api\.example\.com/", RegexOptions.IgnoreCase | RegexOptions.Compiled)
            }
        };

        var result = ExecuteWithRestApi(script, options, expectSuccess: false);

        Assert.False(result.IsSuccessful);
        Assert.Contains(result.Messages, m => m.Severity == MessageSeverity.Error);
    }

    [Fact]
    public void InvokeRestMethod_UrlInAllowList_Succeeds()
    {
        var script = @"
            var response = InvokeRestMethod(""https://jsonplaceholder.typicode.com/posts/1"")
            Data.statusCode = response.statusCode
        ";

        var options = new RestApiOptions
        {
            AllowedUrlPatterns = new List<Regex>
            {
                new Regex(@"^https://jsonplaceholder\.typicode\.com/",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled)
            }
        };

        var result = ExecuteWithRestApi(script, options);

        var data = (JyroObject)result.Data;
        Assert.Equal(200.0, ((JyroNumber)data.GetProperty("statusCode")).Value);
    }

    [Fact]
    public void InvokeRestMethod_DenyListTakesPrecedenceOverAllowList()
    {
        var script = @"
            var response = InvokeRestMethod(""https://jsonplaceholder.typicode.com/posts/1"")
            Data.result = response
        ";

        var options = new RestApiOptions
        {
            // Allow all jsonplaceholder URLs
            AllowedUrlPatterns = new List<Regex>
            {
                new Regex(@"^https://jsonplaceholder\.typicode\.com/",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled)
            },
            // But deny /posts/ URLs specifically - deny should win
            DeniedUrlPatterns = new List<Regex>
            {
                new Regex(@"/posts/", RegexOptions.IgnoreCase | RegexOptions.Compiled)
            }
        };

        var result = ExecuteWithRestApi(script, options, expectSuccess: false);

        Assert.False(result.IsSuccessful);
        Assert.Contains(result.Messages, m =>
            m.Severity == MessageSeverity.Error &&
            (m.ToString() ?? "").Contains("not allowed"));
    }

    #endregion

    #region Security - HTTP Method Filtering

    [Fact]
    public void InvokeRestMethod_DeniedHttpMethod_ThrowsException()
    {
        var script = @"
            var response = InvokeRestMethod(""https://jsonplaceholder.typicode.com/posts/1"", ""DELETE"")
            Data.result = response
        ";

        var options = new RestApiOptions
        {
            // Only allow GET and POST methods
            AllowedHttpMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "GET", "POST"
            }
        };

        var result = ExecuteWithRestApi(script, options, expectSuccess: false);

        Assert.False(result.IsSuccessful);
        Assert.Contains(result.Messages, m =>
            m.Severity == MessageSeverity.Error &&
            (m.ToString() ?? "").Contains("method") &&
            (m.ToString() ?? "").Contains("not allowed"));
    }

    #endregion

    #region Helper Methods

    private JyroExecutionResult ExecuteWithRestApi(
        string script,
        RestApiOptions? options = null,
        bool expectSuccess = true)
    {
        options ??= new RestApiOptions
        {
            // Use generous defaults for testing
            MaxRequestBodySize = 1_048_576,
            MaxResponseSize = 10_485_760,
            RequestTimeout = TimeSpan.FromSeconds(30)
        };

        var data = new JyroObject();

        var result = JyroBuilder
            .Create(NullLoggerFactory.Instance)
            .WithScript(script)
            .WithData(data)
            .WithRestApi(options)
            .Run();

        if (_output != null)
        {
            _output.WriteLine($"Script executed: {(result.IsSuccessful ? "SUCCESS" : "FAILED")}");
            if (result.Messages.Any())
            {
                _output.WriteLine("Messages:");
                foreach (var msg in result.Messages)
                {
                    _output.WriteLine($"  [{msg.Severity}] {msg.ToString()}");
                }
            }
        }

        if (expectSuccess && !result.IsSuccessful)
        {
            var errors = string.Join(", ", result.Messages.Select(m => m.ToString()));
            throw new InvalidOperationException($"Script execution failed: {errors}");
        }

        return result;
    }

    #endregion
}
