using System.Text.RegularExpressions;

namespace Mesch.Jyro;

/// <summary>
/// Tests whether text contains a match for the specified regex pattern.
/// Returns true if a match exists, false otherwise.
/// </summary>
public sealed class RegexTestFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegexTestFunction"/> class
    /// with a signature that accepts text and a regex pattern.
    /// </summary>
    public RegexTestFunction() : base(new JyroFunctionSignature(
        "RegexTest",
        new[] {
            new Parameter("text", ParameterType.String),
            new Parameter("pattern", ParameterType.String)
        },
        ParameterType.Boolean))
    {
    }

    /// <summary>
    /// Executes the regex test operation on the specified text.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The source text to search (JyroString)
    /// - arguments[1]: The regex pattern (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// <see cref="JyroBoolean.True"/> if the pattern matches anywhere in the text,
    /// <see cref="JyroBoolean.False"/> otherwise.
    /// </returns>
    /// <exception cref="JyroRuntimeException">Thrown when the regex pattern is invalid.</exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var text = GetStringArgument(arguments, 0);
        var pattern = GetArgument<JyroString>(arguments, 1);

        try
        {
            var regex = new Regex(pattern.Value);
            var isMatch = regex.IsMatch(text);

            return isMatch ? JyroBoolean.True : JyroBoolean.False;
        }
        catch (ArgumentException ex)
        {
            throw new JyroRuntimeException($"RegexTest(): Invalid regex pattern - {ex.Message}");
        }
    }
}
