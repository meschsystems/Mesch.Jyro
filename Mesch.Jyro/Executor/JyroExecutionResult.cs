namespace Mesch.Jyro;

/// <summary>
/// Represents the comprehensive result of executing a Jyro program, containing
/// the final data state, diagnostic messages, and execution metadata.
/// Provides complete information about the execution outcome and performance characteristics.
/// </summary>
public sealed class JyroExecutionResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JyroExecutionResult"/> class
    /// with the specified execution outcome and associated information.
    /// </summary>
    /// <param name="executionSucceeded">
    /// A value indicating whether execution completed successfully without fatal errors.
    /// </param>
    /// <param name="finalDataValue">
    /// The final state of the root data object after execution completion.
    /// Cannot be null.
    /// </param>
    /// <param name="diagnosticMessages">
    /// The collection of diagnostic messages generated during execution.
    /// If null, will be replaced with an empty collection.
    /// </param>
    /// <param name="executionMetadata">
    /// Metadata containing execution statistics and timing information.
    /// Cannot be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="finalDataValue"/> or <paramref name="executionMetadata"/> is null.
    /// </exception>
    public JyroExecutionResult(
        bool executionSucceeded,
        JyroValue finalDataValue,
        IReadOnlyList<IMessage> diagnosticMessages,
        ExecutionMetadata executionMetadata)
    {
        IsSuccessful = executionSucceeded;
        Data = finalDataValue ?? throw new ArgumentNullException(nameof(finalDataValue));
        Messages = diagnosticMessages ?? Array.Empty<IMessage>();
        Metadata = executionMetadata ?? throw new ArgumentNullException(nameof(executionMetadata));
    }

    /// <summary>
    /// Gets a value indicating whether the execution completed successfully
    /// without any fatal errors that prevented completion.
    /// </summary>
    /// <value>
    /// <c>true</c> if execution succeeded; otherwise, <c>false</c> if fatal errors occurred.
    /// </value>
    public bool IsSuccessful { get; }

    /// <summary>
    /// Gets the final state of the root data object after execution completion.
    /// </summary>
    /// <value>
    /// A <see cref="JyroValue"/> representing the final data state, regardless
    /// of whether execution succeeded or failed.
    /// </value>
    public JyroValue Data { get; }

    /// <summary>
    /// Gets the collection of diagnostic messages produced during execution,
    /// including errors, warnings, and informational messages.
    /// </summary>
    /// <value>
    /// A read-only collection of <see cref="IMessage"/> instances representing
    /// all diagnostics generated during execution. Never null, but may be empty.
    /// </value>
    public IReadOnlyList<IMessage> Messages { get; }

    /// <summary>
    /// Gets the number of error-level diagnostic messages in the execution result.
    /// </summary>
    /// <value>
    /// The count of messages with <see cref="MessageSeverity.Error"/> severity.
    /// </value>
    public int ErrorCount => Messages.Count(message => message.Severity == MessageSeverity.Error);

    /// <summary>
    /// Gets metadata containing detailed statistics and timing information
    /// about the execution operation.
    /// </summary>
    /// <value>
    /// An <see cref="ExecutionMetadata"/> instance providing performance
    /// and statistical data about the execution process.
    /// </value>
    public ExecutionMetadata Metadata { get; }
}