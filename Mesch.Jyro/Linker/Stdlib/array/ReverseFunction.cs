namespace Mesch.Jyro;

/// <summary>
/// Reverses the order of elements in an array, returning a new array with elements
/// in reversed order. The first element becomes the last and the last element becomes
/// the first. The original array is not modified.
/// </summary>
public sealed class ReverseFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReverseFunction"/> class
    /// with a signature that accepts an array and returns a new reversed array.
    /// </summary>
    public ReverseFunction() : base(FunctionSignatures.Unary("Reverse", ParameterType.Array, ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the array reversal operation, returning a new reversed array.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array to reverse (JyroArray)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A new <see cref="JyroArray"/> with elements in reversed order. The original
    /// array is not modified. The first element of the source array becomes the last
    /// element of the result, the second becomes second-last, and so on.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var sourceArray = GetArrayArgument(arguments, 0);
        var arrayElements = new List<JyroValue>();

        // Copy elements from source array
        for (int elementIndex = 0; elementIndex < sourceArray.Length; elementIndex++)
        {
            arrayElements.Add(sourceArray[elementIndex]);
        }

        // Reverse the copied list
        arrayElements.Reverse();

        // Create and return new array with reversed elements
        var reversedArray = new JyroArray();
        foreach (var reversedElement in arrayElements)
        {
            reversedArray.Add(reversedElement);
        }

        return reversedArray;
    }
}