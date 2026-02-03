using System.Text.RegularExpressions;

namespace Mesch.Jyro;

/// <summary>
/// Extracts the first regex match from text with full metadata including
/// the matched string, index position, and capture groups.
/// Returns null if no match is found. Use this when you need access to
/// capture groups; otherwise prefer RegexMatch() for simpler composability.
/// </summary>
public sealed class RegexMatchDetailFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegexMatchDetailFunction"/> class
    /// with a signature that accepts text and a regex pattern.
    /// </summary>
    public RegexMatchDetailFunction() : base(new JyroFunctionSignature(
        "RegexMatchDetail",
        new[] {
            new Parameter("text", ParameterType.String),
            new Parameter("pattern", ParameterType.String)
        },
        ParameterType.Object))
    {
    }

    /// <summary>
    /// Executes the detailed regex match operation on the specified text.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The source text to search (JyroString)
    /// - arguments[1]: The regex pattern (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroObject"/> containing:
    /// - match: The full matched string
    /// - index: The zero-based position where the match starts
    /// - groups: An array of captured group values (excluding the full match)
    /// Returns <see cref="JyroNull"/> if no match is found.
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

            var result = new JyroObject();
            result.SetProperty("match", new JyroString(match.Value));
            result.SetProperty("index", new JyroNumber(match.Index));

            // Build groups array (skip group 0 which is the full match)
            var groups = new JyroArray();
            for (var i = 1; i < match.Groups.Count; i++)
            {
                groups.Add(new JyroString(match.Groups[i].Value));
            }
            result.SetProperty("groups", groups);

            return result;
        }
        catch (ArgumentException ex)
        {
            throw new JyroRuntimeException($"RegexMatchDetail(): Invalid regex pattern - {ex.Message}");
        }
    }
}
