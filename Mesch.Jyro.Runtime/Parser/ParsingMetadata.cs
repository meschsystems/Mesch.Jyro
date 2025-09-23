namespace Mesch.Jyro;

/// <summary>
/// Contains metadata and statistical information about the parsing process.
/// This metadata provides insights into the complexity and performance characteristics
/// of the syntax analysis phase, enabling optimization and debugging of parser behavior.
/// </summary>
/// <remarks>
/// Parsing metadata serves multiple purposes in the compilation pipeline:
/// <list type="bullet">
/// <item><description>Performance analysis: Tracking processing time and complexity metrics</description></item>
/// <item><description>Code complexity assessment: Understanding AST structure and nesting patterns</description></item>
/// <item><description>Development insights: Providing data for parser optimization and tuning</description></item>
/// <item><description>Tooling integration: Enabling development tools to provide structural analysis</description></item>
/// </list>
/// The nesting depth metric is particularly valuable for identifying deeply nested code
/// that might impact runtime performance or exceed stack limits during execution.
/// </remarks>
public sealed class ParsingMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParsingMetadata"/> class with the specified
    /// performance metrics and structural analysis information.
    /// </summary>
    /// <param name="processingTime">
    /// The total time taken to complete the parsing process from token input to AST output.
    /// This includes all syntax analysis work and error recovery operations.
    /// </param>
    /// <param name="statementCount">
    /// The number of top-level statements parsed from the input token sequence.
    /// This provides a measure of the script's structural complexity at the statement level.
    /// </param>
    /// <param name="maxNestingDepth">
    /// The maximum depth of nested constructs observed in the resulting abstract syntax tree.
    /// This metric helps identify potentially problematic deep nesting that could impact performance.
    /// </param>
    /// <param name="startedAt">
    /// The timestamp when the parsing process began, enabling correlation with other stages
    /// and absolute timing analysis for performance monitoring.
    /// </param>
    public ParsingMetadata(TimeSpan processingTime, int statementCount, int maxNestingDepth, DateTimeOffset startedAt)
    {
        ProcessingTime = processingTime;
        StatementCount = statementCount;
        MaxNestingDepth = maxNestingDepth;
        StartedAt = startedAt;
    }

    /// <summary>
    /// Gets the total time taken to complete the parsing process.
    /// This duration encompasses the entire syntax analysis from token input
    /// to abstract syntax tree output, including error detection and recovery.
    /// </summary>
    public TimeSpan ProcessingTime { get; }

    /// <summary>
    /// Gets the number of top-level statements parsed from the input.
    /// This count represents the primary structural units of the script
    /// and provides a basic measure of code complexity and size.
    /// </summary>
    public int StatementCount { get; }

    /// <summary>
    /// Gets the maximum nesting depth observed in the abstract syntax tree.
    /// This metric identifies the deepest level of nested language constructs
    /// such as nested expressions, control structures, or function calls,
    /// which can impact both compilation and runtime performance.
    /// </summary>
    public int MaxNestingDepth { get; }

    /// <summary>
    /// Gets the timestamp when the parsing process started.
    /// This absolute timestamp enables correlation with other compilation stages
    /// and provides context for performance analysis and debugging scenarios.
    /// </summary>
    public DateTimeOffset StartedAt { get; }
}