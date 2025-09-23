namespace Mesch.Jyro;

/// <summary>
/// Represents a fully linked Jyro program that has successfully passed compilation
/// and linking validation, making it ready for execution by the runtime engine.
/// Contains validated statements and resolved function references.
/// </summary>
public sealed class JyroLinkedProgram : ILinkedProgram
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JyroLinkedProgram"/> class
    /// with the specified statements and function collection.
    /// </summary>
    /// <param name="programStatements">
    /// The collection of executable statements that comprise the program body.
    /// Cannot be null.
    /// </param>
    /// <param name="availableFunctions">
    /// The dictionary of functions available to the program during execution.
    /// Cannot be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="programStatements"/> or 
    /// <paramref name="availableFunctions"/> is null.
    /// </exception>
    public JyroLinkedProgram(
        IReadOnlyList<IJyroStatement> programStatements,
        IReadOnlyDictionary<string, IJyroFunction> availableFunctions)
    {
        Statements = programStatements ?? throw new ArgumentNullException(nameof(programStatements));
        Functions = availableFunctions ?? throw new ArgumentNullException(nameof(availableFunctions));
    }

    /// <inheritdoc />
    public IReadOnlyList<IJyroStatement> Statements { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IJyroFunction> Functions { get; }
}