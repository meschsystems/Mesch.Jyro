namespace Mesch.Jyro;

/// <summary>
/// Returns the type name of a Jyro value as a lowercase string representation.
/// Provides runtime type inspection capabilities for dynamic type checking
/// and conditional logic based on value types within Jyro scripts.
/// </summary>
public sealed class TypeOfFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeOfFunction"/> class
    /// with a signature that accepts any value type and returns a string.
    /// </summary>
    public TypeOfFunction() : base(FunctionSignatures.Unary("TypeOf", ParameterType.Any, ParameterType.String))
    {
    }

    /// <summary>
    /// Executes the type inspection operation on the specified value.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The value to inspect for type information (any JyroValue type)
    /// </param>
    /// <param name="executionContext">The execution context (not used by this function).</param>
    /// <returns>
    /// A <see cref="JyroString"/> containing the lowercase type name of the input value.
    /// Possible return values include: "number", "string", "boolean", "object", "array", "null".
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var inputValue = arguments[0];
        var typeName = inputValue.Type.ToString().ToLowerInvariant();
        return new JyroString(typeName);
    }
}