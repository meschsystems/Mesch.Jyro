using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Mesch.Jyro.CodeAnalysis;

/// <summary>
/// Provides comprehensive static analysis capabilities for validated Jyro abstract syntax trees.
/// This analyzer examines code structure, calculates metrics, identifies patterns,
/// and provides insights for code quality improvement.
/// </summary>
public sealed class CodeAnalyzer : ICodeAnalyzer
{
    private readonly ILogger<CodeAnalyzer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeAnalyzer"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public CodeAnalyzer(ILogger<CodeAnalyzer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Analyzes the provided validated abstract syntax tree and returns comprehensive analysis results.
    /// </summary>
    /// <param name="statements">The collection of validated top-level statements to analyze.</param>
    /// <param name="options">Configuration options that control the analysis behavior.</param>
    /// <returns>
    /// A <see cref="CodeAnalysisResult"/> containing all metrics, patterns, and insights
    /// discovered during the static analysis process.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when statements or options are null.</exception>
    public CodeAnalysisResult Analyze(IReadOnlyList<IJyroStatement> statements, CodeAnalysisOptions options)
    {
        ArgumentNullException.ThrowIfNull(statements);
        ArgumentNullException.ThrowIfNull(options);

        var analysisStopwatch = Stopwatch.StartNew();
        var analyzedAtUtc = DateTimeOffset.UtcNow;

        _logger.LogTrace("Starting code analysis for {StatementCount} statements", statements.Count);

        var analysisContext = new AnalysisContext(options);

        try
        {
            PerformMetricsCollection(statements, analysisContext);
            var insights = GenerateInsights(analysisContext, options);

            var codeMetrics = analysisContext.GetMetrics();

            analysisStopwatch.Stop();

            _logger.LogTrace("Code analysis completed in {ElapsedMilliseconds}ms with {InsightCount} insights",
                analysisStopwatch.ElapsedMilliseconds, insights.Count);

            return new CodeAnalysisResult(codeMetrics, insights, analysisStopwatch.Elapsed, analyzedAtUtc);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Code analysis failed for {StatementCount} statements", statements.Count);
            throw;
        }
    }

    /// <summary>
    /// Performs metrics collection on the provided statements.
    /// </summary>
    /// <param name="statements">The statements to analyze for metrics.</param>
    /// <param name="analysisContext">The context to accumulate metrics data.</param>
    private static void PerformMetricsCollection(IReadOnlyList<IJyroStatement> statements, AnalysisContext analysisContext)
    {
        var metricsCollectionVisitor = new MetricsCollectionVisitor(analysisContext);
        foreach (var statement in statements)
        {
            statement.Accept(metricsCollectionVisitor);
        }
    }

    /// <summary>
    /// Generates actionable insights based on collected metrics and patterns.
    /// </summary>
    /// <param name="analysisContext">The context containing collected analysis data.</param>
    /// <param name="analysisOptions">The options controlling insight generation.</param>
    /// <returns>A collection of generated insights.</returns>
    private static IReadOnlyList<CodeInsight> GenerateInsights(AnalysisContext analysisContext, CodeAnalysisOptions analysisOptions)
    {
        var insightGenerator = new InsightGenerator(analysisContext, analysisOptions);
        return insightGenerator.GenerateInsights();
    }
}