namespace Mesch.Jyro;

/// <summary>
/// Calculates the absolute value of a numeric input, returning the non-negative
/// magnitude of the number. Handles both integer and floating-point values
/// according to standard mathematical absolute value semantics.
/// </summary>
public sealed class AbsFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AbsFunction"/> class
    /// with a signature that accepts a number and returns a number.
    /// </summary>
    public AbsFunction() : base(FunctionSignatures.Unary("Abs", ParameterType.Number, ParameterType.Number))
    {
    }

    /// <summary>
    /// Executes the absolute value calculation on the specified numeric input.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The numeric value to process (JyroNumber)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing the absolute value of the input.
    /// The result is always non-negative.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var inputNumber = GetArgument<JyroNumber>(arguments, 0);
        var absoluteValue = Math.Abs(inputNumber.Value);
        return new JyroNumber(absoluteValue);
    }
}