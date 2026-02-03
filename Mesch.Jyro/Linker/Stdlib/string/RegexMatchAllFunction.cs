using System.Text.RegularExpressions;

namespace Mesch.Jyro;

/// <summary>
/// Extracts all regex matches from text as an array of strings.
/// Returns an empty array if no matches are found. Designed for composability
/// with array functions like First(), Last(), Length(), Filter(), etc.
/// </summary>
public sealed class RegexMatchAllFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegexMatchAllFunction"/> class
    /// with a signature that accepts text and a regex pattern.
    /// </summary>
    public RegexMatchAllFunction() : base(new JyroFunctionSignature(
        "RegexMatchAll",
        new[] {
            new Parameter("text", ParameterType.String),
            new Parameter("pattern", ParameterType.String)
        },
        ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the regex match all operation on the specified text.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The source text to search (JyroString)
    /// - arguments[1]: The regex pattern (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroArray"/> containing all matches as strings.
    /// Returns an empty array if no matches are found.
    /// </returns>
    /// <exception cref="JyroRuntimeException">Thrown when the regex pattern is invalid.</exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var text = GetStringArgument(arguments, 0);
        var pattern = GetArgument<JyroString>(arguments, 1);

        try
        {
            var regex = new Regex(pattern.Value);
            var matches = regex.Matches(text);

            var result = new JyroArray();
            foreach (Match match in matches)
            {
                result.Add(new JyroString(match.Value));
            }

            return result;
        }
        catch (ArgumentException ex)
        {
            throw new JyroRuntimeException($"RegexMatchAll(): Invalid regex pattern - {ex.Message}");
        }
    }
}
