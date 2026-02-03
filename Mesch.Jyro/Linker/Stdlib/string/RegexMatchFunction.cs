using System.Text.RegularExpressions;

namespace Mesch.Jyro;

/// <summary>
/// Extracts the first regex match from text as a string.
/// Returns null if no match is found. Designed for composability
/// with other string functions like Upper(), Lower(), Length(), etc.
/// </summary>
public sealed class RegexMatchFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegexMatchFunction"/> class
    /// with a signature that accepts text and a regex pattern.
    /// </summary>
    public RegexMatchFunction() : base(new JyroFunctionSignature(
        "RegexMatch",
        new[] {
            new Parameter("text", ParameterType.String),
            new Parameter("pattern", ParameterType.String)
        },
        ParameterType.String))
    {
    }

    /// <summary>
    /// Executes the regex match operation on the specified text.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The source text to search (JyroString)
    /// - arguments[1]: The regex pattern (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroString"/> containing the first match, or <see cref="JyroNull"/>
    /// if no match is found.
    /// </returns>
    /// <exception cref="JyroRuntimeException">Thrown when the regex pattern is invalid.</exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var text = GetStringArgument(arguments, 0);
        var pattern = GetArgument<JyroString>(arguments, 1);

        try
        {
            var regex = new Regex(pattern.Value);
            var match = regex.Match(text);

            if (!match.Success)
            {
                return JyroNull.Instance;
            }

            return new JyroString(match.Value);
        }
        catch (ArgumentException ex)
        {
            throw new JyroRuntimeException($"RegexMatch(): Invalid regex pattern - {ex.Message}");
        }
    }
}
