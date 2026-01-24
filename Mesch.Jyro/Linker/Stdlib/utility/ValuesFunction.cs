namespace Mesch.Jyro;

/// <summary>
/// Returns an array containing all property values of an object.
/// This is the complement to the Keys function which returns property names.
/// The order of values matches the order of properties in the object.
/// </summary>
public sealed class ValuesFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValuesFunction"/> class
    /// with a signature that accepts an object and returns an array of values.
    /// </summary>
    public ValuesFunction() : base(FunctionSignatures.Unary("Values", ParameterType.Object, ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the value extraction operation on the specified object.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The object to extract values from (JyroObject)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroArray"/> containing all property values from the object.
    /// Returns an empty array if the object has no properties or is null.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var inputObject = GetObjectArgument(arguments, 0);

        var valuesArray = new JyroArray();
        foreach (var kvp in inputObject)
        {
            valuesArray.Add(kvp.Value);
        }

        return valuesArray;
    }
}
