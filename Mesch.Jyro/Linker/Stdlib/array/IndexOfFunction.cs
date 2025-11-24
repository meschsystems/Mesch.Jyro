namespace Mesch.Jyro;

/// <summary>
/// Returns the index of the first element in an array that matches a specified value using deep equality comparison.
/// </summary>
/// <remarks>
/// <para>
/// The <c>IndexOf</c> function searches through an array and returns the zero-based index of the first element
/// that is deeply equal to the specified search value. Deep equality means that complex objects and arrays
/// are compared by their contents, not by reference.
/// </para>
/// <para>
/// If no matching element is found, the function returns -1.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var numbers = [1, 2, 3, 4, 5]
/// var idx = IndexOf(numbers, 3)  # Returns 2
///
/// var orders = [
///     { "id": "ORD-1001", "total": 100 },
///     { "id": "ORD-1002", "total": 200 },
///     { "id": "ORD-1003", "total": 150 }
/// ]
/// var orderIdx = IndexOf(orders, { "id": "ORD-1002", "total": 200 })  # Returns 1
///
/// var notFound = IndexOf(numbers, 10)  # Returns -1
/// </code>
/// </example>
public sealed class IndexOfFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IndexOfFunction"/> class.
    /// </summary>
    public IndexOfFunction() : base(FunctionSignatures.Binary(
        "IndexOf",
        ParameterType.Array,     // First parameter: array to search
        ParameterType.Any,       // Second parameter: value to find
        ParameterType.Number))   // Return type: index (number)
    {
    }

    /// <summary>
    /// Executes the IndexOf function to find the index of a value in an array.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments:
    /// <list type="bullet">
    /// <item><description>Argument 0: The array to search (JyroArray)</description></item>
    /// <item><description>Argument 1: The value to find (any JyroValue)</description></item>
    /// </list>
    /// </param>
    /// <param name="executionContext">The execution context for the function.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> representing the zero-based index of the first matching element,
    /// or -1 if the value is not found in the array.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var searchArray = GetArrayArgument(arguments, 0);
        var searchValue = arguments[1];

        for (var index = 0; index < searchArray.Length; index++)
        {
            var currentElement = searchArray[index];

            // Use deep equality comparison via JyroValue.Equals()
            if (currentElement.Equals(searchValue))
            {
                return new JyroNumber(index);
            }
        }

        // Not found
        return new JyroNumber(-1);
    }
}
