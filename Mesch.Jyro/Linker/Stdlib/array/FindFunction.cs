namespace Mesch.Jyro;

/// <summary>
/// Finds the first element in an array where a specified field satisfies a comparison condition.
/// Short-circuits on first match for efficiency. Returns null if no match is found.
/// Supports nested field paths using dot notation (e.g., "address.city") and comparison operators
/// (==, !=, &lt;, &lt;=, &gt;, &gt;=).
/// </summary>
public sealed class FindFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FindFunction"/> class
    /// with a signature that accepts an array, field name, comparison operator, and value.
    /// </summary>
    public FindFunction() : base(new JyroFunctionSignature(
        "Find",
        [
            new Parameter("array", ParameterType.Array),
            new Parameter("fieldName", ParameterType.String),
            new Parameter("operator", ParameterType.String),
            new Parameter("value", ParameterType.Any)
        ],
        ParameterType.Any))
    {
    }

    /// <summary>
    /// Executes the find operation on the array of elements.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array of elements to search (JyroArray)
    /// - arguments[1]: The field name or path to compare (JyroString). Supports nested paths with dot notation.
    /// - arguments[2]: The comparison operator (JyroString): "==", "!=", "&lt;", "&lt;=", "&gt;", "&gt;="
    /// - arguments[3]: The value to compare against (any JyroValue type)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// The first element that matches the condition, or <see cref="JyroNull.Instance"/> if no match is found.
    /// Non-object elements are skipped during the search.
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

            // Handle null field values
            if (fieldValue is JyroNull)
            {
                // For != operator with non-null compare value, null field matches
                if (operatorString == "!=" && !compareValue.IsNull)
                {
                    return item;
                }
                // For == operator with null compare value, null field matches
                if (operatorString == "==" && compareValue.IsNull)
                {
                    return item;
                }
                continue;
            }

            // Evaluate the comparison - return first match
            if (EvaluateComparison(fieldValue, operatorString, compareValue))
            {
                return item;
            }
        }

        // No match found
        return JyroNull.Instance;
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
    /// Compares two JyroValues for relational operators.
    /// Returns negative if left &lt; right, zero if equal, positive if left &gt; right.
    /// </summary>
    private static int CompareValues(JyroValue left, JyroValue right)
    {
        if (left is JyroNumber leftNumber && right is JyroNumber rightNumber)
        {
            return leftNumber.Value.CompareTo(rightNumber.Value);
        }

        if (left is JyroString leftString && right is JyroString rightString)
        {
            return string.Compare(leftString.Value, rightString.Value, StringComparison.Ordinal);
        }

        if (left is JyroBoolean leftBoolean && right is JyroBoolean rightBoolean)
        {
            return leftBoolean.Value.CompareTo(rightBoolean.Value);
        }

        throw new JyroRuntimeException(
            $"Cannot compare values of incompatible types: {left.GetType().Name} and {right.GetType().Name}. " +
            $"Relational operators (<, <=, >, >=) require both values to be numbers, strings, or booleans of the same type.");
    }
}
