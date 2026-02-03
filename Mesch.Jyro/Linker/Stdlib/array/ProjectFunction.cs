namespace Mesch.Jyro;

/// <summary>
/// Creates new objects containing only the specified fields from each object in the source array.
/// Supports nested field paths using dot notation (e.g., "address.city").
/// Non-object elements are skipped. Missing fields result in null values in the projected object.
/// </summary>
public sealed class ProjectFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectFunction"/> class
    /// with a signature that accepts an array and an array of field names.
    /// </summary>
    public ProjectFunction() : base(new JyroFunctionSignature(
        "Project",
        [
            new Parameter("array", ParameterType.Array),
            new Parameter("fieldNames", ParameterType.Array)
        ],
        ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the projection operation on the array of elements.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array of objects to project from (JyroArray)
    /// - arguments[1]: An array of field names to include in the projection (JyroArray of JyroString).
    ///   Supports nested paths with dot notation.
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A new <see cref="JyroArray"/> containing new objects with only the specified fields.
    /// Non-object elements in the source array are skipped. Missing fields result in null values.
    /// Empty arrays return an empty JyroArray.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var array = GetArrayArgument(arguments, 0);
        var fieldNamesArray = GetArrayArgument(arguments, 1);

        // Extract field names as strings
        var fieldNames = new List<string>();
        for (int i = 0; i < fieldNamesArray.Length; i++)
        {
            var fieldNameValue = fieldNamesArray[i];
            if (fieldNameValue is JyroString fieldNameString)
            {
                fieldNames.Add(fieldNameString.Value);
            }
            // Non-string field names are skipped
        }

        var result = new JyroArray();

        for (int i = 0; i < array.Length; i++)
        {
            var item = array[i];

            if (item is not JyroObject sourceObj)
            {
                continue;
            }

            // Create a new object with only the specified fields
            var projectedObj = new JyroObject();

            foreach (var fieldName in fieldNames)
            {
                // GetProperty supports dot notation for nested paths
                var fieldValue = sourceObj.GetProperty(fieldName);

                // Use the leaf field name as the key in the projected object
                // For nested paths like "address.city", use "city" as the key
                var keyName = GetLeafFieldName(fieldName);
                projectedObj.SetProperty(keyName, fieldValue);
            }

            result.Add(projectedObj);
        }

        return result;
    }

    /// <summary>
    /// Gets the leaf field name from a potentially nested path.
    /// For "address.city", returns "city". For "name", returns "name".
    /// </summary>
    private static string GetLeafFieldName(string fieldPath)
    {
        var lastDotIndex = fieldPath.LastIndexOf('.');
        return lastDotIndex >= 0 ? fieldPath.Substring(lastDotIndex + 1) : fieldPath;
    }
}
