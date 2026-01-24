namespace Mesch.Jyro;

/// <summary>
/// Calculates the median (middle value) of all numeric arguments provided.
/// For an odd count of numbers, returns the middle value when sorted.
/// For an even count, returns the average of the two middle values.
/// Ignores non-numeric arguments during the calculation.
/// Returns zero if no numeric arguments are provided.
/// </summary>
public sealed class MedianFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MedianFunction"/> class
    /// with a variadic signature that accepts multiple numeric arguments.
    /// </summary>
    public MedianFunction() : base(FunctionSignatures.Variadic("Median", ParameterType.Number, ParameterType.Number))
    {
    }

    /// <summary>
    /// Executes the median calculation across all provided numeric arguments.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments containing numeric values to find the median of.
    /// Non-numeric arguments are ignored during the calculation.
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing the median of all numeric arguments.
    /// Returns zero if no numeric arguments are provided.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var numbers = new List<double>();

        foreach (var argumentValue in arguments)
        {
            if (argumentValue is JyroNumber numericArgument)
            {
                numbers.Add(numericArgument.Value);
            }
        }

        if (numbers.Count == 0)
        {
            return new JyroNumber(0);
        }

        numbers.Sort();

        double median;
        var count = numbers.Count;
        var middleIndex = count / 2;

        if (count % 2 == 0)
        {
            // Even count: average the two middle values
            median = (numbers[middleIndex - 1] + numbers[middleIndex]) / 2.0;
        }
        else
        {
            // Odd count: return the middle value
            median = numbers[middleIndex];
        }

        return new JyroNumber(median);
    }
}
