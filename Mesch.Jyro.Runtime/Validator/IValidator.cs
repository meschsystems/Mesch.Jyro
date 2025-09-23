namespace Mesch.Jyro;

/// <summary>
/// Defines a validator that performs semantic checks on an AST.
/// </summary>
public interface IValidator
{
    /// <summary>
    /// Validates the specified AST for semantic correctness.
    /// Always returns a detailed result containing success flag,
    /// diagnostics, and elapsed time.
    /// </summary>
    /// <param name="ast">The AST produced by the parser.</param>
    /// <returns>A <see cref="JyroValidationResult"/> describing the outcome.</returns>
    JyroValidationResult Validate(IReadOnlyList<IJyroStatement> ast);
}
