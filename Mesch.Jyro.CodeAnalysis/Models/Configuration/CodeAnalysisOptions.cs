namespace Mesch.Jyro.CodeAnalysis;

/// <summary>
/// Configuration options for controlling code analysis behavior and determining
/// which analysis metrics and insights should be gathered during static analysis.
/// </summary>
public sealed class CodeAnalysisOptions
{
    /// <summary>
    /// Initializes a new instance with default analysis settings.
    /// </summary>
    public CodeAnalysisOptions()
    {
        EnableComplexityMetrics = true;
        EnablePatternAnalysis = true;
        EnableStyleInsights = true;
        EnablePerformanceInsights = true;
        EnableMaintainabilityMetrics = true;
        ComplexityThresholds = new ComplexityThresholds();
    }

    /// <summary>
    /// Gets or sets whether to calculate complexity metrics including cyclomatic complexity,
    /// nesting depth, and cognitive complexity.
    /// </summary>
    public bool EnableComplexityMetrics { get; set; }

    /// <summary>
    /// Gets or sets whether to analyze code patterns and structural characteristics.
    /// </summary>
    public bool EnablePatternAnalysis { get; set; }

    /// <summary>
    /// Gets or sets whether to provide style and convention insights.
    /// </summary>
    public bool EnableStyleInsights { get; set; }

    /// <summary>
    /// Gets or sets whether to analyze performance characteristics and patterns.
    /// </summary>
    public bool EnablePerformanceInsights { get; set; }

    /// <summary>
    /// Gets or sets whether to calculate maintainability and readability metrics.
    /// </summary>
    public bool EnableMaintainabilityMetrics { get; set; }

    /// <summary>
    /// Gets or sets the complexity thresholds used for categorizing code complexity.
    /// </summary>
    public ComplexityThresholds ComplexityThresholds { get; set; }
}