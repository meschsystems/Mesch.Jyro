namespace Mesch.Jyro;

/// <summary>
/// Removes the last element from an array and returns the modified array to support chaining.
/// The array is modified in-place by removing the final element, reducing the array length by one.
/// If the array is empty, it is returned unchanged rather than throwing an exception.
/// </summary>
public sealed class RemoveLastFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveLastFunction"/> class
    /// with a signature that accepts an array and returns the modified array.
    /// </summary>
    public RemoveLastFunction() : base(FunctionSignatures.Unary("RemoveLast", ParameterType.Array, ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the last element removal operation on the specified array.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array to modify (JyroArray)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// The modified array with the last element removed. If the array is empty,
    /// it is returned unchanged. This enables method chaining.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var targetArray = GetArrayArgument(arguments, 0);

        // Only remove if array is not empty
        if (targetArray.Length > 0)
        {
            var lastElementIndex = targetArray.Length - 1;
            targetArray.RemoveAt(lastElementIndex);
        }

        // Always return the array for chaining
        return targetArray;
    }
}