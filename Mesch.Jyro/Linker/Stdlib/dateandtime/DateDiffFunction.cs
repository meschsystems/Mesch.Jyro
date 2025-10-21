namespace Mesch.Jyro;

/// <summary>
/// Calculates the difference between two dates in the specified time unit,
/// returning the numeric difference as a floating-point value. Supports
/// various time units including days, weeks, months, years, hours, minutes, and seconds.
/// The calculation is performed as (end date - start date), so positive values
/// indicate the end date is later than the start date.
/// </summary>
public sealed class DateDiffFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DateDiffFunction"/> class
    /// with a signature that accepts two date strings and a time unit.
    /// </summary>
    public DateDiffFunction() : base(new JyroFunctionSignature(
        "DateDiff",
        new[] {
            new Parameter("endDate", ParameterType.String),
            new Parameter("startDate", ParameterType.String),
            new Parameter("unit", ParameterType.String)
        },
        ParameterType.Number))
    {
    }

    /// <summary>
    /// Executes the date difference calculation between the specified dates.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The end date string (JyroString)
    /// - arguments[1]: The start date string (JyroString)
    /// - arguments[2]: The time unit for the result (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing the calculated time difference
    /// in the requested unit. Positive values indicate the end date is later.
    /// </returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when either date string cannot be parsed or the time unit is invalid.
    /// Valid units are: days, weeks, months, years, hours, minutes, seconds
    /// (singular and plural forms accepted). Month and year calculations use
    /// approximate values (30.44 days per month, 365.25 days per year).
    /// </exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var endDateArgument = GetArgument<JyroString>(arguments, 0);
        var startDateArgument = GetArgument<JyroString>(arguments, 1);
        var timeUnitArgument = GetArgument<JyroString>(arguments, 2);

        if (!DateTime.TryParse(endDateArgument.Value, out var endDate))
        {
            throw new JyroRuntimeException($"Invalid end date format: '{endDateArgument.Value}'");
        }

        if (!DateTime.TryParse(startDateArgument.Value, out var startDate))
        {
            throw new JyroRuntimeException($"Invalid start date format: '{startDateArgument.Value}'");
        }

        var timeDifference = endDate - startDate;
        var normalizedTimeUnit = timeUnitArgument.Value.ToLowerInvariant();

        var calculatedDifference = normalizedTimeUnit switch
        {
            "days" or "day" => timeDifference.TotalDays,
            "weeks" or "week" => timeDifference.TotalDays / 7.0,
            "hours" or "hour" => timeDifference.TotalHours,
            "minutes" or "minute" => timeDifference.TotalMinutes,
            "seconds" or "second" => timeDifference.TotalSeconds,
            "years" or "year" => timeDifference.TotalDays / 365.25,
            "months" or "month" => timeDifference.TotalDays / 30.44,
            _ => throw new JyroRuntimeException($"Invalid date unit: '{normalizedTimeUnit}'. Valid units: days, weeks, months, years, hours, minutes, seconds")
        };

        return new JyroNumber(calculatedDifference);
    }
}