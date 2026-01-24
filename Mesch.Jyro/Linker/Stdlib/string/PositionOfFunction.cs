namespace Mesch.Jyro;

/// <summary>
/// Finds the zero-based index position of a substring within a string.
/// Returns -1 if the substring is not found. The search is case-sensitive.
/// This function is the string equivalent of IndexOf for arrays.
/// </summary>
public sealed class PositionOfFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PositionOfFunction"/> class
    /// with a signature that accepts two strings and returns a number.
    /// </summary>
    public PositionOfFunction() : base(FunctionSignatures.Binary("PositionOf", ParameterType.String, ParameterType.String, ParameterType.Number))
    {
    }

    /// <summary>
    /// Executes the position search operation on the specified string.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The source string to search within (JyroString)
    /// - arguments[1]: The substring to find (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing the zero-based index of the first
    /// occurrence of the substring, or -1 if the substring is not found.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var sourceString = GetArgument<JyroString>(arguments, 0);
        var searchString = GetArgument<JyroString>(arguments, 1);

        var position = sourceString.Value.IndexOf(searchString.Value, StringComparison.Ordinal);
        return new JyroNumber(position);
    }
}
