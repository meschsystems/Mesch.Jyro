namespace Mesch.Jyro;

/// <summary>
/// Calculates the median (middle value) of all numeric arguments provided.
/// Accepts a variable number of numeric values or a single array.
/// For an odd count of numbers, returns the middle value when sorted.
/// For an even count, returns the average of the two middle values.
/// Ignores non-numeric arguments during the calculation.
/// Returns null if no numeric arguments are provided.
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
    /// The function arguments containing numeric values to find the median of, or a single array of numeric values.
    /// Non-numeric arguments are ignored during the calculation.
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing the median of all numeric arguments.
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

        var numbers = new List<double>();

        foreach (var argumentValue in valuesToProcess)
        {
            if (argumentValue is JyroNumber numericArgument)
            {
                numbers.Add(numericArgument.Value);
            }
        }

        if (numbers.Count == 0)
        {
            return JyroNull.Instance;
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
