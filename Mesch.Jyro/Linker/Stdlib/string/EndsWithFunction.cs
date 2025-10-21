namespace Mesch.Jyro;

/// <summary>
/// Tests whether a string ends with a specified suffix, performing a case-sensitive
/// comparison using ordinal string comparison semantics.
/// </summary>
public sealed class EndsWithFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EndsWithFunction"/> class
    /// with a signature that accepts two strings and returns a boolean result.
    /// </summary>
    public EndsWithFunction() : base(FunctionSignatures.Binary("EndsWith", ParameterType.String, ParameterType.String, ParameterType.Boolean))
    {
    }

    /// <summary>
    /// Executes the string suffix comparison operation.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The source string to test (JyroString)
    /// - arguments[1]: The suffix to search for (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroBoolean"/> indicating whether the source string ends
    /// with the specified suffix.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var sourceString = GetArgument<JyroString>(arguments, 0);
        var suffixString = GetArgument<JyroString>(arguments, 1);

        var endsWithSuffix = sourceString.Value.EndsWith(suffixString.Value);
        return JyroBoolean.FromBoolean(endsWithSuffix);
    }
}