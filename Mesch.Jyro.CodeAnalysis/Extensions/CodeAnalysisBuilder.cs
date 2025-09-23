using Microsoft.Extensions.Logging;

namespace Mesch.Jyro.CodeAnalysis;

/// <summary>
/// Builder that combines JyroBuilder with code analysis capabilities.
/// This maintains the fluent interface while adding analysis functionality.
/// </summary>
public sealed class CodeAnalysisBuilder
{
    private readonly JyroBuilder _builder;
    private readonly CodeAnalysisOptions _analysisOptions;
    private readonly ILogger<CodeAnalyzer>? _analysisLogger;

    internal CodeAnalysisBuilder(JyroBuilder builder, CodeAnalysisOptions options, ILogger<CodeAnalyzer>? logger = null)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _analysisOptions = options ?? throw new ArgumentNullException(nameof(options));
        _analysisLogger = logger;
    }

    /// <summary>
    /// Configures the analysis logger.
    /// </summary>
    public CodeAnalysisBuilder WithLogger(ILogger<CodeAnalyzer> logger)
    {
        return new CodeAnalysisBuilder(_builder, _analysisOptions, logger);
    }

    /// <summary>
    /// Executes the script with code analysis, returning both execution and analysis results.
    /// </summary>
    public JyroExecutionResultWithAnalysis Run(CancellationToken cancellationToken = default)
    {
        // First, ensure we have parsed statements by calling Parse() if needed
        if (_builder.ParsedStatements == null)
        {
            _builder.Parse();
        }

        // Perform code analysis on the parsed AST
        CodeAnalysisResult? analysisResult = null;
        if (_builder.ParsedStatements != null)
        {
            var analyzer = new CodeAnalyzer(_analysisLogger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<CodeAnalyzer>.Instance);
            analysisResult = analyzer.Analyze(_builder.ParsedStatements, _analysisOptions);
        }

        // Execute the script
        var executionResult = _builder.Run(cancellationToken);

        return new JyroExecutionResultWithAnalysis(executionResult, analysisResult);
    }

    /// <summary>
    /// Performs analysis only without executing the script.
    /// </summary>
    public CodeAnalysisResult AnalyzeOnly()
    {
        // Ensure we have parsed statements
        if (_builder.ParsedStatements == null)
        {
            _builder.Parse();
        }

        if (_builder.ParsedStatements == null)
        {
            throw new InvalidOperationException("Failed to parse script for analysis");
        }

        var analyzer = new CodeAnalyzer(_analysisLogger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<CodeAnalyzer>.Instance);
        return analyzer.Analyze(_builder.ParsedStatements, _analysisOptions);
    }
}