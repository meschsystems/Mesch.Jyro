namespace Mesch.Jyro;

/// <summary>
/// Represents a formal parameter definition for a Jyro function, including
/// the parameter name, expected type, and whether the parameter is optional.
/// Used by function signatures for compile-time type checking and validation.
/// </summary>
public sealed class Parameter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Parameter"/> class
    /// with the specified name, type, and optional flag.
    /// </summary>
    /// <param name="parameterName">
    /// The name of the parameter. Cannot be null.
    /// Used for documentation and error reporting.
    /// </param>
    /// <param name="parameterType">
    /// The expected type for values passed to this parameter.
    /// </param>
    /// <param name="isOptionalParameter">
    /// A value indicating whether this parameter is optional.
    /// Defaults to <c>false</c> (required parameter).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="parameterName"/> is null.
    /// </exception>
    public Parameter(string parameterName, ParameterType parameterType, bool isOptionalParameter = false)
    {
        Name = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
        Type = parameterType;
        IsOptional = isOptionalParameter;
    }

    /// <summary>
    /// Gets the name of the parameter, used for documentation and error messages.
    /// </summary>
    /// <value>
    /// The parameter name as defined in the function signature.
    /// </value>
    public string Name { get; }

    /// <summary>
    /// Gets the expected type for values passed to this parameter.
    /// </summary>
    /// <value>
    /// A <see cref="ParameterType"/> indicating the type requirement for this parameter.
    /// </value>
    public ParameterType Type { get; }

    /// <summary>
    /// Gets a value indicating whether this parameter is optional and may be omitted
    /// when calling the function.
    /// </summary>
    /// <value>
    /// <c>true</c> if the parameter is optional; otherwise, <c>false</c> for required parameters.
    /// </value>
    public bool IsOptional { get; }

    /// <summary>
    /// Validates that the specified value matches this parameter's type requirements.
    /// </summary>
    /// <param name="argumentValue">
    /// The value to validate against this parameter's type.
    /// </param>
    /// <returns>
    /// <c>true</c> if the value is compatible with this parameter's type;
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool IsValidValue(JyroValue argumentValue)
    {
        if (Type == ParameterType.Any)
        {
            return true;
        }

        return Type switch
        {
            ParameterType.Number => argumentValue is JyroNumber,
            ParameterType.String => argumentValue is JyroString,
            ParameterType.Boolean => argumentValue is JyroBoolean,
            ParameterType.Object => argumentValue is JyroObject,
            ParameterType.Array => argumentValue is JyroArray,
            ParameterType.Null => argumentValue is JyroNull,
            _ => false
        };
    }

    /// <summary>
    /// Returns a string representation of the parameter definition.
    /// </summary>
    /// <returns>
    /// A string containing the parameter name and type, with optional indicator
    /// if the parameter is optional.
    /// </returns>
    public override string ToString() => IsOptional ? $"{Name}?: {Type}" : $"{Name}: {Type}";
}