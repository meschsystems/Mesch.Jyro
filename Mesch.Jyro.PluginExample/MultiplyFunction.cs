namespace Mesch.Jyro.PluginExample;

/// <summary>
/// A custom Jyro function that multiplies two numbers.
/// This is an example of a dynamically loaded function from a plugin DLL.
/// </summary>
public sealed class MultiplyFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiplyFunction"/> class.
    /// </summary>
    public MultiplyFunction() : base(new JyroFunctionSignature(
        "Multiply",
        new[]
        {
            new Parameter("a", ParameterType.Number),
            new Parameter("b", ParameterType.Number)
        },
        ParameterType.Number))
    {
    }

    /// <summary>
    /// Executes the multiplication operation.
    /// </summary>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var a = GetNumberArgument(arguments, 0);
        var b = GetNumberArgument(arguments, 1);
        return new JyroNumber(a * b);
    }
}
