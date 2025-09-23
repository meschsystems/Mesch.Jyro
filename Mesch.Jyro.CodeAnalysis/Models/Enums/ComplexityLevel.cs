namespace Mesch.Jyro.CodeAnalysis;

/// <summary>
/// Defines the complexity levels that can be assigned to code based on various metrics
/// such as cyclomatic complexity, nesting depth, and cognitive complexity.
/// These levels provide a standardized way to categorize code maintainability and readability.
/// </summary>
public enum ComplexityLevel
{
    /// <summary>
    /// Code has low complexity and is easy to understand, test, and maintain.
    /// Typically corresponds to cyclomatic complexity of 1-5.
    /// </summary>
    Low,

    /// <summary>
    /// Code has moderate complexity with some intricate logic but remains manageable.
    /// Typically corresponds to cyclomatic complexity of 6-10.
    /// </summary>
    Moderate,

    /// <summary>
    /// Code has high complexity that may impact readability and maintainability.
    /// Typically corresponds to cyclomatic complexity of 11-15.
    /// Refactoring should be considered to improve code quality.
    /// </summary>
    High,

    /// <summary>
    /// Code has very high complexity that significantly impacts maintainability and should be refactored.
    /// Typically corresponds to cyclomatic complexity of 16 or higher.
    /// This level indicates code that is difficult to test, debug, and modify safely.
    /// </summary>
    VeryHigh
}