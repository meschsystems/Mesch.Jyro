namespace Mesch.Jyro;

/// <summary>
/// Calculates the length or count of elements for various Jyro value types.
/// Returns the character count for strings, element count for arrays,
/// property count for objects, zero for null values, and one for primitive values.
/// </summary>
public sealed class LengthFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LengthFunction"/> class
    /// with a signature that accepts any value type and returns a numeric result.
    /// </summary>
    public LengthFunction() : base(new JyroFunctionSignature(
        "Length",
        new[] { new Parameter("value", ParameterType.Any) },
        ParameterType.Number))
    {
    }

    /// <summary>
    /// Executes the length calculation based on the type of the input value.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The value to measure (any JyroValue type)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing the length or count:
    /// - For strings: character count
    /// - For arrays: element count
    /// - For objects: property count
    /// - For null: zero
    /// - For primitives (numbers, booleans): one
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var inputValue = arguments[0];

        var calculatedLength = inputValue switch
        {
            JyroString stringValue => stringValue.Length,
            JyroArray arrayValue => arrayValue.Length,
            JyroObject objectValue => objectValue.Count,
            JyroNull => 0,
            _ => 1
        };

        return new JyroNumber(calculatedLength);
    }
}