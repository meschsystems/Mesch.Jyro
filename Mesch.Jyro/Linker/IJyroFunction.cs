namespace Mesch.Jyro;

/// <summary>
/// Defines the contract for all Jyro functions, including both standard library 
/// and host-provided functions. Implementations provide compile-time type safety 
/// and eliminate runtime validation overhead through the signature-based validation system.
/// </summary>
public interface IJyroFunction
{
    /// <summary>
    /// Gets the function signature that defines the parameter types, return type, 
    /// and other metadata required for compile-time validation by the linker.
    /// </summary>
    /// <value>
    /// A <see cref="JyroFunctionSignature"/> containing the complete function signature
    /// used by the linker for type checking and parameter validation.
    /// </value>
    JyroFunctionSignature Signature { get; }

    /// <summary>
    /// Executes the function with the provided arguments that have been pre-validated
    /// by the linker against the function signature. The linker guarantees that
    /// arguments match the signature's parameter types exactly.
    /// </summary>
    /// <param name="arguments">
    /// The collection of arguments to pass to the function. The linker validates
    /// that these arguments match the signature's parameter requirements.
    /// </param>
    /// <param name="executionContext">
    /// The execution context providing access to runtime state and services.
    /// </param>
    /// <returns>
    /// A <see cref="JyroValue"/> representing the result of the function execution.
    /// The returned value's type must match the signature's declared return type.
    /// </returns>
    JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext);
}