namespace Mesch.Jyro;

/// <summary>
/// Represents the result of a validation operation, encapsulating success or failure
/// state along with detailed error information when validation fails.
/// Used throughout the Jyro system for consistent validation result handling.
/// </summary>
public readonly struct ValidationResult
{
    private readonly IMessage? _validationError;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationResult"/> struct
    /// with the specified error message.
    /// </summary>
    /// <param name="validationError">
    /// The error message if validation failed, or null if validation succeeded.
    /// </param>
    private ValidationResult(IMessage? validationError)
    {
        _validationError = validationError;
    }

    /// <summary>
    /// Gets a value indicating whether the validation operation succeeded.
    /// </summary>
    /// <value>
    /// <c>true</c> if validation completed successfully; otherwise, <c>false</c>.
    /// </value>
    public bool IsSuccessful => _validationError == null;

    /// <summary>
    /// Gets a value indicating whether the validation operation failed.
    /// </summary>
    /// <value>
    /// <c>true</c> if validation failed with errors; otherwise, <c>false</c>.
    /// </value>
    public bool IsFailure => _validationError != null;

    /// <summary>
    /// Gets the validation error message when validation has failed.
    /// </summary>
    /// <value>
    /// An <see cref="IMessage"/> containing details about the validation failure.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to access the error on a successful validation result.
    /// </exception>
    public IMessage Error => _validationError ?? throw new InvalidOperationException("Cannot access error information on a successful validation result");

    /// <summary>
    /// Creates a validation result representing successful validation.
    /// </summary>
    /// <returns>
    /// A <see cref="ValidationResult"/> indicating that validation completed successfully.
    /// </returns>
    public static ValidationResult Success() => new(null);

    /// <summary>
    /// Creates a validation result representing failed validation with detailed error information.
    /// </summary>
    /// <param name="messageCode">
    /// The specific error code identifying the type of validation failure.
    /// </param>
    /// <param name="sourceLineNumber">
    /// The line number in source code where the validation error occurred.
    /// </param>
    /// <param name="sourceColumnPosition">
    /// The column position in source code where the validation error occurred.
    /// </param>
    /// <param name="errorDescription">
    /// A descriptive message explaining the validation failure.
    /// </param>
    /// <returns>
    /// A <see cref="ValidationResult"/> containing the detailed error information.
    /// </returns>
    public static ValidationResult Failure(MessageCode messageCode, int sourceLineNumber, int sourceColumnPosition, string errorDescription) =>
        new(new Message(messageCode, sourceLineNumber, sourceColumnPosition, MessageSeverity.Error, ProcessingStage.Linking, errorDescription));
}