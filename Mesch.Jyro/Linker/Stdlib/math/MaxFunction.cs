namespace Mesch.Jyro;

/// <summary>
/// Returns the maximum numeric value from a variable number of arguments.
/// Accepts multiple numeric values and determines the largest value among them.
/// Non-numeric arguments are ignored during the comparison process.
/// </summary>
public sealed class MaxFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MaxFunction"/> class
    /// with a variadic signature that accepts multiple numeric arguments.
    /// </summary>
    public MaxFunction() : base(FunctionSignatures.Variadic("Max", ParameterType.Number, ParameterType.Number))
    {
    }

    /// <summary>
    /// Executes the maximum value calculation across all provided numeric arguments.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments containing numeric values to compare.
    /// Non-numeric arguments are ignored during processing.
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing the maximum value found among the numeric arguments,
    /// or <see cref="JyroNull.Instance"/> if no numeric arguments are provided.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var maximumValue = double.MinValue;
        var foundNumericValue = false;

        foreach (var argumentValue in arguments)
        {
            if (argumentValue is JyroNumber numericArgument)
            {
                maximumValue = Math.Max(maximumValue, numericArgument.Value);
                foundNumericValue = true;
            }
        }

        return foundNumericValue ? new JyroNumber(maximumValue) : JyroNull.Instance;
    }
}