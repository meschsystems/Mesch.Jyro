namespace Mesch.Jyro;

/// <summary>
/// Finds the mode (most frequently occurring value) of all numeric arguments.
/// Accepts a variable number of numeric values or a single array.
/// If multiple values have the same highest frequency, returns the first one
/// encountered in the argument list. Ignores non-numeric arguments.
/// Returns null if no numeric arguments are provided.
/// </summary>
public sealed class ModeFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModeFunction"/> class
    /// with a variadic signature that accepts multiple numeric arguments.
    /// </summary>
    public ModeFunction() : base(FunctionSignatures.Variadic("Mode", ParameterType.Any, ParameterType.Number, 0))
    {
    }

    /// <summary>
    /// Executes the mode calculation across all provided numeric arguments.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments containing numeric values to find the mode of, or a single array of numeric values.
    /// Non-numeric arguments are ignored during the calculation.
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing the mode of all numeric arguments.
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

        var frequencyMap = new Dictionary<double, int>();
        var orderedValues = new List<double>();

        foreach (var argumentValue in valuesToProcess)
        {
            if (argumentValue is JyroNumber numericArgument)
            {
                var value = numericArgument.Value;

                if (!frequencyMap.ContainsKey(value))
                {
                    frequencyMap[value] = 0;
                    orderedValues.Add(value);
                }

                frequencyMap[value]++;
            }
        }

        if (orderedValues.Count == 0)
        {
            return JyroNull.Instance;
        }

        // Find the value with highest frequency, preferring earlier values on tie
        var maxFrequency = 0;
        var modeValue = orderedValues[0];

        foreach (var value in orderedValues)
        {
            if (frequencyMap[value] > maxFrequency)
            {
                maxFrequency = frequencyMap[value];
                modeValue = value;
            }
        }

        return new JyroNumber(modeValue);
    }
}
