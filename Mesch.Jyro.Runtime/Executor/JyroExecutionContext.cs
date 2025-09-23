namespace Mesch.Jyro;

/// <summary>
/// Represents the complete runtime state for executing a Jyro script, maintaining
/// variables, functions, and execution configuration. Uses a unified variable model
/// where the reserved global identifier "Data" serves as the single source of truth
/// for the root data object.
/// </summary>
public sealed class JyroExecutionContext
{
    /// <summary>
    /// The reserved global identifier that always references the root data object.
    /// This identifier is automatically declared in every execution context.
    /// </summary>
    public const string RootIdentifier = "Data";

    /// <summary>
    /// Initializes a new instance of the <see cref="JyroExecutionContext"/> class
    /// with the specified execution parameters and runtime configuration.
    /// </summary>
    /// <param name="initialDataValue">
    /// The initial data value to assign to the root identifier.
    /// If null, will be converted to <see cref="JyroNull.Instance"/>.
    /// </param>
    /// <param name="linkedProgram">
    /// The linked program containing validated statements and function references.
    /// Cannot be null.
    /// </param>
    /// <param name="executionOptions">
    /// The execution options controlling runtime behavior and configuration.
    /// Cannot be null.
    /// </param>
    /// <param name="resourceLimiter">
    /// The resource limiter enforcing execution quotas and limits.
    /// Cannot be null.
    /// </param>
    /// <param name="scriptResolver">
    /// The optional script resolver for handling dynamic imports and external references.
    /// May be null if dynamic resolution is not required.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token for cooperative cancellation of long-running operations.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="linkedProgram"/>, <paramref name="executionOptions"/>,
    /// or <paramref name="resourceLimiter"/> is null.
    /// </exception>
    public JyroExecutionContext(
        JyroValue initialDataValue,
        JyroLinkedProgram linkedProgram,
        JyroExecutionOptions executionOptions,
        JyroResourceLimiter resourceLimiter,
        JyroScriptResolver? scriptResolver,
        CancellationToken cancellationToken)
    {
        Program = linkedProgram ?? throw new ArgumentNullException(nameof(linkedProgram));
        Options = executionOptions ?? throw new ArgumentNullException(nameof(executionOptions));
        Limiter = resourceLimiter ?? throw new ArgumentNullException(nameof(resourceLimiter));
        Resolver = scriptResolver;
        CancellationToken = cancellationToken;

        Variables = new VariableScopes();
        Messages = [];
        ScriptCallStack = [];
        Functions = linkedProgram.Functions;

        Variables.Declare(RootIdentifier, initialDataValue ?? JyroNull.Instance);
    }

    /// <summary>
    /// Gets the linked program being executed, containing validated statements
    /// and resolved function references.
    /// </summary>
    /// <value>
    /// The <see cref="JyroLinkedProgram"/> instance representing the compiled program.
    /// </value>
    public JyroLinkedProgram Program { get; }

    /// <summary>
    /// Gets the execution options controlling runtime behavior, limits, and configuration.
    /// </summary>
    /// <value>
    /// The <see cref="JyroExecutionOptions"/> instance containing runtime configuration.
    /// </value>
    public JyroExecutionOptions Options { get; }

    /// <summary>
    /// Gets the resource limiter enforcing execution quotas including statement counts,
    /// loop iterations, function call depths, and memory usage.
    /// </summary>
    /// <value>
    /// The <see cref="JyroResourceLimiter"/> instance managing execution resources.
    /// </value>
    public JyroResourceLimiter Limiter { get; }

    /// <summary>
    /// Gets the collection of diagnostic messages produced during execution,
    /// including errors, warnings, and informational messages.
    /// </summary>
    /// <value>
    /// A mutable list of <see cref="IMessage"/> instances representing execution diagnostics.
    /// </value>
    public List<IMessage> Messages { get; }

    /// <summary>
    /// Gets the variable scope stack managing variable declarations and assignments
    /// throughout the execution lifetime.
    /// </summary>
    /// <value>
    /// The <see cref="VariableScopes"/> instance managing variable state.
    /// </value>
    public VariableScopes Variables { get; }

    /// <summary>
    /// Gets the call stack tracking active CallScript executions to prevent infinite recursion.
    /// Contains hash values of script source code currently being executed in the call chain.
    /// Used internally by the CallScript standard library function for cycle detection.
    /// This collection is automatically managed during script execution and should not be
    /// modified directly by external code.
    /// </summary>
    /// <value>
    /// A list of string hash values representing the active script call chain, where each
    /// entry corresponds to a script currently being executed. The list is empty when no
    /// CallScript operations are active. Entries are added when entering a script call
    /// and removed when exiting, maintaining the current execution depth.
    /// </value>
    public List<string> ScriptCallStack { get; }

    /// <summary>
    /// Gets the dictionary of functions available during execution, including
    /// standard library functions, host-provided functions, and any overrides
    /// established during the linking process.
    /// </summary>
    /// <value>
    /// A read-only dictionary mapping function names to their implementations.
    /// </value>
    public IReadOnlyDictionary<string, IJyroFunction> Functions { get; }

    /// <summary>
    /// Gets the cancellation token for cooperative cancellation of long-running
    /// execution operations.
    /// </summary>
    /// <value>
    /// The <see cref="CancellationToken"/> for execution cancellation support.
    /// </value>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets the optional script resolver for handling dynamic imports and
    /// external script references during execution.
    /// </summary>
    /// <value>
    /// The <see cref="JyroScriptResolver"/> instance, or null if dynamic resolution
    /// is not supported for this execution context.
    /// </value>
    public JyroScriptResolver? Resolver { get; }
}