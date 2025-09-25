namespace Mesch.Jyro;

/// <summary>
/// Tests whether a source value contains a specified search value. Supports
/// substring searching within strings and value searching within arrays.
/// Uses case-sensitive comparison for strings and Jyro equality semantics for arrays.
/// </summary>
public sealed class ContainsFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContainsFunction"/> class
    /// with a signature that accepts any two values and returns a boolean result.
    /// </summary>
    public ContainsFunction() : base(FunctionSignatures.Binary("Contains", ParameterType.Any, ParameterType.Any, ParameterType.Boolean))
    {
    }

    /// <summary>
    /// Executes the containment check operation based on the types of the provided arguments.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The source value to search within (JyroString or JyroArray)
    /// - arguments[1]: The value to search for (any JyroValue type)
    /// </param>
    /// <param name="executionContext">The execution context (not used by this function).</param>
    /// <returns>
    /// A <see cref="JyroBoolean"/> indicating whether the source contains the search value.
    /// Returns <c>false</c> if either argument is null.
    /// </returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the first argument is neither a string nor an array.
    /// </exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var sourceValue = arguments[0];
        var searchValue = arguments[1];

        if (sourceValue.IsNull || searchValue.IsNull)
        {
            return JyroBoolean.False;
        }

        if (sourceValue is JyroString sourceString && searchValue is JyroString searchString)
        {
            var containsSubstring = sourceString.Value.Contains(searchString.Value);
            return JyroBoolean.FromBoolean(containsSubstring);
        }

        if (sourceValue is JyroArray sourceArray)
        {
            foreach (var arrayElement in sourceArray)
            {
                if (arrayElement.Equals(searchValue))
                {
                    return JyroBoolean.True;
                }
            }
            return JyroBoolean.False;
        }

        throw new JyroRuntimeException("Contains() function requires string or array as first argument");
    }
}