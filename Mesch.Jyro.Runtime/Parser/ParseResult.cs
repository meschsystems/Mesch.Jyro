namespace Mesch.Jyro;

/// <summary>
/// Represents the result of an individual parsing operation within the parser implementation.
/// This is an internal utility type that provides strict success/failure semantics for
/// granular parsing operations, eliminating ambiguous null returns and providing explicit error context.
/// </summary>
/// <typeparam name="T">The type of the parsed AST node or result.</typeparam>
/// <remarks>
/// ParseResult is designed for internal parser implementation use, distinct from JyroParsingResult
/// which represents the overall outcome of the entire parsing stage. This type enables robust
/// error handling within parser methods by making success and failure states explicit and
/// providing detailed error information for recovery and reporting.
/// 
/// <para>
/// This pattern eliminates the common parser antipattern of returning null for failures,
/// which can lead to null reference exceptions and makes error handling inconsistent.
/// Instead, every parsing operation returns a well-defined result that must be explicitly
/// checked for success or failure.
/// </para>
/// </remarks>
public readonly struct ParseResult<T> where T : class
{
    private readonly T? _value;
    private readonly ParseError? _error;

    /// <summary>
    /// Initializes a new ParseResult with the specified value and error information.
    /// This constructor is private to enforce the use of factory methods for creation.
    /// </summary>
    /// <param name="value">The successfully parsed value, or null if parsing failed.</param>
    /// <param name="error">The error information if parsing failed, or null if successful.</param>
    private ParseResult(T? value, ParseError? error)
    {
        _value = value;
        _error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the parsing operation succeeded.
    /// When true, the Value property can be safely accessed.
    /// </summary>
    public bool IsSuccess => _error == null;

    /// <summary>
    /// Gets a value indicating whether the parsing operation failed.
    /// When true, the Error property can be safely accessed.
    /// </summary>
    public bool IsFailure => _error != null;

    /// <summary>
    /// Gets the successfully parsed value.
    /// This property should only be accessed when IsSuccess is true.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to access the value of a failed parsing result.
    /// </exception>
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException($"Cannot access value of failed parse result: {_error}");

    /// <summary>
    /// Gets the error information for failed parsing operations.
    /// This property should only be accessed when IsFailure is true.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to access the error information of a successful parsing result.
    /// </exception>
    public ParseError Error => IsFailure ? _error! : throw new InvalidOperationException("Cannot access error of successful parse result");

    /// <summary>
    /// Creates a successful parse result containing the specified value.
    /// </summary>
    /// <param name="value">The successfully parsed value to wrap in the result.</param>
    /// <returns>A ParseResult indicating successful parsing with the provided value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static ParseResult<T> Success(T value) => new(value ?? throw new ArgumentNullException(nameof(value)), null);

    /// <summary>
    /// Creates a failed parse result containing the specified error information.
    /// </summary>
    /// <param name="error">The error information describing why parsing failed.</param>
    /// <returns>A ParseResult indicating parsing failure with the provided error details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when error is null.</exception>
    public static ParseResult<T> Failure(ParseError error) => new(null, error ?? throw new ArgumentNullException(nameof(error)));

    /// <summary>
    /// Creates a failed parse result with the specified error details.
    /// This is a convenience method for creating parse errors without explicitly constructing a ParseError instance.
    /// </summary>
    /// <param name="code">The message code categorizing the type of parsing failure.</param>
    /// <param name="token">The token where the parsing error occurred.</param>
    /// <param name="description">A human-readable description of the parsing error.</param>
    /// <returns>A ParseResult indicating parsing failure with the constructed error information.</returns>
    public static ParseResult<T> Failure(MessageCode code, JyroToken token, string description) =>
        new(null, new ParseError(code, token, description));
}

/// <summary>
/// Represents detailed information about a parsing failure within the parser implementation.
/// This class provides comprehensive error context for debugging, error reporting, and
/// potential error recovery operations within the parsing process.
/// </summary>
/// <remarks>
/// ParseError encapsulates all necessary information about parsing failures to enable
/// meaningful error reporting and potential recovery strategies. The error information
/// includes both categorical classification through message codes and specific contextual
/// details about where and why the parsing operation failed.
/// </remarks>
public sealed class ParseError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParseError"/> class with the specified
    /// error classification, location information, and descriptive details.
    /// </summary>
    /// <param name="code">
    /// The message code that categorizes the type of parsing failure.
    /// This enables systematic error handling and localization of error messages.
    /// </param>
    /// <param name="token">
    /// The token where the parsing error occurred. This provides precise location
    /// information for error reporting and potential recovery operations.
    /// </param>
    /// <param name="description">
    /// A human-readable description of the parsing error that provides specific
    /// context about what went wrong and potentially how to resolve the issue.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when token or description is null.</exception>
    public ParseError(MessageCode code, JyroToken token, string description)
    {
        Code = code;
        Token = token ?? throw new ArgumentNullException(nameof(token));
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }

    /// <summary>
    /// Gets the error code that categorizes the type of parsing failure.
    /// This classification enables systematic error handling, localization,
    /// and potential automated recovery strategies.
    /// </summary>
    public MessageCode Code { get; }

    /// <summary>
    /// Gets the token where the parsing error occurred.
    /// This token provides precise location information including line and column
    /// numbers for accurate error reporting and debugging.
    /// </summary>
    public JyroToken Token { get; }

    /// <summary>
    /// Gets a human-readable description of the parsing error.
    /// This description provides specific context about the nature of the error
    /// and may include suggestions for resolution.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Creates a diagnostic message from this parse error for integration with the
    /// broader diagnostic system used throughout the compilation pipeline.
    /// </summary>
    /// <returns>
    /// An IMessage instance containing the error information formatted for
    /// the diagnostic system, enabling consistent error reporting across all compilation stages.
    /// </returns>
    public IMessage ToMessage() =>
        new Message(Code, Token.LineNumber, Token.ColumnPosition,
                   MessageSeverity.Error, ProcessingStage.Parsing, Description);

    /// <summary>
    /// Returns a string representation of this parse error for debugging and logging purposes.
    /// </summary>
    /// <returns>
    /// A formatted string containing the error code, location information, and description
    /// in a readable format suitable for debugging output.
    /// </returns>
    public override string ToString() => $"{Code} at {Token.LineNumber}:{Token.ColumnPosition}: {Description}";
}