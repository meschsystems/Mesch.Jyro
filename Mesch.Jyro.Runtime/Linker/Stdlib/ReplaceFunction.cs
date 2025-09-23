namespace Mesch.Jyro;

/// <summary>
/// Replaces all occurrences of a specified substring with a replacement string
/// within a source string. Performs case-sensitive string replacement using
/// ordinal comparison for consistent behavior across different cultural contexts.
/// </summary>
public sealed class ReplaceFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaceFunction"/> class
    /// with a signature that accepts source string, search string, and replacement string.
    /// </summary>
    public ReplaceFunction() : base(new JyroFunctionSignature(
        "Replace",
        new[] {
            new Parameter("source", ParameterType.String),
            new Parameter("oldValue", ParameterType.String),
            new Parameter("newValue", ParameterType.String)
        },
        ParameterType.String))
    {
    }

    /// <summary>
    /// Executes the string replacement operation on all matching substrings.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The source string to process (JyroString)
    /// - arguments[1]: The substring to search for and replace (JyroString)
    /// - arguments[2]: The replacement string to substitute (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context (not used by this function).</param>
    /// <returns>
    /// A <see cref="JyroString"/> containing the modified string with all occurrences
    /// of the search string replaced with the replacement string. If the search
    /// string is not found, the original string is returned unchanged.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var sourceString = GetArgument<JyroString>(arguments, 0);
        var searchString = GetArgument<JyroString>(arguments, 1);
        var replacementString = GetArgument<JyroString>(arguments, 2);

        var modifiedString = sourceString.Value.Replace(searchString.Value, replacementString.Value);
        return new JyroString(modifiedString);
    }
}