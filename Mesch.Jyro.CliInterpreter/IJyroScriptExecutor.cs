namespace Mesch.Jyro.CliInterpreter;

/// <summary>
/// Defines the contract for executing Jyro scripts.
/// </summary>
internal interface IJyroScriptExecutor
{
    /// <summary>
    /// Executes a Jyro script according to the configured options.
    /// </summary>
    /// <returns>A task representing the asynchronous execution operation.</returns>
    Task ExecuteAsync();
}