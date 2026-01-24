namespace Mesch.Jyro;

/// <summary>
/// Rounds a numeric value to a specified number of decimal places using
/// configurable rounding rules. Supports banker's rounding (default),
/// floor, ceiling, and away-from-zero modes. Both digits and mode
/// parameters are optional, defaulting to zero decimal places and
/// banker's rounding respectively.
/// </summary>
public sealed class RoundFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoundFunction"/> class
    /// with a signature that accepts a number, optional decimal places, and optional rounding mode.
    /// </summary>
    public RoundFunction() : base(new JyroFunctionSignature(
        "Round",
        new[] {
            new Parameter("value", ParameterType.Number),
            new Parameter("digits", ParameterType.Number, isOptionalParameter: true),
            new Parameter("mode", ParameterType.String, isOptionalParameter: true)
        },
        ParameterType.Number))
    {
    }

    /// <summary>
    /// Executes the rounding operation on the specified numeric value.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The numeric value to round (JyroNumber)
    /// - arguments[1]: Optional number of decimal places (JyroNumber, must be integer, defaults to 0)
    /// - arguments[2]: Optional rounding mode (JyroString: "floor", "ceiling", "away", defaults to banker's rounding)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing the rounded value using the specified rounding mode.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var inputNumber = GetArgument<JyroNumber>(arguments, 0);
        var digitsArgument = GetOptionalArgument<JyroNumber>(arguments, 1);
        var modeArgument = GetOptionalArgument<JyroString>(arguments, 2);

        var decimalPlaces = digitsArgument?.IsInteger == true ? digitsArgument.ToInteger() : 0;
        var mode = modeArgument?.Value?.ToLowerInvariant() ?? string.Empty;

        double roundedValue;

        switch (mode)
        {
            case "floor":
                // Round toward negative infinity
                var floorMultiplier = Math.Pow(10, decimalPlaces);
                roundedValue = Math.Floor(inputNumber.Value * floorMultiplier) / floorMultiplier;
                break;

            case "ceiling":
                // Round toward positive infinity
                var ceilingMultiplier = Math.Pow(10, decimalPlaces);
                roundedValue = Math.Ceiling(inputNumber.Value * ceilingMultiplier) / ceilingMultiplier;
                break;

            case "away":
                // Round away from zero (standard mathematical rounding)
                roundedValue = Math.Round(inputNumber.Value, decimalPlaces, MidpointRounding.AwayFromZero);
                break;

            default:
                // Default: banker's rounding (round half to even)
                roundedValue = Math.Round(inputNumber.Value, decimalPlaces, MidpointRounding.ToEven);
                break;
        }

        return new JyroNumber(roundedValue);
    }
}