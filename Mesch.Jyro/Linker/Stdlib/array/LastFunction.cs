namespace Mesch.Jyro;

/// <summary>
/// Returns the last element of an array without modifying the array.
/// Returns null if the array is empty, providing a safe way to access the
/// last element without needing to check the array length or calculate indices.
/// </summary>
public sealed class LastFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LastFunction"/> class
    /// with a signature that accepts an array and returns the last element or null.
    /// </summary>
    public LastFunction() : base(FunctionSignatures.Unary("Last", ParameterType.Array, ParameterType.Any))
    {
    }

    /// <summary>
    /// Executes the last element accessor operation on the specified array.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array to access (JyroArray)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// The last element of the array, or <see cref="JyroNull.Instance"/> if the array
    /// is empty. The array is not modified.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var array = GetArrayArgument(arguments, 0);

        if (array.Length == 0)
        {
            return JyroNull.Instance;
        }

        return array[array.Length - 1];
    }
}
