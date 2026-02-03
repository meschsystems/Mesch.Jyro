namespace Mesch.Jyro;

/// <summary>
/// Merges multiple objects into a single new object. Properties from later arguments
/// override properties from earlier arguments (shallow merge). Non-object arguments are skipped.
/// </summary>
public sealed class MergeFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MergeFunction"/> class
    /// with a variadic signature that accepts multiple objects.
    /// </summary>
    public MergeFunction() : base(FunctionSignatures.Variadic("Merge", ParameterType.Any, ParameterType.Object, 0))
    {
    }

    /// <summary>
    /// Executes the merge operation on the provided objects.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments containing objects to merge. Later arguments override earlier ones.
    /// Non-object arguments are silently skipped.
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A new <see cref="JyroObject"/> containing all properties from the input objects,
    /// with later arguments taking precedence over earlier ones. Returns an empty object
    /// if no valid objects are provided.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var result = new JyroObject();

        foreach (var argument in arguments)
        {
            if (argument is JyroObject obj)
            {
                // Copy all properties from this object to the result
                // Later objects override earlier ones
                foreach (var kvp in obj)
                {
                    result.SetProperty(kvp.Key, kvp.Value);
                }
            }
            // Non-object arguments are silently skipped
        }

        return result;
    }
}
