namespace Mesch.Jyro;

/// <summary>
/// Factory class for creating standardized function signatures.
/// Provides convenient methods for generating common function signature patterns
/// used throughout the Jyro runtime system.
/// </summary>
public static class FunctionSignatures
{
    /// <summary>
    /// Creates a function signature for unary operations that accept a single parameter.
    /// </summary>
    /// <param name="functionName">The name of the function.</param>
    /// <param name="inputParameterType">The type of the input parameter.</param>
    /// <param name="returnType">The type returned by the function.</param>
    /// <returns>A <see cref="JyroFunctionSignature"/> representing the unary function.</returns>
    public static JyroFunctionSignature Unary(string functionName, ParameterType inputParameterType, ParameterType returnType) =>
        new(functionName, new[] { new Parameter("value", inputParameterType) }, returnType);

    /// <summary>
    /// Creates a function signature for binary operations that accept two parameters.
    /// </summary>
    /// <param name="functionName">The name of the function.</param>
    /// <param name="leftParameterType">The type of the left-hand parameter.</param>
    /// <param name="rightParameterType">The type of the right-hand parameter.</param>
    /// <param name="returnType">The type returned by the function.</param>
    /// <returns>A <see cref="JyroFunctionSignature"/> representing the binary function.</returns>
    public static JyroFunctionSignature Binary(string functionName, ParameterType leftParameterType, ParameterType rightParameterType, ParameterType returnType) =>
        new(functionName, new[] {
            new Parameter("left", leftParameterType),
            new Parameter("right", rightParameterType)
        }, returnType);

    /// <summary>
    /// Creates a function signature for variadic operations that accept a variable number of parameters.
    /// </summary>
    /// <param name="functionName">The name of the function.</param>
    /// <param name="elementType">The type of each variadic parameter.</param>
    /// <param name="returnType">The type returned by the function.</param>
    /// <param name="minimumArgumentCount">The minimum number of required arguments. Default is 1.</param>
    /// <returns>A <see cref="JyroFunctionSignature"/> representing the variadic function.</returns>
    public static JyroFunctionSignature Variadic(string functionName, ParameterType elementType, ParameterType returnType, int minimumArgumentCount = 1) =>
        new(functionName, CreateVariadicParameters(elementType, minimumArgumentCount, 10), returnType);

    /// <summary>
    /// Creates an array of parameters for variadic functions with specified required and optional parameters.
    /// </summary>
    /// <param name="parameterType">The type for all variadic parameters.</param>
    /// <param name="requiredParameterCount">The number of required parameters.</param>
    /// <param name="maximumParameterCount">The maximum number of parameters supported.</param>
    /// <returns>An array of <see cref="Parameter"/> instances representing the variadic parameters.</returns>
    private static Parameter[] CreateVariadicParameters(ParameterType parameterType, int requiredParameterCount, int maximumParameterCount)
    {
        var parameters = new Parameter[maximumParameterCount];

        for (int parameterIndex = 0; parameterIndex < maximumParameterCount; parameterIndex++)
        {
            bool isOptionalParameter = parameterIndex >= requiredParameterCount;
            parameters[parameterIndex] = new Parameter($"argument{parameterIndex}", parameterType, isOptionalParameter);
        }

        return parameters;
    }
}