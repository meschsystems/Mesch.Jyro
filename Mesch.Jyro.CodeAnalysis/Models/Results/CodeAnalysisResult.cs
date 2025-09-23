namespace Mesch.Jyro.CodeAnalysis;

/// <summary>
/// Contains the complete results of static code analysis including metrics,
/// patterns, and insights discovered during AST examination.
/// </summary>
public sealed class CodeAnalysisResult
{
    /// <summary>
    /// Initializes a new instance with the specified analysis outcomes.
    /// </summary>
    public CodeAnalysisResult(
        CodeMetrics metrics,
        IReadOnlyList<CodeInsight> insights,
        TimeSpan analysisTime,
        DateTimeOffset analyzedAt)
    {
        Metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        Insights = insights ?? [];
        AnalysisTime = analysisTime;
        AnalyzedAt = analyzedAt;
    }

    /// <summary>
    /// Gets the quantitative metrics calculated during analysis.
    /// </summary>
    public CodeMetrics Metrics { get; }

    /// <summary>
    /// Gets the collection of insights and recommendations.
    /// </summary>
    public IReadOnlyList<CodeInsight> Insights { get; }

    /// <summary>
    /// Gets the time taken to perform the analysis.
    /// </summary>
    public TimeSpan AnalysisTime { get; }

    /// <summary>
    /// Gets the timestamp when the analysis was performed.
    /// </summary>
    public DateTimeOffset AnalyzedAt { get; }
}