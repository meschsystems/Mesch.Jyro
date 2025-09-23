namespace Mesch.Jyro;

/// <summary>
/// Returns the current date and time as an ISO 8601 formatted string in UTC.
/// Provides a consistent timestamp format suitable for logging, data processing,
/// and cross-system communication requirements.
/// </summary>
public sealed class NowFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NowFunction"/> class
    /// with a signature that accepts no parameters and returns a string.
    /// </summary>
    public NowFunction() : base(new JyroFunctionSignature("Now", Array.Empty<Parameter>(), ParameterType.String))
    {
    }

    /// <summary>
    /// Executes the current time retrieval operation.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments (empty for this function).
    /// </param>
    /// <param name="executionContext">The execution context (not used by this function).</param>
    /// <returns>
    /// A <see cref="JyroString"/> containing the current UTC date and time
    /// in ISO 8601 format (yyyy-MM-ddTHH:mm:ss.fffZ).
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var currentUtcTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        return new JyroString(currentUtcTime);
    }
}