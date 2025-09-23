namespace Mesch.Jyro;

/// <summary>
/// Defines the contract for executing linked Jyro programs against data objects.
/// Implementations provide the runtime engine that processes program statements
/// and manages execution context including variables, functions, and resource limits.
/// </summary>
public interface IExecutor
{
    /// <summary>
    /// Executes the specified linked program within the provided execution context.
    /// </summary>
    /// <param name="linkedProgram">
    /// The fully linked Jyro program containing validated statements and resolved functions.
    /// Cannot be null.
    /// </param>
    /// <param name="executionContext">
    /// The execution context containing runtime state, data, configuration, and resource limits.
    /// Cannot be null.
    /// </param>
    /// <returns>
    /// A <see cref="JyroExecutionResult"/> containing the execution outcome, including
    /// success status, final data state, diagnostic messages, performance statistics,
    /// and elapsed execution time.
    /// </returns>
    JyroExecutionResult Execute(ILinkedProgram linkedProgram, JyroExecutionContext executionContext);
}