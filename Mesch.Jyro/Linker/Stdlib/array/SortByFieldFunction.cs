namespace Mesch.Jyro;

/// <summary>
/// Sorts an array of objects by a specified field in ascending or descending order.
/// Supports sorting by numeric and string properties with proper type-aware comparison.
/// Non-object elements are treated as equal in the sort order, and missing properties
/// are handled gracefully during the comparison process.
/// </summary>
public sealed class SortByFieldFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SortByFieldFunction"/> class
    /// with a signature that accepts an array, field name, and sort direction.
    /// </summary>
    public SortByFieldFunction() : base(new JyroFunctionSignature(
        "SortByField",
        [
            new Parameter("array", ParameterType.Array),
            new Parameter("fieldName", ParameterType.String),
            new Parameter("direction", ParameterType.String)
        ],
        ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the field-based sorting operation on the array of objects.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array of objects to sort (JyroArray)
    /// - arguments[1]: The field name to sort by (JyroString)
    /// - arguments[2]: Sort direction - "asc" for ascending or "desc" for descending (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A new <see cref="JyroArray"/> containing the sorted elements. Objects are compared
    /// by the specified field value using appropriate type-specific comparison logic.
    /// Non-object elements maintain their relative positions.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var sourceArray = GetArrayArgument(arguments, 0);
        var fieldNameArgument = GetArgument<JyroString>(arguments, 1);
        var sortDirectionArgument = GetArgument<JyroString>(arguments, 2);

        var arrayElements = sourceArray.ToList();
        var isDescendingOrder = string.Equals(sortDirectionArgument.Value, "desc", StringComparison.OrdinalIgnoreCase);

        arrayElements.Sort((firstElement, secondElement) =>
        {
            if (firstElement is not JyroObject firstObject || secondElement is not JyroObject secondObject)
            {
                return 0;
            }

            var firstFieldValue = firstObject.GetProperty(fieldNameArgument.Value);
            var secondFieldValue = secondObject.GetProperty(fieldNameArgument.Value);

            int comparisonResult = 0;
            if (firstFieldValue is JyroNumber firstNumber && secondFieldValue is JyroNumber secondNumber)
            {
                comparisonResult = firstNumber.Value.CompareTo(secondNumber.Value);
            }
            else if (firstFieldValue is JyroString firstString && secondFieldValue is JyroString secondString)
            {
                comparisonResult = string.Compare(firstString.Value, secondString.Value, StringComparison.Ordinal);
            }

            return isDescendingOrder ? -comparisonResult : comparisonResult;
        });

        var sortedResult = new JyroArray();
        foreach (var sortedElement in arrayElements)
        {
            sortedResult.Add(sortedElement);
        }

        return sortedResult;
    }
}