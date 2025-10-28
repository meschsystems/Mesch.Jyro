namespace Mesch.Jyro;

/// <summary>
/// Decodes a Base64-encoded string back to its original string representation.
/// This is the inverse operation of Base64Encode, enabling round-trip encoding and decoding.
/// </summary>
public sealed class Base64DecodeFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Base64DecodeFunction"/> class
    /// with a signature that accepts a Base64-encoded string and returns the decoded string.
    /// </summary>
    public Base64DecodeFunction()
        : base(FunctionSignatures.Unary("Base64Decode", ParameterType.String, ParameterType.String))
    {
    }

    /// <summary>
    /// Executes the Base64 decoding operation on the specified string.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The Base64-encoded string to decode (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroString"/> containing the decoded representation of the input string.
    /// The decoding assumes UTF-8 encoding for converting bytes back to a string.
    /// </returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the input string is not a valid Base64-encoded string.
    /// </exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var inputString = GetArgument<JyroString>(arguments, 0);

        try
        {
            var bytes = Convert.FromBase64String(inputString.Value);
            var decodedResult = System.Text.Encoding.UTF8.GetString(bytes);

            return new JyroString(decodedResult);
        }
        catch (FormatException ex)
        {
            throw new JyroRuntimeException(
                $"Base64Decode() function requires a valid Base64-encoded string. Error: {ex.Message}");
        }
    }
}
