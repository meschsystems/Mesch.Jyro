namespace Mesch.Jyro;

/// <summary>
/// Formats a date string according to a specified format pattern using .NET's
/// standard date and time format strings. Supports both standard and custom
/// format patterns for flexible date presentation requirements.
/// </summary>
public sealed class FormatDateFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FormatDateFunction"/> class
    /// with a signature that accepts a date string and format pattern, returning a formatted string.
    /// </summary>
    public FormatDateFunction() : base(FunctionSignatures.Binary("FormatDate", ParameterType.String, ParameterType.String, ParameterType.String))
    {
    }

    /// <summary>
    /// Executes the date formatting operation using the specified format pattern.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The date string to parse and format (JyroString)
    /// - arguments[1]: The format pattern to apply (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context (not used by this function).</param>
    /// <returns>
    /// A <see cref="JyroString"/> containing the formatted date according to the specified pattern.
    /// </returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the date string cannot be parsed or the format pattern is invalid.
    /// The function uses universal time parsing and adjustment for consistent behavior.
    /// </exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var dateStringArgument = GetArgument<JyroString>(arguments, 0);
        var formatPatternArgument = GetArgument<JyroString>(arguments, 1);

        if (!DateTime.TryParse(dateStringArgument.Value, null,
            System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
            out var parsedDate))
        {
            throw new JyroRuntimeException($"Invalid date format: '{dateStringArgument.Value}'");
        }

        try
        {
            var formattedResult = parsedDate.ToString(formatPatternArgument.Value);
            return new JyroString(formattedResult);
        }
        catch (FormatException)
        {
            throw new JyroRuntimeException($"Invalid date format string: '{formatPatternArgument.Value}'");
        }
    }
}