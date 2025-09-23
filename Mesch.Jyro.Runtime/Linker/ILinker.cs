namespace Mesch.Jyro;

/// <summary>
/// Defines the contract for linking parsed Jyro abstract syntax trees with
/// available functions to create executable programs. The linker performs
/// type checking, function resolution, and validation to ensure program correctness.
/// </summary>
public interface ILinker
{
    /// <summary>
    /// Links the specified abstract syntax tree statements with the available function
    /// implementations to create an executable program. All provided functions must
    /// implement the <see cref="IJyroFunction"/> interface with proper signatures
    /// for compile-time validation.
    /// </summary>
    /// <param name="programStatements">
    /// The collection of parsed AST statements representing the program logic.
    /// </param>
    /// <param name="hostFunctions">
    /// The optional collection of host-provided functions that will be available
    /// to the script during execution. If null, only standard library functions
    /// will be available.
    /// </param>
    /// <returns>
    /// A <see cref="JyroLinkingResult"/> containing the linking outcome, including
    /// the successfully linked program (if linking succeeded), comprehensive
    /// diagnostics information, and performance metrics for the linking operation.
    /// </returns>
    JyroLinkingResult Link(IReadOnlyList<IJyroStatement> programStatements, IEnumerable<IJyroFunction>? hostFunctions = null);
}