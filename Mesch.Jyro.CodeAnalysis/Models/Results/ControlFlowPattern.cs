namespace Mesch.Jyro.CodeAnalysis;

/// <summary>
/// Represents a control flow pattern identified during code analysis, including its frequency of occurrence
/// and descriptive information about its purpose and characteristics.
/// </summary>
/// <param name="PatternType">The type of control flow pattern (e.g., "if-then", "while", "switch").</param>
/// <param name="Frequency">The number of times this pattern occurs in the analyzed code.</param>
/// <param name="Description">A human-readable description of the pattern's purpose or characteristics.</param>
public sealed record ControlFlowPattern(
    string PatternType,
    int Frequency,
    string Description)
{

}