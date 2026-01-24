namespace Mesch.Jyro;

/// <summary>
/// Extracts a portion of a string starting at a specified position and optionally
/// for a specified length. If length is not provided, returns from start position
/// to the end of the string. Returns empty string if start position is beyond
/// string length. Handles negative indices and out-of-bounds gracefully.
/// </summary>
public sealed class SubstringFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubstringFunction"/> class
    /// with a signature that accepts a string, start position, and optional length.
    /// </summary>
    public SubstringFunction() : base(new JyroFunctionSignature(
        "Substring",
        new[] {
            new Parameter("text", ParameterType.String),
            new Parameter("start", ParameterType.Number),
            new Parameter("length", ParameterType.Number, isOptionalParameter: true)
        },
        ParameterType.String))
    {
    }

    /// <summary>
    /// Executes the substring extraction operation on the specified string.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The source string (JyroString)
    /// - arguments[1]: The zero-based start position (JyroNumber, must be integer)
    /// - arguments[2]: Optional length of substring to extract (JyroNumber, must be integer)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroString"/> containing the extracted substring. Returns empty
    /// string if start position is beyond string length or if length is zero or negative.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var inputString = GetArgument<JyroString>(arguments, 0);
        var startArgument = GetArgument<JyroNumber>(arguments, 1);
        var lengthArgument = GetOptionalArgument<JyroNumber>(arguments, 2);

        var text = inputString.Value;
        var start = startArgument.ToInteger();

        // Handle negative start index
        if (start < 0)
        {
            start = 0;
        }

        // Handle start beyond string length
        if (start >= text.Length)
        {
            return new JyroString(string.Empty);
        }

        // Calculate length
        int length;
        if (lengthArgument != null)
        {
            length = lengthArgument.ToInteger();
            if (length <= 0)
            {
                return new JyroString(string.Empty);
            }
            // Clamp length to remaining characters
            length = Math.Min(length, text.Length - start);
        }
        else
        {
            // No length specified, take rest of string
            length = text.Length - start;
        }

        var result = text.Substring(start, length);
        return new JyroString(result);
    }
}
