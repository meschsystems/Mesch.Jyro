namespace Mesch.Jyro;

/// <summary>
/// Appends a value of any type to the end of an array and returns the modified array.
/// This function mutates the original array by adding the specified value as a new element.
/// The append operation supports all Jyro value types including primitive values (numbers, 
/// strings, booleans, null) and complex values (objects, nested arrays). Mixed types are
/// supported within the same array. The function modifies the original array in-place
/// and returns it to enable method chaining or assignment patterns in scripts.
/// </summary>
public sealed class AppendFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppendFunction"/> class
    /// with a signature that accepts an array and any value type, returning the modified array.
    /// </summary>
    public AppendFunction() : base(FunctionSignatures.Binary("Append", ParameterType.Array, ParameterType.Any, ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the append operation by adding the specified value to the end of the array.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The target array to append to (JyroArray)
    /// - arguments[1]: The value to append (any JyroValue type)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// The modified array with the new value appended to the end. The same array
    /// instance is returned to support method chaining.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var targetArray = GetArrayArgument(arguments, 0);
        var valueToAppend = arguments[1];

        targetArray.Add(valueToAppend);
        return targetArray;
    }
}