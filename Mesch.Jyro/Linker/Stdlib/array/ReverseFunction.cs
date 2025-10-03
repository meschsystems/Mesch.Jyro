namespace Mesch.Jyro;

/// <summary>
/// Reverses the order of elements in an array in-place, with the first element
/// becoming the last and the last element becoming the first. The array is
/// modified directly and returned to support method chaining patterns.
/// </summary>
public sealed class ReverseFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReverseFunction"/> class
    /// with a signature that accepts an array and returns the same reversed array.
    /// </summary>
    public ReverseFunction() : base(FunctionSignatures.Unary("Reverse", ParameterType.Array, ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the array reversal operation in-place on the specified array.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array to reverse (JyroArray)
    /// </param>
    /// <param name="executionContext">The execution context (not used by this function).</param>
    /// <returns>
    /// The same array instance with its elements in reversed order. The first
    /// element moves to the last position, the second to second-last, and so on.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var targetArray = GetArrayArgument(arguments, 0);
        var arrayElements = new List<JyroValue>();

        for (int elementIndex = 0; elementIndex < targetArray.Length; elementIndex++)
        {
            arrayElements.Add(targetArray[elementIndex]);
        }

        arrayElements.Reverse();
        targetArray.Clear();

        foreach (var reversedElement in arrayElements)
        {
            targetArray.Add(reversedElement);
        }

        return targetArray;
    }
}