namespace Mesch.Jyro;

/// <summary>
/// Constrains a numeric value to be within a specified range.
/// If the value is less than the minimum, returns the minimum.
/// If the value is greater than the maximum, returns the maximum.
/// Otherwise, returns the original value unchanged.
/// </summary>
public sealed class ClampFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClampFunction"/> class
    /// with a signature that accepts three numbers (value, min, max).
    /// </summary>
    public ClampFunction() : base(new JyroFunctionSignature(
        "Clamp",
        new[] {
            new Parameter("value", ParameterType.Number),
            new Parameter("min", ParameterType.Number),
            new Parameter("max", ParameterType.Number)
        },
        ParameterType.Number))
    {
    }

    /// <summary>
    /// Executes the clamp operation on the specified numeric value.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The value to clamp (JyroNumber)
    /// - arguments[1]: The minimum bound (JyroNumber)
    /// - arguments[2]: The maximum bound (JyroNumber)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing the clamped value. If min is greater
    /// than max, the behavior follows Math.Clamp semantics (returns min).
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var value = GetArgument<JyroNumber>(arguments, 0);
        var min = GetArgument<JyroNumber>(arguments, 1);
        var max = GetArgument<JyroNumber>(arguments, 2);

        var clampedValue = Math.Clamp(value.Value, min.Value, max.Value);
        return new JyroNumber(clampedValue);
    }
}
