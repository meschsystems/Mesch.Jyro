namespace Mesch.Jyro;

/// <summary>
/// Removes an element at a specific index from an array and returns the modified array
/// to support chaining. The array is modified in-place with all subsequent elements
/// shifted down by one position. If the index is out of bounds, the array is returned
/// unchanged rather than throwing an exception.
/// </summary>
public sealed class RemoveAtFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveAtFunction"/> class
    /// with a signature that accepts an array and index, returning the modified array.
    /// </summary>
    public RemoveAtFunction() : base(FunctionSignatures.Binary("RemoveAt", ParameterType.Array, ParameterType.Number, ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the element removal operation at the specified index position.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array to modify (JyroArray)
    /// - arguments[1]: The zero-based index of the element to remove (JyroNumber, must be integer)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// The modified array with the element removed. If the index is out of bounds,
    /// the array is returned unchanged. This enables method chaining.
    /// </returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the index is not an integer value.
    /// </exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var targetArray = GetArrayArgument(arguments, 0);
        var indexArgument = GetArgument<JyroNumber>(arguments, 1);

        if (!indexArgument.IsInteger)
        {
            throw new JyroRuntimeException("RemoveAt() function requires an integer index. Received: " + indexArgument.Value);
        }

        var removalIndex = indexArgument.ToInteger();

        // Only remove if index is within bounds
        if (removalIndex >= 0 && removalIndex < targetArray.Length)
        {
            targetArray.RemoveAt(removalIndex);
        }

        // Always return the array for chaining
        return targetArray;
    }
}