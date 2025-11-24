namespace Mesch.Jyro;

/// <summary>
/// Returns the first element of an array without modifying the array.
/// Returns null if the array is empty, providing a safe way to access the
/// first element without needing to check the array length or calculate indices.
/// </summary>
public sealed class FirstFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FirstFunction"/> class
    /// with a signature that accepts an array and returns the first element or null.
    /// </summary>
    public FirstFunction() : base(FunctionSignatures.Unary("First", ParameterType.Array, ParameterType.Any))
    {
    }

    /// <summary>
    /// Executes the first element accessor operation on the specified array.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array to access (JyroArray)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// The first element of the array, or <see cref="JyroNull.Instance"/> if the array
    /// is empty. The array is not modified.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var array = GetArrayArgument(arguments, 0);

        if (array.Length == 0)
        {
            return JyroNull.Instance;
        }

        return array[0];
    }
}
