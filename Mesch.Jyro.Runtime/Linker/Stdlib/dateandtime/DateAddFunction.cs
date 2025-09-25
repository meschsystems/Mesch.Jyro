namespace Mesch.Jyro;

/// <summary>
/// Adds a specified amount of time to a date, supporting various time units
/// including days, weeks, months, years, hours, minutes, and seconds.
/// Returns the modified date as an ISO 8601 formatted string.
/// </summary>
public sealed class DateAddFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DateAddFunction"/> class
    /// with a signature that accepts a date string, time unit, and amount.
    /// </summary>
    public DateAddFunction() : base(new JyroFunctionSignature(
        "DateAdd",
        new[] {
            new Parameter("date", ParameterType.String),
            new Parameter("unit", ParameterType.String),
            new Parameter("amount", ParameterType.Number)
        },
        ParameterType.String))
    {
    }

    /// <summary>
    /// Executes the date addition operation by adding the specified time amount to the date.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The base date string to modify (JyroString)
    /// - arguments[1]: The time unit for the addition (JyroString)
    /// - arguments[2]: The amount to add (JyroNumber, must be integer)
    /// </param>
    /// <param name="executionContext">The execution context (not used by this function).</param>
    /// <returns>
    /// A <see cref="JyroString"/> containing the modified date in ISO 8601 format.
    /// </returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the date string cannot be parsed, the amount is not an integer,
    /// or the time unit is invalid. Valid units are: days, weeks, months, years,
    /// hours, minutes, seconds (singular and plural forms accepted).
    /// </exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var dateStringArgument = GetArgument<JyroString>(arguments, 0);
        var timeUnitArgument = GetArgument<JyroString>(arguments, 1);
        var amountArgument = GetArgument<JyroNumber>(arguments, 2);

        if (!DateTime.TryParse(dateStringArgument.Value, out var baseDate))
        {
            throw new JyroRuntimeException($"Invalid date format: '{dateStringArgument.Value}'");
        }

        if (!amountArgument.IsInteger)
        {
            throw new JyroRuntimeException("DateAdd() amount must be an integer");
        }

        var timeAmount = amountArgument.ToInteger();
        var normalizedTimeUnit = timeUnitArgument.Value.ToLowerInvariant();

        var modifiedDate = normalizedTimeUnit switch
        {
            "days" or "day" => baseDate.AddDays(timeAmount),
            "weeks" or "week" => baseDate.AddDays(timeAmount * 7),
            "months" or "month" => baseDate.AddMonths(timeAmount),
            "years" or "year" => baseDate.AddYears(timeAmount),
            "hours" or "hour" => baseDate.AddHours(timeAmount),
            "minutes" or "minute" => baseDate.AddMinutes(timeAmount),
            "seconds" or "second" => baseDate.AddSeconds(timeAmount),
            _ => throw new JyroRuntimeException($"Invalid date unit: '{normalizedTimeUnit}'. Valid units: days, weeks, months, years, hours, minutes, seconds")
        };

        var formattedResult = modifiedDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        return new JyroString(formattedResult);
    }
}