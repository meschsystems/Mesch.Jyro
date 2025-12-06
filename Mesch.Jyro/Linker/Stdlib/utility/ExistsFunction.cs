namespace Mesch.Jyro;

/// <summary>
/// Tests whether a value is not null within the Jyro type system. This function
/// provides a direct way to check for the existence of meaningful data, which is the logical
/// inverse of the IsNull function and useful for conditional logic and validation.
/// </summary>
public sealed class ExistsFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExistsFunction"/> class
    /// with a signature that accepts any value type and returns a boolean result.
    /// </summary>
    public ExistsFunction() : base(FunctionSignatures.Unary("Exists", ParameterType.Any, ParameterType.Boolean))
    {
    }

    /// <summary>
    /// Executes the existence check operation on the specified value.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The value to test for existence (any JyroValue type)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroBoolean"/> indicating whether the value exists (is not null).
    /// Returns <c>true</c> for non-null values, <c>false</c> for null values.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var valueToTest = arguments[0];
        var valueExists = !valueToTest.IsNull;
        return JyroBoolean.FromBoolean(valueExists);
    }
}