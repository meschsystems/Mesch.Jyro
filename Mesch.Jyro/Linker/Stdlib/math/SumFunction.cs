namespace Mesch.Jyro;

/// <summary>
/// Calculates the sum of all numeric arguments provided to the function.
/// Accepts a variable number of numeric values or a single array and computes their total,
/// ignoring non-numeric arguments during the calculation process.
/// Returns null if no numeric arguments are provided.
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
    /// The function arguments containing numeric values to sum, or a single array of numeric values.
    /// Non-numeric arguments are ignored during the calculation.
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing the total sum of all numeric arguments.
    /// Returns null if no numeric arguments are provided.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        // If a single array argument is passed, unpack it
        IEnumerable<JyroValue> valuesToProcess = arguments;
        if (arguments.Count == 1 && arguments[0] is JyroArray singleArray)
        {
            valuesToProcess = singleArray;
        }

        var runningTotal = 0.0;
        var foundNumericValue = false;

        foreach (var argumentValue in valuesToProcess)
        {
            if (argumentValue is JyroNumber numericArgument)
            {
                runningTotal += numericArgument.Value;
                foundNumericValue = true;
            }
        }

        return foundNumericValue ? new JyroNumber(runningTotal) : JyroNull.Instance;
    }
}