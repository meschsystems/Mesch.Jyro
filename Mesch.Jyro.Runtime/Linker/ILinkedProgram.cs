namespace Mesch.Jyro;

/// <summary>
/// Represents a fully linked Jyro program that has passed compilation and 
/// linking validation, making it ready for execution by the runtime engine.
/// A linked program contains validated statements and resolved function references.
/// </summary>
public interface ILinkedProgram
{
    /// <summary>
    /// Gets the collection of executable statements that comprise the program body.
    /// These statements have been validated and are ready for execution in sequence.
    /// </summary>
    /// <value>
    /// An ordered collection of <see cref="IJyroStatement"/> instances representing
    /// the program's executable logic.
    /// </value>
    IReadOnlyList<IJyroStatement> Statements { get; }

    /// <summary>
    /// Gets the dictionary of functions available to the program during execution,
    /// including both standard library functions and host-provided custom functions.
    /// All function references in the program statements are guaranteed to resolve
    /// to entries in this collection.
    /// </summary>
    /// <value>
    /// A dictionary mapping function names to their corresponding <see cref="IJyroFunction"/>
    /// implementations, providing the complete function execution environment.
    /// </value>
    IReadOnlyDictionary<string, IJyroFunction> Functions { get; }
}