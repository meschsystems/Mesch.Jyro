namespace Mesch.Jyro.CliInterpreter.Options;

/// <summary>
/// Options controlling the Jyro script execution engine's resource limits and constraints.
/// </summary>
public class JyroEngineOptions
{
    public const string SectionName = "JyroOptions";

    /// <summary>
    /// Maximum number of iterations allowed for a single loop.
    /// </summary>
    public int MaxSingleLoopIterations { get; init; } = 10_000;

    /// <summary>
    /// Maximum total number of iterations across all nested loops.
    /// </summary>
    public int MaxNestedLoopIterations { get; init; } = 100_000;

    /// <summary>
    /// Maximum total number of iterations across all loops in the script.
    /// </summary>
    public int MaxTotalLoopIterations { get; init; } = 1_000_000;

    /// <summary>
    /// Maximum number of function calls allowed during execution.
    /// </summary>
    public int MaxFunctionCalls { get; init; } = 10_000;

    /// <summary>
    /// Maximum stack depth for nested function calls and control structures.
    /// </summary>
    public int MaxStackDepth { get; init; } = 100;

    /// <summary>
    /// Maximum number of statements that can be executed.
    /// </summary>
    public int MaxStatementCount { get; init; } = 100_000;

    /// <summary>
    /// Maximum execution time for a script.
    /// </summary>
    public TimeSpan MaxExecutionTime { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Maximum memory allocation during execution (in bytes).
    /// </summary>
    public long MaxMemoryAllocation { get; init; } = 50 * 1024 * 1024; // 50MB
}
