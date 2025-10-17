using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace Mesch.Jyro.Tests;

/// <summary>
/// Helper utilities for Jyro tests
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Executes a Jyro script and returns the result
    /// </summary>
    public static JyroExecutionResult Execute(
        string script,
        JyroValue? data = null,
        ITestOutputHelper? output = null,
        bool withRestApi = false,
        RestApiOptions? restApiOptions = null)
    {
        data ??= new JyroObject();

        var builder = JyroBuilder
            .Create(NullLoggerFactory.Instance)
            .WithScript(script)
            .WithData(data)
            .WithStandardLibrary();

        if (withRestApi)
        {
            builder = builder.WithRestApi(restApiOptions);
        }

        var result = builder.Run();

        if (output != null)
        {
            output.WriteLine($"Script executed: {(result.IsSuccessful ? "SUCCESS" : "FAILED")}");
            if (result.Messages.Any())
            {
                output.WriteLine("Messages:");
                foreach (var msg in result.Messages)
                {
                    output.WriteLine($"  [{msg.Severity}] {msg.Code} at {msg.LineNumber}:{msg.ColumnPosition}");
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Executes a script and asserts it succeeds
    /// </summary>
    public static JyroExecutionResult ExecuteSuccessfully(
        string script,
        JyroValue? data = null,
        ITestOutputHelper? output = null,
        bool withRestApi = false,
        RestApiOptions? restApiOptions = null)
    {
        var result = Execute(script, data, output, withRestApi, restApiOptions);

        if (!result.IsSuccessful)
        {
            var errors = string.Join(", ", result.Messages.Select(m => $"{m.Code}"));
            throw new InvalidOperationException($"Script execution failed: {errors}");
        }

        return result;
    }

    /// <summary>
    /// Creates test data from a JSON-like object initialization
    /// </summary>
    public static JyroObject CreateData(Action<JyroObject> initializer)
    {
        var obj = new JyroObject();
        initializer(obj);
        return obj;
    }
}
