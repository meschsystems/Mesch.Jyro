namespace Mesch.Jyro;

/// <summary>
/// Provides the default implementation of the diagnostic message interface for the Jyro system.
/// This implementation captures all essential diagnostic information including categorization,
/// location context, severity level, and extensible arguments for detailed error reporting.
/// </summary>
/// <remarks>
/// The Message class serves as the primary diagnostic data carrier throughout the Jyro compilation
/// and execution pipeline. It maintains immutable diagnostic state to ensure consistency and
/// reliability in error reporting scenarios. The implementation prioritizes simplicity and
/// performance while providing comprehensive diagnostic context for development tools and users.
/// </remarks>
public sealed class Message : IMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Message"/> class with comprehensive diagnostic information.
    /// </summary>
    /// <param name="code">
    /// The diagnostic code that categorizes this message type and enables systematic handling.
    /// </param>
    /// <param name="lineNumber">
    /// The one-based line number in the source code where this diagnostic applies.
    /// Must be a positive integer representing a valid source location.
    /// </param>
    /// <param name="columnPosition">
    /// The one-based column position in the source code where this diagnostic applies.
    /// Must be a positive integer representing a valid source location.
    /// </param>
    /// <param name="severity">
    /// The severity level indicating the impact and urgency of this diagnostic message.
    /// </param>
    /// <param name="stage">
    /// The compilation pipeline stage that generated this diagnostic message.
    /// </param>
    /// <param name="arguments">
    /// Optional arguments providing context-specific information for message formatting.
    /// These arguments are typically substituted into message templates during formatting.
    /// If null is provided, an empty array is used to ensure consistent behavior.
    /// </param>
    public Message(
        MessageCode code,
        int lineNumber,
        int columnPosition,
        MessageSeverity severity,
        ProcessingStage stage,
        params string[] arguments)
    {
        Code = code;
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
        Severity = severity;
        Stage = stage;
        Arguments = arguments ?? Array.Empty<string>();
    }

    /// <summary>
    /// Gets the diagnostic code that categorizes this message type.
    /// </summary>
    public MessageCode Code { get; }

    /// <summary>
    /// Gets the severity level that indicates the impact and urgency of this message.
    /// </summary>
    public MessageSeverity Severity { get; }

    /// <summary>
    /// Gets the compilation pipeline stage that generated this diagnostic message.
    /// </summary>
    public ProcessingStage Stage { get; }

    /// <summary>
    /// Gets the one-based line number in the source code where this diagnostic applies.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the one-based column position in the source code where this diagnostic applies.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Gets the collection of arguments that provide context-specific information for this message.
    /// These arguments are typically used for template substitution during message formatting
    /// and enable parameterized diagnostic messages with relevant contextual details.
    /// </summary>
    public IReadOnlyList<string> Arguments { get; }

    /// <summary>
    /// Returns a string representation of this diagnostic message for debugging and logging purposes.
    /// The format includes severity, location, code, and arguments in a compact, readable format
    /// suitable for console output and log files.
    /// </summary>
    /// <returns>
    /// A formatted string containing the essential diagnostic information in the format:
    /// [Severity] LineNumber:ColumnPosition Code Arguments
    /// </returns>
    public override string ToString()
    {
        return $"[{Severity}] {LineNumber}:{ColumnPosition} {Code} {string.Join(' ', Arguments)}";
    }
}