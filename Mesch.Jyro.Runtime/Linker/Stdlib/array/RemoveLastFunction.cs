namespace Mesch.Jyro;

/// <summary>
/// Removes and returns the last element from an array. The array is modified
/// in-place by removing the final element, reducing the array length by one.
/// Returns null if the array is empty rather than throwing an exception.
/// </summary>
public sealed class RemoveLastFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveLastFunction"/> class
    /// with a signature that accepts an array and returns the removed element.
    /// </summary>
    public RemoveLastFunction() : base(FunctionSignatures.Unary("RemoveLast", ParameterType.Array, ParameterType.Any))
    {
    }

    /// <summary>
    /// Executes the last element removal operation on the specified array.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array to modify (JyroArray)
    /// </param>
    /// <param name="executionContext">The execution context (not used by this function).</param>
    /// <returns>
    /// The element that was removed from the end of the array, or <see cref="JyroNull.Instance"/>
    /// if the array is empty. The array is modified in-place.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var targetArray = GetArrayArgument(arguments, 0);

        if (targetArray.Length == 0)
        {
            return JyroNull.Instance;
        }

        var lastElementIndex = targetArray.Length - 1;
        var removedElement = targetArray[lastElementIndex];
        targetArray.RemoveAt(lastElementIndex);
        return removedElement;
    }
}