namespace Mesch.Jyro;

/// <summary>
/// Contains metadata and performance information collected during the linking stage
/// of Jyro program compilation. Used for debugging, analysis, and performance monitoring.
/// </summary>
public sealed class JyroLinkingMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JyroLinkingMetadata"/> class
    /// with the specified performance and statistical information.
    /// </summary>
    /// <param name="linkingProcessingTime">
    /// The total time taken to complete the linking operation.
    /// </param>
    /// <param name="resolvedFunctionCount">
    /// The number of functions that were successfully resolved and made available
    /// to the program.
    /// </param>
    /// <param name="linkingStartedAt">
    /// The timestamp when the linking operation was initiated.
    /// </param>
    public JyroLinkingMetadata(TimeSpan linkingProcessingTime, int resolvedFunctionCount, DateTimeOffset linkingStartedAt)
    {
        ProcessingTime = linkingProcessingTime;
        FunctionCount = resolvedFunctionCount;
        StartedAt = linkingStartedAt;
    }

    /// <summary>
    /// Gets the total time taken to complete the linking operation.
    /// </summary>
    /// <value>
    /// A <see cref="TimeSpan"/> representing the elapsed time from the start
    /// to the completion of the linking process.
    /// </value>
    public TimeSpan ProcessingTime { get; }

    /// <summary>
    /// Gets the number of functions that were successfully resolved and made
    /// available to the linked program.
    /// </summary>
    /// <value>
    /// The count of functions in the program's function dictionary, including
    /// both standard library and host-provided functions.
    /// </value>
    public int FunctionCount { get; }

    /// <summary>
    /// Gets the timestamp when the linking operation was initiated.
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> representing the moment when linking began.
    /// </value>
    public DateTimeOffset StartedAt { get; }
}