namespace Mesch.Jyro;

/// <summary>
/// Removes duplicate values from an array, returning a new array containing
/// only unique elements. Uses deep equality comparison for determining
/// duplicates, preserving the order of first occurrence for each unique value.
/// </summary>
public sealed class DistinctFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DistinctFunction"/> class
    /// with a signature that accepts an array and returns an array of unique values.
    /// </summary>
    public DistinctFunction() : base(FunctionSignatures.Unary("Distinct", ParameterType.Array, ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the distinct operation on the specified array.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array to remove duplicates from (JyroArray)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A new <see cref="JyroArray"/> containing only unique elements from the
    /// input array. The order of first occurrence is preserved.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var inputArray = GetArgument<JyroArray>(arguments, 0);
        var resultArray = new JyroArray();

        foreach (var element in inputArray)
        {
            // Check if element already exists in result using deep equality
            var isDuplicate = false;
            foreach (var existingElement in resultArray)
            {
                if (element.Equals(existingElement))
                {
                    isDuplicate = true;
                    break;
                }
            }

            if (!isDuplicate)
            {
                resultArray.Add(element);
            }
        }

        return resultArray;
    }
}
