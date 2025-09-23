namespace Mesch.Jyro.CodeAnalysis;

/// <summary>
/// Represents an insight or recommendation discovered during static code analysis.
/// Insights provide actionable feedback about code quality, maintainability, and best practices.
/// </summary>
public sealed class CodeInsight
{
    /// <summary>
    /// Initializes a new insight with the specified details.
    /// </summary>
    public CodeInsight(
        string category,
        InsightType type,
        string title,
        string description,
        int? lineNumber = null,
        int? columnPosition = null,
        string? recommendation = null,
        double? impact = null)
    {
        Category = category ?? throw new ArgumentNullException(nameof(category));
        Type = type;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
        Recommendation = recommendation;
        Impact = impact;
    }

    /// <summary>
    /// Gets the category of this insight (e.g., "Complexity", "Style", "Performance").
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// Gets the type of insight.
    /// </summary>
    public InsightType Type { get; }

    /// <summary>
    /// Gets the title or summary of this insight.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the detailed description of this insight.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the line number where this insight applies, if specific to a location.
    /// </summary>
    public int? LineNumber { get; }

    /// <summary>
    /// Gets the column position where this insight applies, if specific to a location.
    /// </summary>
    public int? ColumnPosition { get; }

    /// <summary>
    /// Gets an optional recommendation for addressing this insight.
    /// </summary>
    public string? Recommendation { get; }

    /// <summary>
    /// Gets an optional impact score (0.0 to 1.0) indicating the potential benefit of addressing this insight.
    /// </summary>
    public double? Impact { get; }
}