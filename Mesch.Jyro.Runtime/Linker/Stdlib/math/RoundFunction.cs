namespace Mesch.Jyro;

/// <summary>
/// Rounds a numeric value to a specified number of decimal places using
/// standard mathematical rounding rules (round half to even). Supports
/// both integer and decimal precision specification with an optional
/// digits parameter that defaults to zero decimal places.
/// </summary>
public sealed class RoundFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoundFunction"/> class
    /// with a signature that accepts a number and optional decimal places parameter.
    /// </summary>
    public RoundFunction() : base(new JyroFunctionSignature(
        "Round",
        new[] {
            new Parameter("value", ParameterType.Number),
            new Parameter("digits", ParameterType.Number, isOptionalParameter: true)
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
    /// </param>
    /// <param name="executionContext">The execution context (not used by this function).</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing the rounded value. Uses .NET's
    /// Math.Round implementation with banker's rounding (round half to even).
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var inputNumber = GetArgument<JyroNumber>(arguments, 0);
        var digitsArgument = GetOptionalArgument<JyroNumber>(arguments, 1);

        var decimalPlaces = digitsArgument?.IsInteger == true ? digitsArgument.ToInteger() : 0;
        var roundedValue = Math.Round(inputNumber.Value, decimalPlaces);
        return new JyroNumber(roundedValue);
    }
}