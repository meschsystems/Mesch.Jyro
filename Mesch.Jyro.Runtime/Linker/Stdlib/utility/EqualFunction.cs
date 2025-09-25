namespace Mesch.Jyro;

/// <summary>
/// Tests whether two values are equal using Jyro's standard equality semantics.
/// Performs deep comparison for complex types including objects and arrays,
/// and handles type coercion according to Jyro language rules.
/// </summary>
public sealed class EqualFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EqualFunction"/> class
    /// with a signature that accepts any two values and returns a boolean result.
    /// </summary>
    public EqualFunction() : base(FunctionSignatures.Binary("Equal", ParameterType.Any, ParameterType.Any, ParameterType.Boolean))
    {
    }

    /// <summary>
    /// Executes the equality comparison operation between two values.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The first value to compare (any JyroValue type)
    /// - arguments[1]: The second value to compare (any JyroValue type)
    /// </param>
    /// <param name="executionContext">The execution context (not used by this function).</param>
    /// <returns>
    /// A <see cref="JyroBoolean"/> indicating whether the two values are considered
    /// equal according to Jyro's equality rules.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var firstValue = arguments[0];
        var secondValue = arguments[1];

        var valuesAreEqual = firstValue.Equals(secondValue);
        return JyroBoolean.FromBoolean(valuesAreEqual);
    }
}