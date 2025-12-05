namespace Mesch.Jyro.PluginExample;

/// <summary>
/// A custom Jyro function that reverses a string.
/// This is an example of a dynamically loaded function from a plugin DLL.
/// </summary>
public sealed class ReverseStringFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReverseStringFunction"/> class.
    /// </summary>
    public ReverseStringFunction() : base(new JyroFunctionSignature(
        "ReverseString",
        new[] { new Parameter("text", ParameterType.String) },
        ParameterType.String))
    {
    }

    /// <summary>
    /// Executes the string reversal operation.
    /// </summary>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var text = GetStringArgument(arguments, 0);
        var reversed = new string(text.Reverse().ToArray());
        return new JyroString(reversed);
    }
}
