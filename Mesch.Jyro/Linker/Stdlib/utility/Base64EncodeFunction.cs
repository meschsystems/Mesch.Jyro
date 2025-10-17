namespace Mesch.Jyro;

/// <summary>
/// Encodes a string to Base64 format.
/// </summary>
public sealed class Base64EncodeFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Base64EncodeFunction"/> class
    /// with a signature that accepts a string and returns a Base64-encoded string.
    /// </summary>
    public Base64EncodeFunction()
        : base(FunctionSignatures.Unary("Base64Encode", ParameterType.String, ParameterType.String))
    {
    }

    /// <summary>
    /// Executes the Base64 encoding operation on the specified string.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The string to encode (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroString"/> containing the Base64-encoded representation of the input string.
    /// The encoding uses UTF-8 for converting the string to bytes before Base64 encoding.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var inputString = GetArgument<JyroString>(arguments, 0);

        var bytes = System.Text.Encoding.UTF8.GetBytes(inputString.Value);
        var base64Result = Convert.ToBase64String(bytes);

        return new JyroString(base64Result);
    }
}
