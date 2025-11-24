namespace Mesch.Jyro;

/// <summary>
/// Returns a new array containing the first n elements from the source array.
/// The original array remains unmodified. If n is greater than the array length,
/// all elements are returned. If n is less than or equal to 0, an empty array is returned.
/// </summary>
public sealed class TakeFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TakeFunction"/> class
    /// with a signature that accepts an array and count, returning a new array.
    /// </summary>
    public TakeFunction() : base(FunctionSignatures.Binary("Take", ParameterType.Array, ParameterType.Number, ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the take operation to retrieve the first n elements.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The source array (JyroArray)
    /// - arguments[1]: The number of elements to take (JyroNumber, must be integer)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A new array containing the first n elements from the source array.
    /// The original array is not modified.
    /// </returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the count parameter is not an integer value.
    /// </exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var sourceArray = GetArrayArgument(arguments, 0);
        var countArgument = GetArgument<JyroNumber>(arguments, 1);

        if (!countArgument.IsInteger)
        {
            throw new JyroRuntimeException("Take() function requires an integer count. Received: " + countArgument.Value);
        }

        var count = countArgument.ToInteger();

        // Handle edge cases
        if (count <= 0)
        {
            return new JyroArray();
        }

        // Take the minimum of count and array length
        var takeCount = Math.Min(count, sourceArray.Length);

        // Create a new array with the first n elements
        var resultArray = new JyroArray();
        for (int i = 0; i < takeCount; i++)
        {
            resultArray.Add(sourceArray[i]);
        }

        return resultArray;
    }
}
