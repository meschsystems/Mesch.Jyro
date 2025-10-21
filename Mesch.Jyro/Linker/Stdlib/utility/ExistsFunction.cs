namespace Mesch.Jyro;

/// <summary>
/// Tests whether a value exists (is not null) within the Jyro type system.
/// This function provides a convenient way to check for the presence of
/// meaningful data before performing operations that require non-null values.
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
    /// Returns <c>true</c> for all non-null values, <c>false</c> for null values.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var valueToTest = arguments[0];
        var valueExists = !valueToTest.IsNull;
        return JyroBoolean.FromBoolean(valueExists);
    }
}