namespace Mesch.Jyro;

/// <summary>
/// Represents an error that occurs during Jyro program execution, typically
/// raised by function implementations when encountering invalid operations,
/// constraint violations, or other runtime failures.
/// </summary>
public sealed class JyroRuntimeException : Exception
{
    /// <summary>
    /// Gets the diagnostic code for this runtime error.
    /// </summary>
    public MessageCode Code { get; }

    /// <summary>
    /// Gets the 1-based line number where the error occurred, or 0 if unknown.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the 1-based column position where the error occurred, or 0 if unknown.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JyroRuntimeException"/> class
    /// with complete diagnostic information including error code and source location.
    /// </summary>
    /// <param name="code">The diagnostic code identifying the specific error type.</param>
    /// <param name="lineNumber">The 1-based line number where the error occurred.</param>
    /// <param name="columnPosition">The 1-based column position where the error occurred.</param>
    /// <param name="errorMessage">The message that describes the runtime error.</param>
    public JyroRuntimeException(MessageCode code, int lineNumber, int columnPosition, string errorMessage)
        : base(errorMessage)
    {
        Code = code;
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JyroRuntimeException"/> class
    /// with the specified error message. Uses RuntimeError as the default code
    /// and unknown (0) location.
    /// </summary>
    /// <param name="errorMessage">
    /// The message that describes the runtime error.
    /// </param>
    public JyroRuntimeException(string errorMessage) : base(errorMessage)
    {
        Code = MessageCode.RuntimeError;
        LineNumber = 0;
        ColumnPosition = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JyroRuntimeException"/> class
    /// with the specified error message and inner exception. Uses RuntimeError as
    /// the default code and unknown (0) location.
    /// </summary>
    /// <param name="errorMessage">
    /// The message that describes the runtime error.
    /// </param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception.
    /// </param>
    public JyroRuntimeException(string errorMessage, Exception innerException) : base(errorMessage, innerException)
    {
        Code = MessageCode.RuntimeError;
        LineNumber = 0;
        ColumnPosition = 0;
    }
}
