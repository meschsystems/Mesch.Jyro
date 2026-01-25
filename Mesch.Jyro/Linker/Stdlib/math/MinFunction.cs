namespace Mesch.Jyro;

/// <summary>
/// Returns the minimum numeric value from a variable number of arguments.
/// Accepts multiple numeric values or a single array and determines the smallest value among them.
/// Non-numeric arguments are ignored during the comparison process.
/// </summary>
public sealed class MinFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MinFunction"/> class
    /// with a variadic signature that accepts multiple numeric arguments.
    /// </summary>
    public MinFunction() : base(FunctionSignatures.Variadic("Min", ParameterType.Number, ParameterType.Number))
    {
    }

    /// <summary>
    /// Executes the minimum value calculation across all provided numeric arguments.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments containing numeric values to compare, or a single array of numeric values.
    /// Non-numeric arguments are ignored during processing.
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing the minimum value found among the numeric arguments,
    /// or <see cref="JyroNull.Instance"/> if no numeric arguments are provided.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        // If a single array argument is passed, unpack it
        IEnumerable<JyroValue> valuesToProcess = arguments;
        if (arguments.Count == 1 && arguments[0] is JyroArray singleArray)
        {
            valuesToProcess = singleArray;
        }

        var minimumValue = double.MaxValue;
        var foundNumericValue = false;

        foreach (var argumentValue in valuesToProcess)
        {
            if (argumentValue is JyroNumber numericArgument)
            {
                minimumValue = Math.Min(minimumValue, numericArgument.Value);
                foundNumericValue = true;
            }
        }

        return foundNumericValue ? new JyroNumber(minimumValue) : JyroNull.Instance;
    }
}