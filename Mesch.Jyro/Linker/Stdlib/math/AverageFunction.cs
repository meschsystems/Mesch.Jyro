namespace Mesch.Jyro;

/// <summary>
/// Calculates the arithmetic mean (average) of all numeric arguments provided.
/// Accepts a variable number of numeric values or a single array and computes their average,
/// ignoring non-numeric arguments during the calculation process.
/// Returns null if no numeric arguments are provided.
/// </summary>
public sealed class AverageFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AverageFunction"/> class
    /// with a variadic signature that accepts multiple numeric arguments.
    /// </summary>
    public AverageFunction() : base(FunctionSignatures.Variadic("Average", ParameterType.Any, ParameterType.Number, 0))
    {
    }

    /// <summary>
    /// Executes the average calculation across all provided numeric arguments.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments containing numeric values to average, or a single array of numeric values.
    /// Non-numeric arguments are ignored during the calculation.
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing the arithmetic mean of all numeric arguments.
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

        var sum = 0.0;
        var count = 0;

        foreach (var argumentValue in valuesToProcess)
        {
            if (argumentValue is JyroNumber numericArgument)
            {
                sum += numericArgument.Value;
                count++;
            }
        }

        if (count == 0)
        {
            return JyroNull.Instance;
        }

        return new JyroNumber(sum / count);
    }
}
