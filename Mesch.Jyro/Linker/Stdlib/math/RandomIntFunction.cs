using System.Security.Cryptography;

namespace Mesch.Jyro;

/// <summary>
/// Generates a cryptographically secure random integer within a specified range.
/// Supports two calling patterns: RandomInt(max) for range [0, max) or
/// RandomInt(min, max) for range [min, max). Uses System.Security.Cryptography.RandomNumberGenerator
/// to ensure cryptographic security suitable for security-sensitive scenarios including password
/// generation, token creation, and unpredictable selection.
/// </summary>
public sealed class RandomIntFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RandomIntFunction"/> class
    /// with a signature supporting either one or two numeric parameters.
    /// </summary>
    public RandomIntFunction() : base(new JyroFunctionSignature(
        "RandomInt",
        new[] {
            new Parameter("minOrMax", ParameterType.Number),
            new Parameter("max", ParameterType.Number, isOptionalParameter: true)
        },
        ParameterType.Number))
    {
    }

    /// <summary>
    /// Executes the random integer generation within the specified range.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - Single argument: arguments[0] is max, returns random integer in [0, max)
    /// - Two arguments: arguments[0] is min, arguments[1] is max, returns random integer in [min, max)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroNumber"/> containing a cryptographically secure random integer within
    /// the specified range. The upper bound is exclusive (range does not include max value).
    /// </returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when:
    /// - Arguments are not integers
    /// - max is less than or equal to min
    /// - The range is invalid
    /// </exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var firstArg = GetArgument<JyroNumber>(arguments, 0);
        var secondArg = GetOptionalArgument<JyroNumber>(arguments, 1);

        if (!firstArg.IsInteger)
        {
            throw new JyroRuntimeException($"RandomInt() requires integer arguments. First argument: {firstArg.Value}");
        }

        int min, max;

        if (secondArg == null)
        {
            // Single argument: RandomInt(max) -> [0, max)
            min = 0;
            max = firstArg.ToInteger();
        }
        else
        {
            // Two arguments: RandomInt(min, max) -> [min, max)
            if (!secondArg.IsInteger)
            {
                throw new JyroRuntimeException($"RandomInt() requires integer arguments. Second argument: {secondArg.Value}");
            }

            min = firstArg.ToInteger();
            max = secondArg.ToInteger();
        }

        if (max <= min)
        {
            throw new JyroRuntimeException($"RandomInt() requires max > min. Received: min={min}, max={max}");
        }

        // Use cryptographically secure random number generator
        var randomValue = RandomNumberGenerator.GetInt32(min, max);
        return new JyroNumber(randomValue);
    }
}
