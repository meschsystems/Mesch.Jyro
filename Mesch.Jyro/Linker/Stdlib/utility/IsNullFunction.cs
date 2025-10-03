namespace Mesch.Jyro;

/// <summary>
/// Tests whether a value is null within the Jyro type system. This function
/// provides a direct way to check for null values, which is the logical
/// inverse of the Exists function and useful for conditional logic and validation.
/// </summary>
public sealed class IsNullFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IsNullFunction"/> class
    /// with a signature that accepts any value type and returns a boolean result.
    /// </summary>
    public IsNullFunction() : base(FunctionSignatures.Unary("IsNull", ParameterType.Any, ParameterType.Boolean))
    {
    }

    /// <summary>
    /// Executes the null check operation on the specified value.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The value to test for null (any JyroValue type)
    /// </param>
    /// <param name="executionContext">The execution context (not used by this function).</param>
    /// <returns>
    /// A <see cref="JyroBoolean"/> indicating whether the value is null.
    /// Returns <c>true</c> for null values, <c>false</c> for all non-null values.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var valueToTest = arguments[0];
        var valueIsNull = valueToTest.IsNull;
        return JyroBoolean.FromBoolean(valueIsNull);
    }
}