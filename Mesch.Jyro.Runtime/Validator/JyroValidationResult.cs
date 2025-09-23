namespace Mesch.Jyro;

/// <summary>
/// Result of validation stage with comprehensive metadata and diagnostics.
/// </summary>
public sealed class JyroValidationResult
{
    /// <summary>
    /// Initializes a new validation result.
    /// </summary>
    /// <param name="isSuccessful">Whether validation completed successfully</param>
    /// <param name="messages">Messages generated during validation</param>
    /// <param name="processingTime">Time taken for validation</param>
    /// <param name="metadata">Metadata containing validation statistics and timing information</param>
    public JyroValidationResult(
        bool isSuccessful,
        IReadOnlyList<IMessage> messages,
        TimeSpan processingTime,
        ValidationMetadata metadata)
    {
        IsSuccessful = isSuccessful;
        Messages = messages ?? Array.Empty<IMessage>();
        ProcessingTime = processingTime;
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    }

    /// <summary>
    /// Whether the validation completed successfully.
    /// </summary>
    public bool IsSuccessful { get; }

    /// <summary>
    /// Messages generated during validation.
    /// </summary>
    public IReadOnlyList<IMessage> Messages { get; }

    /// <summary>
    /// Time taken to complete validation.
    /// </summary>
    public TimeSpan ProcessingTime { get; }

    /// <summary>
    /// Gets metadata containing validation statistics and timing information.
    /// </summary>
    public ValidationMetadata Metadata { get; }

    /// <summary>
    /// Number of error messages.
    /// </summary>
    public int ErrorCount => Messages.Count(m => m.Severity == MessageSeverity.Error);

    /// <summary>
    /// Number of warning messages.
    /// </summary>
    public int WarningCount => Messages.Count(m => m.Severity == MessageSeverity.Warning);

    /// <summary>
    /// Number of informational messages.
    /// </summary>
    public int InfoCount => Messages.Count(m => m.Severity == MessageSeverity.Info);

    /// <summary>
    /// Indicates whether the validation produced any diagnostics.
    /// </summary>
    public bool HasDiagnostics => Messages.Count > 0;

    /// <summary>
    /// Gets all error messages from the validation.
    /// </summary>
    public IEnumerable<IMessage> Errors => Messages.Where(m => m.Severity == MessageSeverity.Error);

    /// <summary>
    /// Gets all warning messages from the validation.
    /// </summary>
    public IEnumerable<IMessage> Warnings => Messages.Where(m => m.Severity == MessageSeverity.Warning);
}