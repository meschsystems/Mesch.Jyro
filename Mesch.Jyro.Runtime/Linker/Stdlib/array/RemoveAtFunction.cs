namespace Mesch.Jyro;

/// <summary>
/// Removes an element at a specific index from an array and returns the removed element.
/// The array is modified in-place with all subsequent elements shifted down by one position.
/// Returns null if the specified index is out of bounds rather than throwing an exception.
/// </summary>
public sealed class RemoveAtFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveAtFunction"/> class
    /// with a signature that accepts an array and index, returning the removed element.
    /// </summary>
    public RemoveAtFunction() : base(FunctionSignatures.Binary("RemoveAt", ParameterType.Array, ParameterType.Number, ParameterType.Any))
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
    /// <param name="executionContext">The execution context (not used by this function).</param>
    /// <returns>
    /// The element that was removed from the array, or <see cref="JyroNull.Instance"/>
    /// if the index is out of bounds. The array is modified in-place.
    /// </returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the index is not an integer value.
    /// </exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var targetArray = GetArrayArgument(arguments, 0);
        var indexArgument = GetArgument<JyroNumber>(arguments, 1);

        if (!indexArgument.IsInteger)
        {
            throw new JyroRuntimeException("RemoveAt() function requires an integer index");
        }

        var removalIndex = indexArgument.ToInteger();
        if (removalIndex < 0 || removalIndex >= targetArray.Length)
        {
            return JyroNull.Instance;
        }

        var removedElement = targetArray[removalIndex];
        targetArray.RemoveAt(removalIndex);
        return removedElement;
    }
}