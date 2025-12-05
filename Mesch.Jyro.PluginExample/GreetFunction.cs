namespace Mesch.Jyro.PluginExample;

/// <summary>
/// A custom Jyro function that creates a greeting message.
/// This is an example of a dynamically loaded function from a plugin DLL.
/// </summary>
public sealed class GreetFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GreetFunction"/> class.
    /// </summary>
    public GreetFunction() : base(new JyroFunctionSignature(
        "Greet",
        new[] { new Parameter("name", ParameterType.String) },
        ParameterType.String))
    {
    }

    /// <summary>
    /// Executes the greeting operation.
    /// </summary>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var name = GetStringArgument(arguments, 0);
        var greeting = $"Hello, {name}! Welcome to Jyro plugins!";
        return new JyroString(greeting);
    }
}
