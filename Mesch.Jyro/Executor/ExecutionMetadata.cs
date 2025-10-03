namespace Mesch.Jyro;

/// <summary>
/// Contains metadata and performance information collected during the execution stage
/// of Jyro program processing. Used for debugging, analysis, performance monitoring,
/// and resource usage tracking.
/// </summary>
public sealed class ExecutionMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionMetadata"/> class
    /// with the specified performance metrics and statistical information.
    /// </summary>
    /// <param name="executionProcessingTime">
    /// The total time taken to complete the execution operation.
    /// </param>
    /// <param name="executedStatementCount">
    /// The total number of statements executed during the program run.
    /// </param>
    /// <param name="executedLoopCount">
    /// The total number of loop iterations performed during execution.
    /// </param>
    /// <param name="performedFunctionCallCount">
    /// The total number of function calls made during execution.
    /// </param>
    /// <param name="maximumCallDepth">
    /// The maximum call stack depth reached during execution.
    /// </param>
    /// <param name="executionStartedAt">
    /// The timestamp when the execution operation was initiated.
    /// </param>
    public ExecutionMetadata(
        TimeSpan executionProcessingTime,
        int executedStatementCount,
        int executedLoopCount,
        int performedFunctionCallCount,
        int maximumCallDepth,
        DateTimeOffset executionStartedAt)
    {
        ProcessingTime = executionProcessingTime;
        StatementCount = executedStatementCount;
        LoopCount = executedLoopCount;
        FunctionCallCount = performedFunctionCallCount;
        MaxCallDepth = maximumCallDepth;
        StartedAt = executionStartedAt;
    }

    /// <summary>
    /// Gets the total time taken to complete the execution operation.
    /// </summary>
    /// <value>
    /// A <see cref="TimeSpan"/> representing the elapsed time from the start
    /// to the completion of the execution process.
    /// </value>
    public TimeSpan ProcessingTime { get; }

    /// <summary>
    /// Gets the total number of statements that were executed during the program run.
    /// </summary>
    /// <value>
    /// The count of individual statements processed by the execution engine.
    /// </value>
    public int StatementCount { get; }

    /// <summary>
    /// Gets the total number of loop iterations performed during execution.
    /// </summary>
    /// <value>
    /// The aggregate count of all loop iterations across all loop constructs
    /// in the program.
    /// </value>
    public int LoopCount { get; }

    /// <summary>
    /// Gets the total number of function calls made during execution.
    /// </summary>
    /// <value>
    /// The count of function invocations, including both standard library
    /// and host-provided function calls.
    /// </value>
    public int FunctionCallCount { get; }

    /// <summary>
    /// Gets the maximum call stack depth reached during execution.
    /// </summary>
    /// <value>
    /// The deepest level of nested function calls encountered during
    /// the execution process.
    /// </value>
    public int MaxCallDepth { get; }

    /// <summary>
    /// Gets the timestamp when the execution operation was initiated.
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> representing the moment when execution began.
    /// </value>
    public DateTimeOffset StartedAt { get; }
}