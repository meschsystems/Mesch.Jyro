using System.Security.Cryptography;

namespace Mesch.Jyro;

/// <summary>
/// Generates a cryptographically secure random string of specified length from a character set.
/// Uses System.Security.Cryptography.RandomNumberGenerator to create unpredictable strings
/// suitable for passwords, tokens, authentication codes, and other security-sensitive scenarios.
/// Supports custom character sets or defaults to alphanumeric characters (a-z, A-Z, 0-9).
/// </summary>
public sealed class RandomStringFunction : JyroFunctionBase
{
    private const string DefaultCharset = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    /// <summary>
    /// Initializes a new instance of the <see cref="RandomStringFunction"/> class
    /// with a signature that accepts a length and optional character set.
    /// </summary>
    public RandomStringFunction() : base(new JyroFunctionSignature(
        "RandomString",
        new[] {
            new Parameter("length", ParameterType.Number),
            new Parameter("characterSet", ParameterType.String, isOptionalParameter: true)
        },
        ParameterType.String))
    {
    }

    /// <summary>
    /// Executes the random string generation with the specified parameters.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The length of the string to generate (JyroNumber, must be non-negative integer)
    /// - arguments[1]: Optional character set to select characters from (JyroString, defaults to alphanumeric)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroString"/> containing a cryptographically secure random string
    /// of the specified length composed of characters from the character set.
    /// </returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when:
    /// - Length is not a non-negative integer
    /// - Character set is empty or null
    /// </exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var lengthArg = GetArgument<JyroNumber>(arguments, 0);

        if (!lengthArg.IsInteger || lengthArg.Value < 0)
        {
            throw new JyroRuntimeException($"RandomString() requires a non-negative integer length. Received: {lengthArg.Value}");
        }

        var length = lengthArg.ToInteger();
        var charsetArg = GetOptionalArgument<JyroString>(arguments, 1);
        var charset = charsetArg?.Value ?? DefaultCharset;

        if (string.IsNullOrEmpty(charset))
        {
            throw new JyroRuntimeException("RandomString() character set cannot be empty");
        }

        // Use cryptographically secure random string generation
        var randomString = RandomNumberGenerator.GetString(charset, length);
        return new JyroString(randomString);
    }
}
