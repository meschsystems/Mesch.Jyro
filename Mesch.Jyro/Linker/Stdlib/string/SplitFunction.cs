namespace Mesch.Jyro;

/// <summary>
/// Splits a string into an array of substrings based on a specified delimiter.
/// Returns all parts including empty strings that result from consecutive delimiters
/// or delimiters at the beginning or end of the source string.
/// </summary>
public sealed class SplitFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SplitFunction"/> class
    /// with a signature that accepts a text string and delimiter, returning an array.
    /// </summary>
    public SplitFunction() : base(new JyroFunctionSignature(
        "Split",
        new[] {
            new Parameter("text", ParameterType.String),
            new Parameter("delimiter", ParameterType.String)
        },
        ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the string splitting operation using the specified delimiter.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The text string to split (JyroString)
    /// - arguments[1]: The delimiter string used for splitting (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroArray"/> containing string elements representing the split parts.
    /// Empty strings are preserved when they occur between consecutive delimiters
    /// or at string boundaries.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var textString = GetArgument<JyroString>(arguments, 0);
        var delimiterString = GetArgument<JyroString>(arguments, 1);

        var splitParts = textString.Value.Split(delimiterString.Value, StringSplitOptions.None);
        var resultArray = new JyroArray();

        foreach (var stringPart in splitParts)
        {
            resultArray.Add(new JyroString(stringPart));
        }

        return resultArray;
    }
}