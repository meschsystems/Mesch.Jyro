namespace Mesch.Jyro;

/// <summary>
/// Filters an array of objects, returning only elements where a specified field satisfies a comparison condition.
/// Supports nested field paths using dot notation (e.g., "address.city") and comparison operators
/// (==, !=, &lt;, &lt;=, &gt;, &gt;=) for flexible filtering logic.
/// </summary>
public sealed class FilterFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilterFunction"/> class
    /// with a signature that accepts an array, field name, comparison operator, and value.
    /// </summary>
    public FilterFunction() : base(new JyroFunctionSignature(
        "Filter",
        [
            new Parameter("array", ParameterType.Array),
            new Parameter("fieldName", ParameterType.String),
            new Parameter("operator", ParameterType.String),
            new Parameter("value", ParameterType.Any)
        ],
        ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the filtering operation on the array of elements.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array of elements to filter (JyroArray)
    /// - arguments[1]: The field name or path to compare (JyroString). Supports nested paths with dot notation.
    /// - arguments[2]: The comparison operator (JyroString): "==", "!=", "&lt;", "&lt;=", "&gt;", "&gt;="
    /// - arguments[3]: The value to compare against (any JyroValue type)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A new <see cref="JyroArray"/> containing only the elements where the specified
    /// field satisfies the comparison condition. Non-object elements and elements with missing
    /// fields are excluded from the result. Empty arrays or arrays with no matches return
    /// an empty JyroArray.
    /// </returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when an unsupported comparison operator is provided or when comparison
    /// operations are attempted on incompatible types.
    /// </exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var array = GetArrayArgument(arguments, 0);
        var fieldName = GetStringArgument(arguments, 1);
        var operatorString = GetStringArgument(arguments, 2);
        var compareValue = arguments[3];

        var filteredResult = new JyroArray();

        for (int i = 0; i < array.Length; i++)
        {
            var item = array[i];

            // Skip non-object items
            if (item is not JyroObject obj)
            {
                continue;
            }

            // Get the field value using nested path support from GetProperty
            var fieldValue = obj.GetProperty(fieldName);

            // For != operator, include items where field is missing (null)
            // For other operators, skip items where field is missing
            if (fieldValue is JyroNull)
            {
                if (operatorString == "!=" && !compareValue.IsNull)
                {
                    filteredResult.Add(item);
                }
                continue;
            }

            // Evaluate the comparison
            if (EvaluateComparison(fieldValue, operatorString, compareValue))
            {
                filteredResult.Add(item);
            }
        }

        return filteredResult;
    }

    /// <summary>
    /// Evaluates a comparison between two JyroValues using the specified operator.
    /// </summary>
    private static bool EvaluateComparison(JyroValue left, string operatorString, JyroValue right)
    {
        switch (operatorString)
        {
            case "==":
                return left.Equals(right);

            case "!=":
                return !left.Equals(right);

            case "<":
                return CompareValues(left, right) < 0;

            case "<=":
                return CompareValues(left, right) <= 0;

            case ">":
                return CompareValues(left, right) > 0;

            case ">=":
                return CompareValues(left, right) >= 0;

            default:
                throw new JyroRuntimeException($"Unsupported comparison operator: '{operatorString}'. Supported operators are: ==, !=, <, <=, >, >=");
        }
    }

    /// <summary>
    /// Compares two JyroValues for relational operators (&lt;, &lt;=, &gt;, &gt;=).
    /// Returns negative if left &lt; right, zero if equal, positive if left &gt; right.
    /// </summary>
    private static int CompareValues(JyroValue left, JyroValue right)
    {
        // Number comparison
        if (left is JyroNumber leftNumber && right is JyroNumber rightNumber)
        {
            return leftNumber.Value.CompareTo(rightNumber.Value);
        }

        // String comparison (case-sensitive, ordinal)
        if (left is JyroString leftString && right is JyroString rightString)
        {
            return string.Compare(leftString.Value, rightString.Value, StringComparison.Ordinal);
        }

        // Boolean comparison (false < true)
        if (left is JyroBoolean leftBoolean && right is JyroBoolean rightBoolean)
        {
            return leftBoolean.Value.CompareTo(rightBoolean.Value);
        }

        // Incompatible types for comparison
        throw new JyroRuntimeException(
            $"Cannot compare values of incompatible types: {left.GetType().Name} and {right.GetType().Name}. " +
            $"Relational operators (<, <=, >, >=) require both values to be numbers, strings, or booleans of the same type.");
    }
}
