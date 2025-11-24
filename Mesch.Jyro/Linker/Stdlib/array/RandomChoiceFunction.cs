using System.Security.Cryptography;

namespace Mesch.Jyro;

/// <summary>
/// Selects a random element from an array using cryptographically secure random generation.
/// Uses System.Security.Cryptography.RandomNumberGenerator to ensure unpredictable selection
/// suitable for security-sensitive scenarios. Each element has an equal probability of being
/// selected. Throws an error if the array is empty rather than returning null.
/// </summary>
public sealed class RandomChoiceFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RandomChoiceFunction"/> class
    /// with a unary signature that accepts an array and returns a randomly selected element.
    /// </summary>
    public RandomChoiceFunction() : base(FunctionSignatures.Unary("RandomChoice", ParameterType.Array, ParameterType.Any))
    {
    }

    /// <summary>
    /// Executes the random selection operation on the specified array.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array to select from (JyroArray)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A randomly selected element from the array. The type matches the type of elements
    /// in the array (can be JyroNumber, JyroString, JyroObject, JyroArray, etc.).
    /// </returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the array is empty. Use conditional logic to check array length
    /// if empty arrays are possible.
    /// </exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var array = GetArrayArgument(arguments, 0);

        if (array.Length == 0)
        {
            throw new JyroRuntimeException("RandomChoice() cannot select from an empty array");
        }

        // Use cryptographically secure random index generation
        var randomIndex = RandomNumberGenerator.GetInt32(0, array.Length);
        return array[randomIndex];
    }
}
