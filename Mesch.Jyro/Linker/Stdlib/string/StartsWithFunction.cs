namespace Mesch.Jyro;

/// <summary>
/// Tests whether a string begins with a specified prefix, performing a case-sensitive
/// comparison using ordinal string comparison semantics. Provides a fundamental
/// string pattern matching capability for prefix-based filtering and validation.
/// </summary>
public sealed class StartsWithFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StartsWithFunction"/> class
    /// with a signature that accepts two strings and returns a boolean result.
    /// </summary>
    public StartsWithFunction() : base(FunctionSignatures.Binary("StartsWith", ParameterType.String, ParameterType.String, ParameterType.Boolean))
    {
    }

    /// <summary>
    /// Executes the string prefix comparison operation.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The source string to test (JyroString)
    /// - arguments[1]: The prefix to search for (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context (not used by this function).</param>
    /// <returns>
    /// A <see cref="JyroBoolean"/> indicating whether the source string begins
    /// with the specified prefix using case-sensitive comparison.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var sourceString = GetArgument<JyroString>(arguments, 0);
        var prefixString = GetArgument<JyroString>(arguments, 1);

        var startsWithPrefix = sourceString.Value.StartsWith(prefixString.Value);
        return JyroBoolean.FromBoolean(startsWithPrefix);
    }
}