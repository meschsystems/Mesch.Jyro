namespace Mesch.Jyro.CodeAnalysis;

/// <summary>
/// Defines the types of insights that can be discovered during code analysis.
/// </summary>
public enum InsightType
{
    /// <summary>
    /// General informational insight about code characteristics.
    /// </summary>
    Information,

    /// <summary>
    /// Suggestion for improving code quality or maintainability.
    /// </summary>
    Suggestion,

    /// <summary>
    /// Observation about potential performance characteristics.
    /// </summary>
    Performance,

    /// <summary>
    /// Insight about code style and conventions.
    /// </summary>
    Style,

    /// <summary>
    /// Insight about code complexity and maintainability.
    /// </summary>
    Maintainability
}