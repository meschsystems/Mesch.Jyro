namespace Mesch.Jyro;

/// <summary>
/// Returns the current date in UTC timezone as an ISO 8601 formatted date string.
/// Provides a consistent date representation without time components, suitable
/// for date-only operations and comparisons across different time zones.
/// </summary>
public sealed class TodayFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TodayFunction"/> class
    /// with a signature that accepts no parameters and returns a string.
    /// </summary>
    public TodayFunction() : base(new JyroFunctionSignature("Today", Array.Empty<Parameter>(), ParameterType.String))
    {
    }

    /// <summary>
    /// Executes the current date retrieval operation.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments (empty for this function).
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroString"/> containing the current UTC date
    /// in ISO 8601 date format (yyyy-MM-dd) without time components.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var currentUtcDate = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
        return new JyroString(currentUtcDate);
    }
}