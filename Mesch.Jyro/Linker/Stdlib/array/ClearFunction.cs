namespace Mesch.Jyro;

/// <summary>
/// Removes all elements from an array, resulting in an empty array with zero length.
/// This function modifies the original array in-place and returns the same array
/// instance to support method chaining patterns.
/// </summary>
public sealed class ClearFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClearFunction"/> class
    /// with a signature that accepts an array and returns the same array after clearing.
    /// </summary>
    public ClearFunction() : base(FunctionSignatures.Unary("Clear", ParameterType.Array, ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the clear operation by removing all elements from the specified array.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array to clear (JyroArray)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// The same array instance after all elements have been removed. The array
    /// will have a length of zero but retains its identity for reference equality.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var targetArray = GetArrayArgument(arguments, 0);
        targetArray.Clear();
        return targetArray;
    }
}