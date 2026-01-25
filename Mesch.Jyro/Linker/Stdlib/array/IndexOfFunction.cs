namespace Mesch.Jyro;

/// <summary>
/// Returns the index of the first occurrence of a value in a string or array using deep equality comparison.
/// </summary>
/// <remarks>
/// <para>
/// The <c>IndexOf</c> function is polymorphic and works with both strings and arrays:
/// </para>
/// <list type="bullet">
/// <item><description>For strings: Returns the zero-based index of the first occurrence of a substring.</description></item>
/// <item><description>For arrays: Returns the zero-based index of the first element that is deeply equal to the search value.</description></item>
/// </list>
/// <para>
/// Deep equality for arrays means that complex objects and arrays are compared by their contents, not by reference.
/// String comparison is case-sensitive using ordinal comparison semantics.
/// </para>
/// <para>
/// If no match is found, the function returns -1.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// # String usage
/// var pos = IndexOf("Hello World", "World")  # Returns 6
/// var notFound = IndexOf("Hello", "xyz")     # Returns -1
///
/// # Array usage
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
    public IndexOfFunction() : base(new JyroFunctionSignature(
        "IndexOf",
        new[] {
            new Parameter("text", ParameterType.Any),   // First parameter: string or array to search
            new Parameter("search", ParameterType.Any)  // Second parameter: value to find
        },
        ParameterType.Number))   // Return type: index (number)
    {
    }

    /// <summary>
    /// Executes the IndexOf function to find the index of a value in a string or array.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments:
    /// <list type="bullet">
    /// <item><description>Argument 0: The string or array to search (JyroString or JyroArray)</description></item>
    /// <item><description>Argument 1: The value to find (any JyroValue)</description></item>
    /// </list>
    /// </param>
    /// <param name="executionContext">The execution context for the function.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> representing the zero-based index of the first matching element or substring,
    /// or -1 if the value is not found.
    /// </returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the first argument is neither a string nor an array.
    /// </exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var sourceValue = arguments[0];
        var searchValue = arguments[1];

        // Handle string case
        if (sourceValue is JyroString sourceString && searchValue is JyroString searchString)
        {
            var position = sourceString.Value.IndexOf(searchString.Value, StringComparison.Ordinal);
            return new JyroNumber(position);
        }

        // Handle array case
        if (sourceValue is JyroArray searchArray)
        {
            for (var index = 0; index < searchArray.Length; index++)
            {
                var currentElement = searchArray[index];

                // Use deep equality comparison via JyroValue.Equals()
                if (currentElement.Equals(searchValue))
                {
                    return new JyroNumber(index);
                }
            }

            // Not found in array
            return new JyroNumber(-1);
        }

        throw new JyroRuntimeException("IndexOf() function requires string or array as first argument");
    }
}
