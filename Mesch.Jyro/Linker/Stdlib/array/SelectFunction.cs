namespace Mesch.Jyro;

/// <summary>
/// Extracts a single field value from each object in an array, returning an array of those values.
/// Supports nested field paths using dot notation (e.g., "address.city").
/// Non-object elements result in null values in the output. Objects missing the specified field
/// also return null for that position.
/// </summary>
public sealed class SelectFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectFunction"/> class
    /// with a signature that accepts an array and a field name.
    /// </summary>
    public SelectFunction() : base(new JyroFunctionSignature(
        "Select",
        [
            new Parameter("array", ParameterType.Array),
            new Parameter("fieldName", ParameterType.String)
        ],
        ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the field extraction operation on the array of elements.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array of elements to extract from (JyroArray)
    /// - arguments[1]: The field name or path to extract (JyroString). Supports nested paths with dot notation.
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A new <see cref="JyroArray"/> containing the extracted field values from each object.
    /// Non-object elements and missing fields result in null values at those positions.
    /// Empty arrays return an empty JyroArray.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var array = GetArrayArgument(arguments, 0);
        var fieldName = GetStringArgument(arguments, 1);

        var result = new JyroArray();

        for (int i = 0; i < array.Length; i++)
        {
            var item = array[i];

            if (item is JyroObject obj)
            {
                // GetProperty supports dot notation for nested paths
                var fieldValue = obj.GetProperty(fieldName);
                result.Add(fieldValue);
            }
            else
            {
                // Non-object elements contribute null
                result.Add(JyroNull.Instance);
            }
        }

        return result;
    }
}
