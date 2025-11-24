namespace Mesch.Jyro;

/// <summary>
/// Removes leading and trailing whitespace characters from a string, including
/// spaces, tabs, carriage returns, line feeds, and other Unicode whitespace
/// characters as defined by the .NET framework. Preserves internal whitespace.
/// </summary>
public sealed class TrimFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TrimFunction"/> class
    /// with a signature that accepts a string and returns a trimmed string.
    /// </summary>
    public TrimFunction() : base(FunctionSignatures.Unary("Trim", ParameterType.String, ParameterType.String))
    {
    }

    /// <summary>
    /// Executes the whitespace trimming operation on the specified string.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The string to trim (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroString"/> containing the input string with leading and
    /// trailing whitespace removed. Internal whitespace is preserved unchanged.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var inputString = GetArgument<JyroString>(arguments, 0);
        var trimmedResult = inputString.Value.Trim();
        return new JyroString(trimmedResult);
    }
}