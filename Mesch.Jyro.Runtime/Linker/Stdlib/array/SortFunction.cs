namespace Mesch.Jyro;

/// <summary>
/// Sorts an array in-place using type-aware comparison logic. Handles mixed-type
/// arrays by grouping like types together and applying appropriate comparison
/// rules for numbers, strings, and booleans. Null values are sorted to the
/// beginning of the array, followed by typed values in their natural order.
/// </summary>
public sealed class SortFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SortFunction"/> class
    /// with a signature that accepts an array and returns the same sorted array.
    /// </summary>
    public SortFunction() : base(FunctionSignatures.Unary("Sort", ParameterType.Array, ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the in-place sorting operation on the specified array.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array to sort (JyroArray)
    /// </param>
    /// <param name="executionContext">The execution context (not used by this function).</param>
    /// <returns>
    /// The same array instance after sorting. Elements are ordered with null values first,
    /// followed by numbers in ascending order, then strings in lexicographic order,
    /// then boolean values (false before true). Incomparable types maintain relative order.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var targetArray = GetArrayArgument(arguments, 0);
        var arrayElements = new List<JyroValue>();

        for (int elementIndex = 0; elementIndex < targetArray.Length; elementIndex++)
        {
            arrayElements.Add(targetArray[elementIndex]);
        }

        arrayElements.Sort((firstElement, secondElement) =>
        {
            if (firstElement.IsNull && secondElement.IsNull)
            {
                return 0;
            }

            if (firstElement.IsNull)
            {
                return -1;
            }

            if (secondElement.IsNull)
            {
                return 1;
            }

            if (firstElement is JyroNumber firstNumber && secondElement is JyroNumber secondNumber)
            {
                return firstNumber.Value.CompareTo(secondNumber.Value);
            }

            if (firstElement is JyroString firstString && secondElement is JyroString secondString)
            {
                return string.Compare(firstString.Value, secondString.Value, StringComparison.Ordinal);
            }

            if (firstElement is JyroBoolean firstBoolean && secondElement is JyroBoolean secondBoolean)
            {
                return firstBoolean.Value.CompareTo(secondBoolean.Value);
            }

            return 0;
        });

        targetArray.Clear();
        foreach (var sortedElement in arrayElements)
        {
            targetArray.Add(sortedElement);
        }

        return targetArray;
    }
}