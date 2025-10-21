namespace Mesch.Jyro;

/// <summary>
/// Converts a string representation of a number to a numeric value.
/// This function parses numeric strings using culture-invariant formatting
/// rules to ensure consistent number parsing behavior across different
/// system locales and cultural settings. If the string cannot be parsed
/// as a valid number, the function returns zero.
/// </summary>
public sealed class ToNumberFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ToNumberFunction"/> class
    /// with a signature that accepts a string and returns a numeric value.
    /// </summary>
    public ToNumberFunction() : base(FunctionSignatures.Unary("ToNumber", ParameterType.String, ParameterType.Number))
    {
    }

    /// <summary>
    /// Executes the string-to-number conversion operation on the specified string.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The string to convert to a number (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing the numeric value parsed from the input string
    /// using invariant culture rules. Returns zero if the string cannot be parsed as a valid number.
    /// </returns>
    /// <remarks>
    /// This function uses <see cref="double.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider?, out double)"/>
    /// with invariant culture settings to ensure
    /// consistent parsing behavior regardless of the system's regional settings. Common use cases
    /// include converting form input, query parameters, or user-provided text into numeric values
    /// for mathematical operations or database storage.
    /// </remarks>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var inputString = GetArgument<JyroString>(arguments, 0);

        if (double.TryParse(inputString.Value, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var result))
        {
            return new JyroNumber(result);
        }

        return new JyroNumber(0);
    }
}
