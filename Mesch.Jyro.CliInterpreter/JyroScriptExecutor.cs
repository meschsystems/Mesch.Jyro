using System.Text.Json;
using Mesch.Jyro.CliInterpreter.Options;
using Mesch.Jyro.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Mesch.Jyro.CliInterpreter;

/// <summary>
/// Executes Jyro scripts with optional code analysis based on command-line options.
/// </summary>
internal sealed class JyroScriptExecutor : IJyroScriptExecutor
{
    private readonly JyroBuilder _builder;
    private readonly ILogger<JyroScriptExecutor> _logger;
    private readonly JyroCommandOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="JyroScriptExecutor"/> class.
    /// </summary>
    /// <param name="builder">The Jyro builder instance.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The command options.</param>
    public JyroScriptExecutor(
        JyroBuilder builder,
        ILogger<JyroScriptExecutor> logger,
        JyroCommandOptions options)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Executes the Jyro script according to the configured options.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when script execution fails.</exception>
    public async Task ExecuteAsync()
    {
        _options.Validate();

        _logger.LogInformation("Starting Jyro execution. Script={Script}, Data={Data}, Output={Output}, Analysis={Analysis}",
            _options.InputScriptFile,
            _options.DataJsonFile ?? "(empty object)",
            _options.OutputJsonFile ?? "(stdout)",
            _options.EnableCodeAnalysis ? "enabled" : "disabled");

        // Validate the input script file
        if (!File.Exists(_options.InputScriptFile))
        {
            throw new FileNotFoundException("Input script file not found", _options.InputScriptFile);
        }

        var script = await File.ReadAllTextAsync(_options.InputScriptFile);
        var data = await LoadDataAsync();

        // Configure the builder with script and data
        _builder
            .WithScript(script)
            .WithData(data)
            .WithOptions(CreateExecutionOptions())
            .WithStandardLibrary();

        if (_options.EnableCodeAnalysis)
        {
            await ExecuteWithCodeAnalysisAsync();
        }
        else
        {
            await ExecuteScriptAsync();
        }
    }

    /// <summary>
    /// Loads the JSON data file or returns an empty object if no file is specified.
    /// </summary>
    /// <returns>A JyroValue representing the loaded data.</returns>
    private async Task<JyroValue> LoadDataAsync()
    {
        if (string.IsNullOrWhiteSpace(_options.DataJsonFile))
        {
            return JyroValue.FromJson("{}");
        }

        var dataJson = await File.ReadAllTextAsync(_options.DataJsonFile);
        return JyroValue.FromJson(dataJson);
    }

    /// <summary>
    /// Executes the script with code analysis enabled.
    /// </summary>
    private async Task ExecuteWithCodeAnalysisAsync()
    {
        try
        {
            var analysisOptions = CreateAnalysisOptions();
            var analysisBuilder = _builder.WithCodeAnalysis(analysisOptions);

            if (_options.AnalysisOnlyMode)
            {
                _logger.LogInformation("Performing code analysis only...");
                var analysisResult = analysisBuilder.AnalyzeOnly();
                await OutputAnalysisResultsAsync(analysisResult);
                return;
            }

            _logger.LogInformation("Performing code analysis and execution...");
            var executionWithAnalysisResult = analysisBuilder.Run();

            if (executionWithAnalysisResult.AnalysisResult != null)
            {
                await OutputAnalysisResultsAsync(executionWithAnalysisResult.AnalysisResult);
            }

            await OutputExecutionResultsAsync(executionWithAnalysisResult.ExecutionResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Code analysis execution failed");
            throw;
        }
    }

    /// <summary>
    /// Executes the Jyro script without analysis.
    /// </summary>
    private async Task ExecuteScriptAsync()
    {
        _logger.LogInformation("Executing script without analysis...");
        var result = _builder.Run();
        await OutputExecutionResultsAsync(result);
    }

    /// <summary>
    /// Creates execution options with appropriate limits.
    /// </summary>
    /// <returns>Configured execution options.</returns>
    private static JyroExecutionOptions CreateExecutionOptions()
    {
        return new JyroExecutionOptions
        {
            MaxExecutionTime = TimeSpan.FromSeconds(30),
            MaxStatements = 1_000_000,
            MaxLoops = 10_000_000,
            MaxStackDepth = 1000,
            MaxCallDepth = 256
        };
    }

    /// <summary>
    /// Creates code analysis options based on command-line settings.
    /// </summary>
    /// <returns>Configured code analysis options.</returns>
    private static CodeAnalysisOptions CreateAnalysisOptions()
    {
        return new CodeAnalysisOptions
        {
            EnableComplexityMetrics = true,
            EnablePatternAnalysis = false, // Disabled since we removed pattern analysis
            EnableStyleInsights = false,   // Disabled since we removed pattern analysis
            EnablePerformanceInsights = true,
            EnableMaintainabilityMetrics = true
        };
    }

    /// <summary>
    /// Outputs the code analysis results to console and/or file.
    /// </summary>
    /// <param name="analysisResult">The analysis result to output.</param>
    private async Task OutputAnalysisResultsAsync(CodeAnalysisResult analysisResult)
    {
        var report = GenerateAnalysisReport(analysisResult);

        if (_options.ShowAnalysisOnConsole)
        {
            System.Console.WriteLine("=== CODE ANALYSIS REPORT ===");
            System.Console.WriteLine(report);
            System.Console.WriteLine();
        }

        if (!string.IsNullOrWhiteSpace(_options.AnalysisOutputFile))
        {
            await File.WriteAllTextAsync(_options.AnalysisOutputFile, report);
            _logger.LogInformation("Analysis report written to {AnalysisFile}", _options.AnalysisOutputFile);
        }

        _logger.LogInformation("Code analysis completed");
    }

    /// <summary>
    /// Generates a formatted analysis report.
    /// </summary>
    /// <param name="analysisResult">The analysis result.</param>
    /// <returns>A formatted report string.</returns>
    private static string GenerateAnalysisReport(CodeAnalysisResult analysisResult)
    {
        var report = new System.Text.StringBuilder();

        report.AppendLine($"Analysis completed at: {analysisResult.AnalyzedAt:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine($"Analysis duration: {analysisResult.AnalysisTime.TotalMilliseconds:F0}ms");
        report.AppendLine();

        // Metrics section
        var metrics = analysisResult.Metrics;
        report.AppendLine("Code Metrics:");
        report.AppendLine($"  Total Statements: {metrics.TotalStatements}");
        report.AppendLine($"  Total Expressions: {metrics.TotalExpressions}");
        report.AppendLine($"  Variable Declarations: {metrics.VariableDeclarations}");
        report.AppendLine($"  Function Calls: {metrics.FunctionCalls}");
        report.AppendLine($"  Cyclomatic Complexity: {metrics.CyclomaticComplexity}");
        report.AppendLine($"  Cognitive Complexity: {metrics.CognitiveComplexity}");
        report.AppendLine($"  Max Nesting Depth: {metrics.MaxNestingDepth}");
        report.AppendLine($"  Unique Variables: {metrics.UniqueVariableNames}");
        report.AppendLine();

        // Insights section
        if (analysisResult.Insights.Count > 0)
        {
            report.AppendLine("Analysis Insights:");
            foreach (var insight in analysisResult.Insights)
            {
                report.AppendLine($"  [{insight.Type}] {insight.Title}");
                report.AppendLine($"    {insight.Description}");
                if (!string.IsNullOrEmpty(insight.Recommendation))
                {
                    report.AppendLine($"    Recommendation: {insight.Recommendation}");
                }
                if (insight.Impact.HasValue)
                {
                    report.AppendLine($"    Impact: {insight.Impact.Value:P0}");
                }
                report.AppendLine();
            }
        }
        else
        {
            report.AppendLine("No specific insights identified.");
            report.AppendLine();
        }

        return report.ToString();
    }

    /// <summary>
    /// Outputs the script execution results to console and/or file.
    /// </summary>
    /// <param name="result">The execution result.</param>
    /// <exception cref="InvalidOperationException">Thrown when script execution fails.</exception>
    private async Task OutputExecutionResultsAsync(JyroExecutionResult result)
    {
        if (!result.IsSuccessful)
        {
            foreach (var msg in result.Messages)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(msg.ToString());
                System.Console.ResetColor();
            }
            throw new InvalidOperationException("Jyro script execution failed.");
        }

        var outputJson = JsonSerializer.Serialize(
            result.Data.ToObjectValue(),
            new JsonSerializerOptions { WriteIndented = true });

        if (!string.IsNullOrWhiteSpace(_options.OutputJsonFile))
        {
            await File.WriteAllTextAsync(_options.OutputJsonFile, outputJson);
            _logger.LogInformation("Execution complete. Output written to {OutputFile}", _options.OutputJsonFile);
        }
        else
        {
            System.Console.WriteLine(outputJson);
            _logger.LogInformation("Execution complete. Output written to stdout");
        }
    }
}