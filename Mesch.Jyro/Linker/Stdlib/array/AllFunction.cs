namespace Mesch.Jyro;

/// <summary>
/// Checks if all elements in an array satisfy a comparison condition on a specified field.
/// Short-circuits on first non-match for efficiency. Returns true for empty arrays (vacuous truth).
/// Supports nested field paths using dot notation (e.g., "address.city") and comparison operators
/// (==, !=, &lt;, &lt;=, &gt;, &gt;=).
/// </summary>
public sealed class AllFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AllFunction"/> class
    /// with a signature that accepts an array, field name, comparison operator, and value.
    /// </summary>
    public AllFunction() : base(new JyroFunctionSignature(
        "All",
        [
            new Parameter("array", ParameterType.Array),
            new Parameter("fieldName", ParameterType.String),
            new Parameter("operator", ParameterType.String),
            new Parameter("value", ParameterType.Any)
        ],
        ParameterType.Boolean))
    {
    }

    /// <summary>
    /// Executes the all-match check on the array of elements.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array of elements to check (JyroArray)
    /// - arguments[1]: The field name or path to compare (JyroString). Supports nested paths with dot notation.
    /// - arguments[2]: The comparison operator (JyroString): "==", "!=", "&lt;", "&lt;=", "&gt;", "&gt;="
    /// - arguments[3]: The value to compare against (any JyroValue type)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// <see cref="JyroBoolean.True"/> if all elements match the condition, <see cref="JyroBoolean.False"/> otherwise.
    /// Returns true for empty arrays (vacuous truth). Non-object elements cause the function to return false.
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

            // Non-object items fail the "all" check
            if (item is not JyroObject obj)
            {
                return JyroBoolean.False;
            }

            // Get the field value using nested path support from GetProperty
            var fieldValue = obj.GetProperty(fieldName);

            // Handle null field values
            if (fieldValue is JyroNull)
            {
                // For != operator with non-null compare value, null field matches (continue)
                if (operatorString == "!=" && !compareValue.IsNull)
                {
                    continue;
                }
                // For == operator with null compare value, null field matches (continue)
                if (operatorString == "==" && compareValue.IsNull)
                {
                    continue;
                }
                // Otherwise, null field fails the condition
                return JyroBoolean.False;
            }

            // Evaluate the comparison - short-circuit on first non-match
            if (!EvaluateComparison(fieldValue, operatorString, compareValue))
            {
                return JyroBoolean.False;
            }
        }

        // All elements matched (or array was empty - vacuous truth)
        return JyroBoolean.True;
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
