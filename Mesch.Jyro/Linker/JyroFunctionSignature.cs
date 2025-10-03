namespace Mesch.Jyro;

/// <summary>
/// Defines the complete signature of a Jyro function, including its name,
/// parameters, return type, and validation rules. Used by the linker for
/// compile-time type checking and argument validation.
/// </summary>
public sealed class JyroFunctionSignature
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JyroFunctionSignature"/> class
    /// with the specified name, parameters, and return type.
    /// </summary>
    /// <param name="functionName">
    /// The name of the function. Cannot be null.
    /// </param>
    /// <param name="parameters">
    /// The collection of formal parameter definitions. Cannot be null.
    /// Required parameters must appear before optional parameters.
    /// </param>
    /// <param name="returnType">
    /// The expected return type of the function. Defaults to <see cref="ParameterType.Any"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="functionName"/> or <paramref name="parameters"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when required parameters are defined after optional parameters.
    /// </exception>
    public JyroFunctionSignature(
        string functionName,
        IReadOnlyList<Parameter> parameters,
        ParameterType returnType = ParameterType.Any)
    {
        Name = functionName ?? throw new ArgumentNullException(nameof(functionName));
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ReturnType = returnType;

        ValidateParameterOrdering(parameters);

        MinimumArgumentCount = parameters.Count(parameter => !parameter.IsOptional);
        MaximumArgumentCount = parameters.Count;
    }

    /// <summary>
    /// Gets the name of the function.
    /// </summary>
    /// <value>The function name as defined in the signature.</value>
    public string Name { get; }

    /// <summary>
    /// Gets the collection of formal parameter definitions for the function.
    /// </summary>
    /// <value>
    /// An ordered collection of <see cref="Parameter"/> instances defining
    /// the function's parameter list.
    /// </value>
    public IReadOnlyList<Parameter> Parameters { get; }

    /// <summary>
    /// Gets the expected return type of the function.
    /// </summary>
    /// <value>
    /// A <see cref="ParameterType"/> indicating the type that the function
    /// is expected to return.
    /// </value>
    public ParameterType ReturnType { get; }

    /// <summary>
    /// Gets the minimum number of arguments required when calling this function.
    /// </summary>
    /// <value>
    /// The count of required (non-optional) parameters in the function signature.
    /// </value>
    public int MinimumArgumentCount { get; }

    /// <summary>
    /// Gets the maximum number of arguments accepted when calling this function.
    /// </summary>
    /// <value>
    /// The total count of all parameters (required and optional) in the function signature.
    /// </value>
    public int MaximumArgumentCount { get; }

    /// <summary>
    /// Validates the provided arguments against this function signature,
    /// checking both argument count and type compatibility.
    /// </summary>
    /// <param name="arguments">
    /// The collection of arguments to validate against the signature.
    /// </param>
    /// <param name="lineNumber">
    /// The line number in source code where the function call occurs, used for error reporting.
    /// </param>
    /// <param name="columnPosition">
    /// The column position in source code where the function call occurs, used for error reporting.
    /// </param>
    /// <returns>
    /// A <see cref="ValidationResult"/> indicating whether the arguments are valid
    /// and providing detailed error information if validation fails.
    /// </returns>
    public ValidationResult ValidateArguments(IReadOnlyList<JyroValue> arguments, int lineNumber, int columnPosition)
    {
        var argumentCountValidation = ValidateArgumentCount(arguments, lineNumber, columnPosition);
        if (!argumentCountValidation.IsSuccessful)
        {
            return argumentCountValidation;
        }

        return ValidateArgumentTypes(arguments, lineNumber, columnPosition);
    }

    /// <summary>
    /// Returns a string representation of the function signature in a readable format.
    /// </summary>
    /// <returns>
    /// A string containing the function name, parameter list, and return type.
    /// </returns>
    public override string ToString()
    {
        var parameterString = string.Join(", ", Parameters);
        return $"{Name}({parameterString}): {ReturnType}";
    }

    /// <summary>
    /// Validates that required parameters appear before optional parameters in the parameter list.
    /// </summary>
    /// <param name="parameters">The parameter collection to validate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when a required parameter is found after an optional parameter.
    /// </exception>
    private static void ValidateParameterOrdering(IReadOnlyList<Parameter> parameters)
    {
        bool hasSeenOptionalParameter = false;

        foreach (var parameter in parameters)
        {
            if (hasSeenOptionalParameter && !parameter.IsOptional)
            {
                throw new ArgumentException("Required parameters cannot be defined after optional parameters");
            }

            if (parameter.IsOptional)
            {
                hasSeenOptionalParameter = true;
            }
        }
    }

    /// <summary>
    /// Validates that the argument count falls within the acceptable range for this function.
    /// </summary>
    /// <param name="arguments">The arguments to validate.</param>
    /// <param name="lineNumber">The source line number for error reporting.</param>
    /// <param name="columnPosition">The source column position for error reporting.</param>
    /// <returns>A validation result indicating success or failure with details.</returns>
    private ValidationResult ValidateArgumentCount(IReadOnlyList<JyroValue> arguments, int lineNumber, int columnPosition)
    {
        if (arguments.Count < MinimumArgumentCount)
        {
            return ValidationResult.Failure(
                MessageCode.InvalidNumberArguments,
                lineNumber, columnPosition,
                $"Function '{Name}' requires at least {MinimumArgumentCount} arguments, but received {arguments.Count}");
        }

        if (arguments.Count > MaximumArgumentCount)
        {
            return ValidationResult.Failure(
                MessageCode.InvalidNumberArguments,
                lineNumber, columnPosition,
                $"Function '{Name}' accepts at most {MaximumArgumentCount} arguments, but received {arguments.Count}");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that each argument's type is compatible with the corresponding parameter type.
    /// </summary>
    /// <param name="arguments">The arguments to validate.</param>
    /// <param name="lineNumber">The source line number for error reporting.</param>
    /// <param name="columnPosition">The source column position for error reporting.</param>
    /// <returns>A validation result indicating success or failure with details.</returns>
    private ValidationResult ValidateArgumentTypes(IReadOnlyList<JyroValue> arguments, int lineNumber, int columnPosition)
    {
        for (int argumentIndex = 0; argumentIndex < arguments.Count; argumentIndex++)
        {
            var parameter = Parameters[argumentIndex];
            var argument = arguments[argumentIndex];

            if (!parameter.IsValidValue(argument))
            {
                return ValidationResult.Failure(
                    MessageCode.InvalidArgumentType,
                    lineNumber, columnPosition,
                    $"Function '{Name}' parameter '{parameter.Name}' expects {parameter.Type}, but received {argument.Type}");
            }
        }

        return ValidationResult.Success();
    }
}