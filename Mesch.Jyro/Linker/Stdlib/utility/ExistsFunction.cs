namespace Mesch.Jyro;

/// <summary>
/// Tests whether a property exists on an object and is not null.
/// This function provides a convenient way to check for the presence of
/// meaningful data on objects before performing operations that require non-null property values.
/// </summary>
public sealed class ExistsFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExistsFunction"/> class
    /// with a signature that accepts an object and a property name, returning a boolean result.
    /// </summary>
    public ExistsFunction() : base(FunctionSignatures.Binary("Exists", ParameterType.Object, ParameterType.String, ParameterType.Boolean))
    {
    }

    /// <summary>
    /// Executes the property existence check operation.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The object to check (JyroObject)
    /// - arguments[1]: The property name to look for (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroBoolean"/> indicating whether the property exists on the object and is not null.
    /// Returns <c>true</c> if the property exists and is not null, <c>false</c> otherwise.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var obj = arguments[0];
        var fieldName = arguments[1];

        // Check if first arg is an object
        if (obj is not JyroObject jyroObject)
        {
            return JyroBoolean.False;
        }

        // Check if second arg is a string
        if (fieldName is not JyroString jyroString)
        {
            return JyroBoolean.False;
        }

        // Check if property exists and is not null using JyroObject.TryGet
        if (jyroObject.TryGet(jyroString.Value, out var value))
        {
            return JyroBoolean.FromBoolean(!value.IsNull);
        }

        return JyroBoolean.False;
    }
}