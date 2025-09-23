using System.CommandLine;
using Mesch.Jyro.CliInterpreter.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Mesch.Jyro.CliInterpreter;

/// <summary>
/// Factory for creating the root command with all options and handlers configured.
/// </summary>
internal sealed class JyroCommandFactory
{
    /// <summary>
    /// Creates the root command with all options and handlers configured.
    /// </summary>
    /// <returns>The configured root command.</returns>
    public RootCommand CreateRootCommand()
    {
        var rootCommand = new RootCommand(
            "Jyro - an imperative data manipulation language for secure, sandboxed processing of JSON-like data structures.");

        // Script execution options
        var inputScriptFileOption = new Option<string?>("--input-script-file")
        {
            Description = "Path to the Jyro script to execute",
            Aliases = { "-i" },
            Required = true
        };

        var dataJsonFileOption = new Option<string?>("--data-json-file")
        {
            Description = "JSON file providing the script's Data object (defaults to empty object)",
            Aliases = { "-d" }
        };

        var outputJsonFileOption = new Option<string?>("--output-json-file")
        {
            Description = "Output file for script results (defaults to stdout)",
            Aliases = { "-o" }
        };

        // Logging options
        var logFileOption = new Option<string?>("--log-file")
        {
            Description = "File for logging output (defaults to console only)",
            Aliases = { "-l" }
        };

        var logLevelOption = new Option<LogLevel?>("--log-level")
        {
            Description = "Minimum log level (Trace, Debug, Information, Warning, Error, Critical, None)"
        };

        var noLoggingOption = new Option<bool>("--no-logging")
        {
            Description = "Disable all logging output"
        };

        var consoleLoggingOption = new Option<bool?>("--console-logging")
        {
            Description = "Enable console logging output (true/false). Default true."
        };

        // Code analysis options
        var enableAnalysisOption = new Option<bool>("--analyze")
        {
            Description = "Enable static code analysis of the Jyro script",
            Aliases = { "-a" }
        };

        var analysisOutputFileOption = new Option<string?>("--analysis-output")
        {
            Description = "Output file for code analysis report",
            Aliases = { "--analysis-file" }
        };

        var showAnalysisOnConsoleOption = new Option<bool>("--show-analysis")
        {
            Description = "Display analysis results on console"
        };

        var analysisOnlyModeOption = new Option<bool>("--analyze-only")
        {
            Description = "Perform code analysis without executing the script"
        };

        // Add all options to root command
        rootCommand.Options.Add(inputScriptFileOption);
        rootCommand.Options.Add(dataJsonFileOption);
        rootCommand.Options.Add(outputJsonFileOption);
        rootCommand.Options.Add(logFileOption);
        rootCommand.Options.Add(logLevelOption);
        rootCommand.Options.Add(noLoggingOption);
        rootCommand.Options.Add(consoleLoggingOption);
        rootCommand.Options.Add(enableAnalysisOption);
        rootCommand.Options.Add(analysisOutputFileOption);
        rootCommand.Options.Add(showAnalysisOnConsoleOption);
        rootCommand.Options.Add(analysisOnlyModeOption);

        rootCommand.SetAction(async parseResult =>
        {
            var opts = new JyroCommandOptions
            {
                InputScriptFile = parseResult.GetValue(inputScriptFileOption),
                DataJsonFile = parseResult.GetValue(dataJsonFileOption),
                OutputJsonFile = parseResult.GetValue(outputJsonFileOption),
                LogFile = parseResult.GetValue(logFileOption),
                LogLevel = parseResult.GetValue(logLevelOption) ?? LogLevel.Information,
                NoLogging = parseResult.GetValue(noLoggingOption),
                ConsoleLogging = parseResult.GetValue(consoleLoggingOption) ?? true,
                EnableCodeAnalysis = parseResult.GetValue(enableAnalysisOption),
                AnalysisOutputFile = parseResult.GetValue(analysisOutputFileOption),
                ShowAnalysisOnConsole = parseResult.GetValue(showAnalysisOnConsoleOption),
                AnalysisOnlyMode = parseResult.GetValue(analysisOnlyModeOption)
            };

            // Build host with opts injected
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton(opts);
                    services.AddTransient<JyroBuilder>();
                    services.AddSingleton<IJyroScriptExecutor, JyroScriptExecutor>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    if (opts.NoLogging || opts.LogLevel == LogLevel.None)
                    {
                        logging.SetMinimumLevel(LogLevel.None);
                        return;
                    }
                    if (opts.ConsoleLogging)
                    {
                        logging.AddSimpleConsole(o =>
                        {
                            o.IncludeScopes = false;
                            o.SingleLine = true;
                            o.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                        });
                    }
                    if (!string.IsNullOrWhiteSpace(opts.LogFile))
                    {
                        logging.AddProvider(new SimpleFileLoggerProvider(opts.LogFile));
                    }

                    logging.SetMinimumLevel(opts.LogLevel);
                })
                .Build();

            // Resolve from DI
            var executor = host.Services.GetRequiredService<IJyroScriptExecutor>();
            await executor.ExecuteAsync();
            return 0;
        });

        return rootCommand;
    }
}