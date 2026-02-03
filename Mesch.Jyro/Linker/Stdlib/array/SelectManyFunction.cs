namespace Mesch.Jyro;

/// <summary>
/// Extracts an array field from each object in the source array and flattens all results into a single array.
/// Supports nested field paths using dot notation (e.g., "metadata.tags").
/// Non-object elements, missing fields, and non-array field values are skipped.
/// </summary>
public sealed class SelectManyFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectManyFunction"/> class
    /// with a signature that accepts an array and a field name.
    /// </summary>
    public SelectManyFunction() : base(new JyroFunctionSignature(
        "SelectMany",
        [
            new Parameter("array", ParameterType.Array),
            new Parameter("fieldName", ParameterType.String)
        ],
        ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the field extraction and flattening operation on the array of elements.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array of elements to extract from (JyroArray)
    /// - arguments[1]: The field name or path to extract (JyroString). Supports nested paths with dot notation.
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A new <see cref="JyroArray"/> containing all elements from the extracted array fields,
    /// flattened into a single array. Non-object elements, missing fields, and non-array
    /// field values are skipped. Empty arrays return an empty JyroArray.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var array = GetArrayArgument(arguments, 0);
        var fieldName = GetStringArgument(arguments, 1);

        var result = new JyroArray();

        for (int i = 0; i < array.Length; i++)
        {
            var item = array[i];

            if (item is not JyroObject obj)
            {
                continue;
            }

            // GetProperty supports dot notation for nested paths
            var fieldValue = obj.GetProperty(fieldName);

            if (fieldValue is JyroArray nestedArray)
            {
                // Flatten the nested array into the result
                foreach (var nestedItem in nestedArray)
                {
                    result.Add(nestedItem);
                }
            }
            // Non-array field values are skipped
        }

        return result;
    }
}
