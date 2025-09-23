namespace Mesch.Jyro;

/// <summary>
/// Contains metadata and statistical information about the lexical analysis process.
/// This metadata provides insights into the performance and characteristics of the tokenization
/// phase, enabling analysis, debugging, and optimization of the lexing process.
/// </summary>
/// <remarks>
/// Lexing metadata serves multiple purposes in the compilation pipeline:
/// <list type="bullet">
/// <item><description>Performance monitoring: Tracking processing time for optimization</description></item>
/// <item><description>Statistical analysis: Understanding tokenization patterns and complexity</description></item>
/// <item><description>Debugging support: Providing timing information for troubleshooting</description></item>
/// <item><description>Tooling integration: Enabling development tools to provide performance insights</description></item>
/// </list>
/// This information is particularly valuable for large scripts or batch processing scenarios
/// where lexing performance may impact overall compilation time.
/// </remarks>
public sealed class LexingMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LexingMetadata"/> class with the specified
    /// performance metrics and statistical information.
    /// </summary>
    /// <param name="processingTime">
    /// The total time taken to complete the lexical analysis process.
    /// This includes all tokenization work from start to finish.
    /// </param>
    /// <param name="tokenCount">
    /// The total number of tokens produced during lexical analysis.
    /// This includes all token types except the implicit end-of-file token.
    /// </param>
    /// <param name="startedAt">
    /// The timestamp when the lexical analysis process began.
    /// Used for absolute timing and correlation with other processing stages.
    /// </param>
    public LexingMetadata(TimeSpan processingTime, int tokenCount, DateTimeOffset startedAt)
    {
        ProcessingTime = processingTime;
        TokenCount = tokenCount;
        StartedAt = startedAt;
    }

    /// <summary>
    /// Gets the total time taken to complete the lexical analysis process.
    /// This duration encompasses the entire tokenization process from source input
    /// to token sequence output, including error detection and recovery.
    /// </summary>
    public TimeSpan ProcessingTime { get; }

    /// <summary>
    /// Gets the total number of tokens produced by the lexical analyzer.
    /// This count includes all meaningful tokens extracted from the source code
    /// but excludes whitespace, comments, and the implicit end-of-file marker.
    /// </summary>
    public int TokenCount { get; }

    /// <summary>
    /// Gets the timestamp when the lexical analysis process started.
    /// This absolute timestamp enables correlation with other compilation stages
    /// and provides context for performance analysis and debugging scenarios.
    /// </summary>
    public DateTimeOffset StartedAt { get; }
}