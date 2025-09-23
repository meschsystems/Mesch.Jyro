namespace Mesch.Jyro;

/// <summary>
/// Metadata about the validation stage for debugging and analysis.
/// </summary>
public sealed class ValidationMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationMetadata"/> class.
    /// </summary>
    /// <param name="processingTime">The time taken to complete validation.</param>
    /// <param name="startedAt">The timestamp when validation started.</param>
    public ValidationMetadata(TimeSpan processingTime, DateTimeOffset startedAt)
    {
        ProcessingTime = processingTime;
        StartedAt = startedAt;
    }

    /// <summary>
    /// Gets the time taken to complete validation.
    /// </summary>
    public TimeSpan ProcessingTime { get; }

    /// <summary>
    /// Gets the timestamp when validation started.
    /// </summary>
    public DateTimeOffset StartedAt { get; }
}
