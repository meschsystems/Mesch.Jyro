namespace Mesch.Jyro;

/// <summary>
/// Removes and returns the last element from an array, similar to stack pop operations.
/// The array is modified in-place by removing the final element. This function is
/// useful when you need the removed element value, unlike RemoveLast which returns
/// the array for chaining. Returns null if the array is empty.
/// </summary>
public sealed class PopFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PopFunction"/> class
    /// with a signature that accepts an array and returns the removed element.
    /// </summary>
    public PopFunction() : base(FunctionSignatures.Unary("Pop", ParameterType.Array, ParameterType.Any))
    {
    }

    /// <summary>
    /// Executes the pop operation, removing and returning the last element.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array to modify (JyroArray)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// The element that was removed from the end of the array, or <see cref="JyroNull.Instance"/>
    /// if the array is empty. The array is modified in-place.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Use <see cref="PopFunction"/> when you need the removed element value:
    /// <code>
    /// var last = Pop(array)  // Get and remove last element
    /// </code>
    /// </para>
    /// <para>
    /// Use <see cref="RemoveLastFunction"/> when you need to continue chaining:
    /// <code>
    /// var result = Length(RemoveLast(array))  // Remove and chain
    /// </code>
    /// </para>
    /// <para>
    /// Use <see cref="LastFunction"/> when you only need to read without removing:
    /// <code>
    /// var last = Last(array)  // Read without modifying
    /// </code>
    /// </para>
    /// </remarks>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
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
