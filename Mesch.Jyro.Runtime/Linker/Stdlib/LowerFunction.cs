namespace Mesch.Jyro;

/// <summary>
/// Converts a string to lowercase using culture-invariant conversion rules.
/// This function ensures consistent lowercase conversion behavior across
/// different system locales and cultural settings.
/// </summary>
public sealed class LowerFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LowerFunction"/> class
    /// with a signature that accepts a string and returns a lowercase string.
    /// </summary>
    public LowerFunction() : base(FunctionSignatures.Unary("Lower", ParameterType.String, ParameterType.String))
    {
    }

    /// <summary>
    /// Executes the lowercase conversion operation on the specified string.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The string to convert to lowercase (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context (not used by this function).</param>
    /// <returns>
    /// A <see cref="JyroString"/> containing the input string converted to lowercase
    /// using invariant culture rules for consistent behavior across environments.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var inputString = GetArgument<JyroString>(arguments, 0);
        var lowercaseResult = inputString.Value.ToLowerInvariant();
        return new JyroString(lowercaseResult);
    }
}