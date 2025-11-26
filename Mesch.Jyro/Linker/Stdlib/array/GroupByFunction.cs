namespace Mesch.Jyro;

/// <summary>
/// Groups an array of objects by a specified field, returning an object where keys are
/// the distinct field values and values are arrays of items with that field value.
/// Supports nested field paths using dot notation (e.g., "address.city").
/// </summary>
public sealed class GroupByFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GroupByFunction"/> class
    /// with a signature that accepts an array and a field name.
    /// </summary>
    public GroupByFunction() : base(new JyroFunctionSignature(
        "GroupBy",
        [
            new Parameter("array", ParameterType.Array),
            new Parameter("fieldName", ParameterType.String)
        ],
        ParameterType.Object))
    {
    }

    /// <summary>
    /// Executes the grouping operation on the array of elements.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array of elements to group (JyroArray)
    /// - arguments[1]: The field name or path to group by (JyroString). Supports nested paths with dot notation.
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A new <see cref="JyroObject"/> where keys are the distinct field values (as strings)
    /// and values are <see cref="JyroArray"/> instances containing the items with that field value.
    /// Items with null or missing field values are grouped under the key "null".
    /// Non-object elements in the input array are skipped.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var array = GetArrayArgument(arguments, 0);
        var fieldName = GetStringArgument(arguments, 1);

        var result = new JyroObject();

        for (int i = 0; i < array.Length; i++)
        {
            var item = array[i];

            // Skip non-object items
            if (item is not JyroObject obj)
            {
                continue;
            }

            // Get the field value using nested path support
            var fieldValue = obj.GetProperty(fieldName);

            // Convert field value to string key (null values use "null" key)
            var groupKey = fieldValue is JyroNull ? "null" : fieldValue.ToStringValue();

            // Get or create the group array
            var existingGroup = result.GetProperty(groupKey);
            JyroArray groupArray;

            if (existingGroup is JyroArray arr)
            {
                groupArray = arr;
            }
            else
            {
                groupArray = new JyroArray();
                result.SetProperty(groupKey, groupArray);
            }

            // Add item to group
            groupArray.Add(item);
        }

        return result;
    }
}
