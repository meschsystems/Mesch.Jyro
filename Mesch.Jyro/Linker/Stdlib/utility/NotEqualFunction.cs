namespace Mesch.Jyro;

/// <summary>
/// Tests whether two values are not equal using Jyro's standard equality semantics.
/// Returns the logical negation of the equality comparison, performing deep comparison
/// for complex types including objects and arrays with appropriate type coercion.
/// </summary>
public sealed class NotEqualFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotEqualFunction"/> class
    /// with a signature that accepts any two values and returns a boolean result.
    /// </summary>
    public NotEqualFunction() : base(FunctionSignatures.Binary("NotEqual", ParameterType.Any, ParameterType.Any, ParameterType.Boolean))
    {
    }

    /// <summary>
    /// Executes the inequality comparison operation between two values.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The first value to compare (any JyroValue type)
    /// - arguments[1]: The second value to compare (any JyroValue type)
    /// </param>
    /// <param name="executionContext">The execution context (not used by this function).</param>
    /// <returns>
    /// A <see cref="JyroBoolean"/> indicating whether the two values are not equal
    /// according to Jyro's equality rules. Returns <c>true</c> if the values differ,
    /// <c>false</c> if they are considered equal.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var firstValue = arguments[0];
        var secondValue = arguments[1];

        var valuesAreNotEqual = !firstValue.Equals(secondValue);
        return JyroBoolean.FromBoolean(valuesAreNotEqual);
    }
}