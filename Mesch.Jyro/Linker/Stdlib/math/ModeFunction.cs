namespace Mesch.Jyro;

/// <summary>
/// Finds the mode (most frequently occurring value) of all numeric arguments.
/// If multiple values have the same highest frequency, returns the first one
/// encountered in the argument list. Ignores non-numeric arguments.
/// Returns zero if no numeric arguments are provided.
/// </summary>
public sealed class ModeFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModeFunction"/> class
    /// with a variadic signature that accepts multiple numeric arguments.
    /// </summary>
    public ModeFunction() : base(FunctionSignatures.Variadic("Mode", ParameterType.Number, ParameterType.Number))
    {
    }

    /// <summary>
    /// Executes the mode calculation across all provided numeric arguments.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments containing numeric values to find the mode of.
    /// Non-numeric arguments are ignored during the calculation.
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing the mode of all numeric arguments.
    /// Returns zero if no numeric arguments are provided.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var frequencyMap = new Dictionary<double, int>();
        var orderedValues = new List<double>();

        foreach (var argumentValue in arguments)
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
            return new JyroNumber(0);
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
