namespace Mesch.Jyro;

/// <summary>
/// Calculates the arithmetic mean (average) of all numeric arguments provided.
/// Accepts a variable number of numeric values and computes their average,
/// ignoring non-numeric arguments during the calculation process.
/// Returns zero if no numeric arguments are provided.
/// </summary>
public sealed class AverageFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AverageFunction"/> class
    /// with a variadic signature that accepts multiple numeric arguments.
    /// </summary>
    public AverageFunction() : base(FunctionSignatures.Variadic("Average", ParameterType.Number, ParameterType.Number))
    {
    }

    /// <summary>
    /// Executes the average calculation across all provided numeric arguments.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments containing numeric values to average.
    /// Non-numeric arguments are ignored during the calculation.
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing the arithmetic mean of all numeric arguments.
    /// Returns zero if no numeric arguments are provided.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var sum = 0.0;
        var count = 0;

        foreach (var argumentValue in arguments)
        {
            if (argumentValue is JyroNumber numericArgument)
            {
                sum += numericArgument.Value;
                count++;
            }
        }

        if (count == 0)
        {
            return new JyroNumber(0);
        }

        return new JyroNumber(sum / count);
    }
}
