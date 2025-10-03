namespace Mesch.Jyro;

/// <summary>
/// Converts a string to uppercase using culture-invariant conversion rules.
/// This function ensures consistent uppercase conversion behavior across
/// different system locales and cultural settings for reliable text processing.
/// </summary>
public sealed class UpperFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpperFunction"/> class
    /// with a signature that accepts a string and returns an uppercase string.
    /// </summary>
    public UpperFunction() : base(FunctionSignatures.Unary("Upper", ParameterType.String, ParameterType.String))
    {
    }

    /// <summary>
    /// Executes the uppercase conversion operation on the specified string.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The string to convert to uppercase (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context (not used by this function).</param>
    /// <returns>
    /// A <see cref="JyroString"/> containing the input string converted to uppercase
    /// using invariant culture rules for consistent behavior across environments.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var inputString = GetArgument<JyroString>(arguments, 0);
        var uppercaseResult = inputString.Value.ToUpperInvariant();
        return new JyroString(uppercaseResult);
    }
}