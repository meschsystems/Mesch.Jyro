namespace Mesch.Jyro;

/// <summary>
/// Returns the minimum numeric value from a variable number of arguments.
/// Accepts multiple numeric values and determines the smallest value among them.
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
    /// The function arguments containing numeric values to compare.
    /// Non-numeric arguments are ignored during processing.
    /// </param>
    /// <param name="executionContext">The execution context (not used by this function).</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing the minimum value found among the numeric arguments,
    /// or <see cref="JyroNull.Instance"/> if no numeric arguments are provided.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var minimumValue = double.MaxValue;
        var foundNumericValue = false;

        foreach (var argumentValue in arguments)
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