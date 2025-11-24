namespace Mesch.Jyro;

/// <summary>
/// Merges multiple arrays into a single array, concatenating all elements
/// in the order they appear. Supports merging variable numbers of arrays
/// and handles mixed argument types by treating non-array values as individual elements.
/// Null values are ignored during the merge process.
/// </summary>
public sealed class MergeArraysFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MergeArraysFunction"/> class
    /// with a variadic signature that accepts multiple array arguments.
    /// </summary>
    public MergeArraysFunction() : base(FunctionSignatures.Variadic("MergeArrays", ParameterType.Array, ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the array merging operation by concatenating all provided arrays
    /// and individual values into a single result array.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments containing arrays and values to merge.
    /// Array arguments have their elements added individually, while
    /// non-array arguments are added as single elements. Null values are ignored.
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A new <see cref="JyroArray"/> containing all elements from the input arrays
    /// and individual values, preserving the order in which they were provided.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var mergedResult = new JyroArray();

        foreach (var argumentValue in arguments)
        {
            if (argumentValue is JyroArray arrayArgument)
            {
                foreach (var arrayElement in arrayArgument)
                {
                    mergedResult.Add(arrayElement);
                }
            }
            else if (!argumentValue.IsNull)
            {
                mergedResult.Add(argumentValue);
            }
        }

        return mergedResult;
    }
}