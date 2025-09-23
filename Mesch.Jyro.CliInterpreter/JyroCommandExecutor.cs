using Mesch.Jyro.CliInterpreter.Options;
using Microsoft.Extensions.Logging;

namespace Mesch.Jyro.CliInterpreter;
/// <summary>
/// Orchestrates Jyro execution based on CLI/config/environment options.
/// </summary>
public sealed class JyroCommandExecutor
{
    private readonly JyroCommandOptions _options;
    private readonly JyroBuilder _builder;
    private readonly ILogger<JyroCommandExecutor> _logger;

    public JyroCommandExecutor(
        JyroCommandOptions options,
        JyroBuilder builder,
        ILogger<JyroCommandExecutor> logger)
    {
        _options = options;
        _builder = builder;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync()
    {
        if (string.IsNullOrWhiteSpace(_options.InputScriptFile) ||
            string.IsNullOrWhiteSpace(_options.DataJsonFile))
        {
            _logger.LogError("Both input script and data file must be specified.");
            return -1;
        }

        try
        {
            _logger.LogInformation("Starting Jyro execution. Script={Script}, Data={Data}",
                _options.InputScriptFile, _options.DataJsonFile);

            var script = await File.ReadAllTextAsync(_options.InputScriptFile);
            var data = JyroJson.LoadFile(_options.DataJsonFile);

            var result = _builder
                .WithScript(script)
                .WithData(data)
                .WithOptions(new JyroExecutionOptions())
                .WithStandardLibrary()
                .Run();

            if (!result.IsSuccessful)
            {
                foreach (var msg in result.Messages)
                {
                    _logger.LogError("{Message}", msg.ToString());
                }
                return -1;
            }

            var outputJson = result.Data.ToJson(new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            if (!string.IsNullOrWhiteSpace(_options.OutputJsonFile))
            {
                await File.WriteAllTextAsync(_options.OutputJsonFile, outputJson);
                _logger.LogInformation("Output written to {OutputFile}", _options.OutputJsonFile);
            }
            else
            {
                System.Console.WriteLine(outputJson);
            }

            _logger.LogInformation("Jyro execution completed successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Jyro script execution failed.");
            return -1;
        }
    }

}
