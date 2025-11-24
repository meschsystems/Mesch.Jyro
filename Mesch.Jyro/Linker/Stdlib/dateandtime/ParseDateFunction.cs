namespace Mesch.Jyro;

/// <summary>
/// Parses a date string using multiple format patterns and returns a normalized
/// ISO 8601 formatted date string. Attempts parsing with common date formats
/// before falling back to general date parsing for maximum compatibility.
/// </summary>
public sealed class ParseDateFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParseDateFunction"/> class
    /// with a signature that accepts a date string and returns a normalized date string.
    /// </summary>
    public ParseDateFunction() : base(FunctionSignatures.Unary("ParseDate", ParameterType.String, ParameterType.String))
    {
    }

    /// <summary>
    /// Executes the date parsing operation using multiple format patterns.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The date string to parse (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroString"/> containing the parsed date in ISO 8601 format
    /// (yyyy-MM-ddTHH:mm:ss.fffZ) in UTC timezone.
    /// </returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the date string cannot be parsed using any of the supported formats.
    /// Supported formats include ISO dates, common slash-separated formats, and
    /// general date parsing as a fallback.
    /// </exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var dateStringArgument = GetArgument<JyroString>(arguments, 0);

        var supportedFormats = new[]
        {
            "yyyy-MM-dd",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            "MM/dd/yyyy",
            "dd/MM/yyyy",
            "yyyy/MM/dd"
        };

        if (DateTime.TryParseExact(dateStringArgument.Value, supportedFormats, null,
            System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
            out var exactParsedDate))
        {
            var formattedExactResult = exactParsedDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            return new JyroString(formattedExactResult);
        }

        if (DateTime.TryParse(dateStringArgument.Value, null,
            System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
            out var generalParsedDate))
        {
            var formattedGeneralResult = generalParsedDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            return new JyroString(formattedGeneralResult);
        }

        throw new JyroRuntimeException($"Unable to parse date: '{dateStringArgument.Value}'");
    }
}