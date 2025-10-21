namespace Mesch.Jyro;

/// <summary>
/// Calculates the sum of all numeric arguments provided to the function.
/// Accepts a variable number of numeric values and computes their total,
/// ignoring non-numeric arguments during the calculation process.
/// Returns zero if no numeric arguments are provided.
/// </summary>
public sealed class SumFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SumFunction"/> class
    /// with a variadic signature that accepts multiple numeric arguments.
    /// </summary>
    public SumFunction() : base(FunctionSignatures.Variadic("Sum", ParameterType.Number, ParameterType.Number))
    {
    }

    /// <summary>
    /// Executes the summation operation across all provided numeric arguments.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments containing numeric values to sum.
    /// Non-numeric arguments are ignored during the calculation.
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing the total sum of all numeric arguments.
    /// Returns zero if no numeric arguments are provided.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var runningTotal = 0.0;

        foreach (var argumentValue in arguments)
        {
            if (argumentValue is JyroNumber numericArgument)
            {
                runningTotal += numericArgument.Value;
            }
        }

        return new JyroNumber(runningTotal);
    }
}