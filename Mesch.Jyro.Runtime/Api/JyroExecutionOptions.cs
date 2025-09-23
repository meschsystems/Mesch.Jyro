namespace Mesch.Jyro;

/// <summary>
/// Defines configuration options for Jyro script execution, including resource limits,
/// environment settings, and diagnostic configuration. These options provide fine-grained
/// control over script execution behavior and help prevent runaway scripts.
/// </summary>
public sealed class JyroExecutionOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JyroExecutionOptions"/> class
    /// with reasonable default values suitable for most use cases.
    /// </summary>
    public JyroExecutionOptions()
    {
        MaxExecutionTime = TimeSpan.FromSeconds(5);
        MaxStatements = 10_000;
        MaxLoops = 1_000;
        MaxStackDepth = 256;
        MaxCallDepth = 64;
        MaxScriptCallDepth = 5;
        MessageProvider = new MessageProvider();
        HostFunctions = [];
    }

    /// <summary>
    /// Gets or sets the maximum wall-clock execution time allowed for a script.
    /// Scripts that exceed this time limit will be terminated with a timeout error.
    /// Default value is 5 seconds.
    /// </summary>
    /// <value>A TimeSpan representing the maximum execution time.</value>
    public TimeSpan MaxExecutionTime { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of statements that may be executed during script execution.
    /// This limit helps prevent infinite loops and runaway scripts from consuming excessive resources.
    /// Default value is 10,000 statements.
    /// </summary>
    /// <value>The maximum number of statements allowed.</value>
    public int MaxStatements { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of loop iterations that may be performed across all loops.
    /// This provides an additional safeguard against infinite loops beyond the statement limit.
    /// Default value is 1,000 iterations.
    /// </summary>
    /// <value>The maximum number of loop iterations allowed.</value>
    public int MaxLoops { get; set; }

    /// <summary>
    /// Gets or sets the maximum stack depth allowed for nested scopes and variable declarations.
    /// This prevents stack overflow errors from deeply nested code structures.
    /// Default value is 256 levels.
    /// </summary>
    /// <value>The maximum stack depth allowed.</value>
    public int MaxStackDepth { get; set; }

    /// <summary>
    /// Gets or sets the maximum function or script call depth allowed for recursive operations.
    /// This prevents stack overflow errors from excessive recursion in function calls.
    /// Default value is 64 levels.
    /// </summary>
    /// <value>The maximum call depth allowed.</value>
    public int MaxCallDepth { get; set; }

    /// <summary>
    /// Gets or sets the maximum depth that scripts can call other scripts (script chaining).
    /// This prevents excessive script chains.
    /// Default value is 5 (levels deep).
    /// </summary>
    /// <value>The maximum script chain depth allowed.</value>
    public int MaxScriptCallDepth { get; set; }

    /// <summary>
    /// Gets or sets the message provider used to format and localize diagnostic messages
    /// emitted during script execution. This enables customization of error messages
    /// and internationalization support.
    /// Default value is a new instance of <see cref="MessageProvider"/>.
    /// </summary>
    /// <value>The message provider instance to use for diagnostic formatting.</value>
    public IMessageProvider MessageProvider { get; set; }

    /// <summary>
    /// Gets or sets the collection of host functions that should be made available
    /// to Jyro scripts during execution. These functions extend the script's capabilities
    /// by providing access to host application functionality.
    /// Default value is an empty collection.
    /// </summary>
    /// <value>An enumerable collection of host functions to register.</value>
    public IEnumerable<IJyroFunction> HostFunctions { get; set; }
}