using Microsoft.Extensions.Logging;

namespace Mesch.Jyro.CliInterpreter.Options;

/// <summary>
/// Represents the command-line options for the Jyro console application.
/// </summary>
public sealed class JyroCommandOptions
{
    /// <summary>
    /// Gets or sets the path to the Jyro script file to execute.
    /// </summary>
    public string? InputScriptFile { get; set; }

    /// <summary>
    /// Gets or sets the path to the JSON data file that will be provided as the script's Data object.
    /// </summary>
    public string? DataJsonFile { get; set; }

    /// <summary>
    /// Gets or sets the path for the JSON output file. If null, output goes to stdout.
    /// </summary>
    public string? OutputJsonFile { get; set; }

    /// <summary>
    /// Gets or sets the path for logging output. If null, logs go to console only.
    /// </summary>
    public string? LogFile { get; set; }

    /// <summary>
    /// Gets or sets the minimum log level.
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Gets or sets a value indicating whether logging is completely disabled.
    /// </summary>
    public bool NoLogging { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether console logging is enabled.
    /// </summary>
    public bool ConsoleLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether static code analysis is enabled.
    /// </summary>
    public bool EnableCodeAnalysis { get; set; }

    /// <summary>
    /// Gets or sets the output file path for the code analysis report.
    /// </summary>
    public string? AnalysisOutputFile { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether analysis results should be displayed on the console.
    /// </summary>
    public bool ShowAnalysisOnConsole { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to perform analysis only without script execution.
    /// </summary>
    public bool AnalysisOnlyMode { get; set; }

    /// <summary>
    /// Validates the options and applies smart defaults.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required options are missing or invalid.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(InputScriptFile))
        {
            throw new InvalidOperationException("Input script file is required.");
        }

        if (!File.Exists(InputScriptFile))
        {
            throw new InvalidOperationException($"Input script file not found: {InputScriptFile}");
        }

        if (!string.IsNullOrWhiteSpace(DataJsonFile) && !File.Exists(DataJsonFile))
        {
            throw new InvalidOperationException($"Data JSON file not found: {DataJsonFile}");
        }

        // Analysis option validation and smart defaults
        if (AnalysisOnlyMode)
        {
            EnableCodeAnalysis = true;
        }

        if (EnableCodeAnalysis)
        {
            // If analysis is enabled but no output method specified, default to console display
            if (string.IsNullOrWhiteSpace(AnalysisOutputFile) && !ShowAnalysisOnConsole)
            {
                ShowAnalysisOnConsole = true;
            }
        }
    }
}