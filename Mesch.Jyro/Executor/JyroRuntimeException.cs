namespace Mesch.Jyro;

/// <summary>
/// Represents an error that occurs during Jyro program execution, typically
/// raised by function implementations when encountering invalid operations,
/// constraint violations, or other runtime failures.
/// </summary>
public sealed class JyroRuntimeException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JyroRuntimeException"/> class
    /// with the specified error message.
    /// </summary>
    /// <param name="errorMessage">
    /// The message that describes the runtime error.
    /// </param>
    public JyroRuntimeException(string errorMessage) : base(errorMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JyroRuntimeException"/> class
    /// with the specified error message and inner exception.
    /// </summary>
    /// <param name="errorMessage">
    /// The message that describes the runtime error.
    /// </param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception.
    /// </param>
    public JyroRuntimeException(string errorMessage, Exception innerException) : base(errorMessage, innerException)
    {
    }
}