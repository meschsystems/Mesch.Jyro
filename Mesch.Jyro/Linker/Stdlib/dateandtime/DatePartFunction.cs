namespace Mesch.Jyro;

/// <summary>
/// Extracts a specific component from a date string, returning the component
/// as a numeric value. Supports extracting year, month, day, hour, minute,
/// second, day of week, and day of year components.
/// </summary>
public sealed class DatePartFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatePartFunction"/> class
    /// with a signature that accepts a date string and part name, returning a number.
    /// </summary>
    public DatePartFunction() : base(FunctionSignatures.Binary("DatePart", ParameterType.String, ParameterType.String, ParameterType.Number))
    {
    }

    /// <summary>
    /// Executes the date part extraction operation on the specified date string.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The date string to parse (JyroString)
    /// - arguments[1]: The part name to extract (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing the requested date component value.
    /// </returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the date string cannot be parsed or the part name is invalid.
    /// Valid part names are: year, month, day, hour, minute, second, dayofweek, dayofyear.
    /// </exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var dateStringArgument = GetArgument<JyroString>(arguments, 0);
        var partNameArgument = GetArgument<JyroString>(arguments, 1);

        if (!DateTime.TryParse(dateStringArgument.Value, out var parsedDate))
        {
            throw new JyroRuntimeException($"Invalid date format: '{dateStringArgument.Value}'");
        }

        var normalizedPartName = partNameArgument.Value.ToLowerInvariant();

        var extractedValue = normalizedPartName switch
        {
            "year" => parsedDate.Year,
            "month" => parsedDate.Month,
            "day" => parsedDate.Day,
            "hour" => parsedDate.Hour,
            "minute" => parsedDate.Minute,
            "second" => parsedDate.Second,
            "dayofweek" => (int)parsedDate.DayOfWeek,
            "dayofyear" => parsedDate.DayOfYear,
            _ => throw new JyroRuntimeException($"Invalid date part: '{normalizedPartName}'. Valid parts: year, month, day, hour, minute, second, dayofweek, dayofyear")
        };

        return new JyroNumber(extractedValue);
    }
}