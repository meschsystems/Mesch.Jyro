namespace Mesch.Jyro;

/// <summary>
/// Returns an array containing all property names (keys) of an object.
/// </summary>
public sealed class KeysFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeysFunction"/> class
    /// with a signature that accepts an object and returns an array of keys.
    /// </summary>
    public KeysFunction() : base(new JyroFunctionSignature(
        "Keys",
        [
            new Parameter("object", ParameterType.Object)
        ],
        ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the keys extraction operation on the object.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The object to extract keys from (JyroObject)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A new <see cref="JyroArray"/> containing the property names of the object as strings.
    /// Returns an empty array if the object has no properties or if the argument is null.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var obj = GetObjectArgument(arguments, 0);

        var result = new JyroArray();

        foreach (var kvp in obj)
        {
            result.Add(new JyroString(kvp.Key));
        }

        return result;
    }
}
