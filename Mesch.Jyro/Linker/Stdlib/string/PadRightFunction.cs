namespace Mesch.Jyro;

/// <summary>
/// Pads a string on the right side to a specified total length using a
/// specified padding character. If the string is already longer than
/// the specified length, the original string is returned unchanged.
/// The padding character defaults to a space if not specified.
/// </summary>
public sealed class PadRightFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PadRightFunction"/> class
    /// with a signature that accepts a string, target length, and optional padding character.
    /// </summary>
    public PadRightFunction() : base(new JyroFunctionSignature(
        "PadRight",
        new[] {
            new Parameter("text", ParameterType.String),
            new Parameter("length", ParameterType.Number),
            new Parameter("padChar", ParameterType.String, isOptionalParameter: true)
        },
        ParameterType.String))
    {
    }

    /// <summary>
    /// Executes the right padding operation on the specified string.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The string to pad (JyroString)
    /// - arguments[1]: The target total length (JyroNumber, must be integer)
    /// - arguments[2]: Optional padding character (JyroString, first character used, defaults to space)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroString"/> padded on the right to the specified length.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var inputString = GetArgument<JyroString>(arguments, 0);
        var lengthArgument = GetArgument<JyroNumber>(arguments, 1);
        var padCharArgument = GetOptionalArgument<JyroString>(arguments, 2);

        var targetLength = lengthArgument.ToInteger();
        var padChar = ' ';

        if (padCharArgument != null && !string.IsNullOrEmpty(padCharArgument.Value))
        {
            padChar = padCharArgument.Value[0];
        }

        var result = inputString.Value.PadRight(targetLength, padChar);
        return new JyroString(result);
    }
}
