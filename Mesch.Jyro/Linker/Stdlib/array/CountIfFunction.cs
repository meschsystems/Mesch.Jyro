namespace Mesch.Jyro;

/// <summary>
/// Counts the number of elements in an array where a specified field equals a given value.
/// Supports nested field paths using dot notation (e.g., "address.city") and performs
/// case-sensitive comparison using Jyro's standard equality semantics.
/// </summary>
public sealed class CountIfFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CountIfFunction"/> class
    /// with a signature that accepts an array, field name, and comparison value.
    /// </summary>
    public CountIfFunction() : base(new JyroFunctionSignature(
        "CountIf",
        [
            new Parameter("array", ParameterType.Array),
            new Parameter("fieldName", ParameterType.String),
            new Parameter("value", ParameterType.Any)
        ],
        ParameterType.Number))
    {
    }

    /// <summary>
    /// Executes the counting operation on the array of elements.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array of elements to count (JyroArray)
    /// - arguments[1]: The field name or path to compare (JyroString). Supports nested paths with dot notation.
    /// - arguments[2]: The value to compare against (any JyroValue type)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> representing the count of elements where the specified
    /// field equals the comparison value. Non-object elements and elements with missing
    /// fields are skipped (not counted). Empty arrays return 0.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var array = GetArrayArgument(arguments, 0);
        var fieldName = GetStringArgument(arguments, 1);
        var compareValue = arguments[2];

        var count = 0;

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

            // Skip if field is missing (returns null)
            if (fieldValue is JyroNull)
            {
                continue;
            }

            // Use deep equality comparison (case-sensitive for strings)
            if (fieldValue.Equals(compareValue))
            {
                count++;
            }
        }

        return new JyroNumber(count);
    }
}
