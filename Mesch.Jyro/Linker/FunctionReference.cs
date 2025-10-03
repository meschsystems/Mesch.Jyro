namespace Mesch.Jyro;

/// <summary>
/// Represents a reference to a function call within Jyro source code, 
/// capturing the function name, arguments, and source location for 
/// compilation and runtime analysis.
/// </summary>
/// <param name="Name">The name of the function being called.</param>
/// <param name="Arguments">The collection of arguments passed to the function.</param>
/// <param name="LineNumber">The line number in source code where the function call occurs.</param>
/// <param name="ColumnPosition">The column position in source code where the function call begins.</param>
public sealed record FunctionReference(
    string Name,
    IReadOnlyList<JyroValue> Arguments,
    int LineNumber,
    int ColumnPosition);