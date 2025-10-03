namespace Mesch.Jyro;

/// <summary>
/// Contains metadata and performance information collected during the parsing stage
/// of Jyro program processing. Used for debugging, analysis, performance monitoring,
/// and diagnostics tracking.
/// </summary>
public sealed class ParsingMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParsingMetadata"/> class
    /// with the specified performance metrics and statistical information.
    /// </summary>
    /// <param name="parsingProcessingTime">
    /// The total time taken to complete the parsing operation.
    /// </param>
    /// <param name="tokenCount">
    /// The total number of tokens processed during parsing.
    /// </param>
    /// <param name="parsingStartedAt">
    /// The timestamp when the parsing operation was initiated.
    /// </param>
    public ParsingMetadata(
        TimeSpan parsingProcessingTime,
        int tokenCount,
        DateTimeOffset parsingStartedAt)
    {
        ProcessingTime = parsingProcessingTime;
        TokenCount = tokenCount;
        StartedAt = parsingStartedAt;
    }

    /// <summary>
    /// Gets the total time taken to complete the parsing operation.
    /// </summary>
    /// <value>
    /// A <see cref="TimeSpan"/> representing the elapsed time from the start
    /// to the completion of the parsing process.
    /// </value>
    public TimeSpan ProcessingTime { get; }

    /// <summary>
    /// Gets the total number of tokens that were processed during parsing.
    /// </summary>
    /// <value>
    /// The count of individual tokens in the token stream.
    /// </value>
    public int TokenCount { get; }

    /// <summary>
    /// Gets the timestamp when the parsing operation was initiated.
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> representing the moment when parsing began.
    /// </value>
    public DateTimeOffset StartedAt { get; }
}
