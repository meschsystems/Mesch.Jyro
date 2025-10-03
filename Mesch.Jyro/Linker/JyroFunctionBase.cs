namespace Mesch.Jyro;

/// <summary>
/// Abstract base class for implementing strongly-typed Jyro functions with
/// flexible type handling and developer-friendly infrastructure. Provides both
/// strict type validation and automatic coercion capabilities to accommodate
/// different function implementation patterns.
/// </summary>
/// <remarks>
/// This class provides the foundation for all Jyro functions, offering multiple
/// strategies for argument handling:
/// <list type="bullet">
/// <item><description>Strict type checking with <see cref="GetArgument{TJyroValue}"/></description></item>
/// <item><description>Automatic coercion with <see cref="GetArgumentWithCoercion{TJyroValue}"/></description></item>
/// <item><description>Convenience methods for common types like <see cref="GetStringArgument"/> and <see cref="GetNumberArgument"/></description></item>
/// </list>
/// Function authors can choose the approach that best fits their needs while maintaining
/// consistent error handling and parameter validation.
/// </remarks>
public abstract class JyroFunctionBase : IJyroFunction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JyroFunctionBase"/> class
    /// with the specified function signature.
    /// </summary>
    /// <param name="functionSignature">
    /// The signature defining the function's parameters and return type.
    /// This signature is used for documentation, IDE support, and basic validation.
    /// Cannot be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="functionSignature"/> is null.
    /// </exception>
    protected JyroFunctionBase(JyroFunctionSignature functionSignature)
    {
        Signature = functionSignature ?? throw new ArgumentNullException(nameof(functionSignature));
    }

    /// <inheritdoc />
    public JyroFunctionSignature Signature { get; }

    /// <inheritdoc />
    public abstract JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext);

    /// <summary>
    /// Safely retrieves and casts the argument at the specified index to the expected type
    /// with strict type validation. No automatic type coercion is performed.
    /// </summary>
    /// <typeparam name="TJyroValue">
    /// The expected <see cref="JyroValue"/> derived type for the argument.
    /// </typeparam>
    /// <param name="arguments">The collection of arguments passed to the function.</param>
    /// <param name="argumentIndex">The zero-based index of the argument to retrieve.</param>
    /// <returns>The argument cast to the specified type.</returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the argument index is out of range, the argument is null when not expected,
    /// or the argument cannot be cast to the expected type.
    /// </exception>
    /// <remarks>
    /// This method provides strict type checking without any automatic coercion.
    /// Use this when your function requires exact type matching and you want to
    /// fail fast with clear error messages for type mismatches.
    /// </remarks>
    protected TJyroValue GetArgument<TJyroValue>(IReadOnlyList<JyroValue> arguments, int argumentIndex)
        where TJyroValue : JyroValue
    {
        if (argumentIndex >= arguments.Count)
        {
            throw new JyroRuntimeException($"Missing required argument at position {argumentIndex} in function '{Signature.Name}'");
        }

        var argument = arguments[argumentIndex];
        if (argument is not TJyroValue typedArgument)
        {
            var expectedTypeName = typeof(TJyroValue).Name.Replace("Jyro", "").ToLowerInvariant();
            throw new JyroRuntimeException($"Function '{Signature.Name}' expects {expectedTypeName} at position {argumentIndex}, but received {argument.Type}");
        }

        return typedArgument;
    }

    /// <summary>
    /// Retrieves the argument at the specified index with automatic type coercion when possible.
    /// Attempts to convert compatible types and provides reasonable default conversions.
    /// </summary>
    /// <typeparam name="TJyroValue">
    /// The expected <see cref="JyroValue"/> derived type for the argument.
    /// </typeparam>
    /// <param name="arguments">The collection of arguments passed to the function.</param>
    /// <param name="argumentIndex">The zero-based index of the argument to retrieve.</param>
    /// <returns>The argument converted to the specified type.</returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the argument index is out of range or the argument cannot be
    /// converted to the expected type through available coercion rules.
    /// </exception>
    /// <remarks>
    /// This method provides automatic type coercion for common scenarios:
    /// <list type="bullet">
    /// <item><description>Null values can be coerced to empty arrays or empty strings</description></item>
    /// <item><description>Any value can be converted to string using its string representation</description></item>
    /// <item><description>Numbers can be coerced between integer and floating-point representations</description></item>
    /// </list>
    /// Use this when your function can reasonably work with converted values and you want
    /// to provide a more flexible, user-friendly experience.
    /// </remarks>
    protected TJyroValue GetArgumentWithCoercion<TJyroValue>(IReadOnlyList<JyroValue> arguments, int argumentIndex)
        where TJyroValue : JyroValue
    {
        if (argumentIndex >= arguments.Count)
        {
            throw new JyroRuntimeException($"Missing required argument at position {argumentIndex} in function '{Signature.Name}'");
        }

        var argument = arguments[argumentIndex];

        // Direct type match - no coercion needed
        if (argument is TJyroValue typedArgument)
        {
            return typedArgument;
        }

        // Attempt standard coercions based on target type
        if (typeof(TJyroValue) == typeof(JyroArray))
        {
            if (argument.IsNull)
            {
                return (TJyroValue)(object)new JyroArray();
            }
        }
        else if (typeof(TJyroValue) == typeof(JyroString))
        {
            if (argument.IsNull)
            {
                return (TJyroValue)(object)new JyroString("");
            }
            // Any value can be converted to string
            return (TJyroValue)(object)new JyroString(argument.ToString() ?? "");
        }
        else if (typeof(TJyroValue) == typeof(JyroObject))
        {
            if (argument.IsNull)
            {
                return (TJyroValue)(object)new JyroObject();
            }
        }
        else if (typeof(TJyroValue) == typeof(JyroBoolean))
        {
            // Convert to boolean using truthiness rules
            var truthiness = argument.ToBooleanTruthiness();
            return (TJyroValue)(object)JyroBoolean.FromBoolean(truthiness);
        }

        var expectedTypeName = typeof(TJyroValue).Name.Replace("Jyro", "").ToLowerInvariant();
        throw new JyroRuntimeException($"Cannot convert {argument.Type} to {expectedTypeName} in function '{Signature.Name}' at position {argumentIndex}");
    }

    /// <summary>
    /// Retrieves an optional argument at the specified index, returning null
    /// if the argument is not provided or cannot be cast to the expected type.
    /// </summary>
    /// <typeparam name="TJyroValue">
    /// The expected <see cref="JyroValue"/> derived type for the argument.
    /// </typeparam>
    /// <param name="arguments">The collection of arguments passed to the function.</param>
    /// <param name="argumentIndex">The zero-based index of the argument to retrieve.</param>
    /// <returns>
    /// The argument cast to the specified type, or null if the argument is not
    /// available or cannot be cast to the expected type.
    /// </returns>
    /// <remarks>
    /// This method is useful for handling optional parameters in function signatures.
    /// It performs strict type checking but returns null rather than throwing an
    /// exception when the argument is missing or incompatible.
    /// </remarks>
    protected static TJyroValue? GetOptionalArgument<TJyroValue>(IReadOnlyList<JyroValue> arguments, int argumentIndex)
        where TJyroValue : JyroValue
    {
        return argumentIndex < arguments.Count ? arguments[argumentIndex] as TJyroValue : null;
    }

    /// <summary>
    /// Convenience method for retrieving string arguments with automatic coercion.
    /// Converts the argument to a string representation if it's not already a string.
    /// </summary>
    /// <param name="arguments">The collection of arguments passed to the function.</param>
    /// <param name="argumentIndex">The zero-based index of the argument to retrieve.</param>
    /// <returns>The string value of the argument.</returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the argument index is out of range.
    /// </exception>
    /// <remarks>
    /// This is a convenience method that handles the common case of needing string
    /// values from arguments. It automatically converts any argument type to its
    /// string representation, making functions more flexible when working with
    /// mixed data types.
    /// </remarks>
    protected string GetStringArgument(IReadOnlyList<JyroValue> arguments, int argumentIndex)
    {
        return GetArgumentWithCoercion<JyroString>(arguments, argumentIndex).Value;
    }

    /// <summary>
    /// Convenience method for retrieving numeric arguments with automatic coercion.
    /// Attempts to convert the argument to a numeric value if it's not already a number.
    /// </summary>
    /// <param name="arguments">The collection of arguments passed to the function.</param>
    /// <param name="argumentIndex">The zero-based index of the argument to retrieve.</param>
    /// <returns>The numeric value of the argument.</returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the argument index is out of range or the argument cannot be
    /// converted to a numeric value.
    /// </exception>
    /// <remarks>
    /// This convenience method extracts numeric values from arguments. Currently,
    /// it requires the argument to already be a <see cref="JyroNumber"/>. Future
    /// enhancements could add string-to-number parsing for greater flexibility.
    /// </remarks>
    protected double GetNumberArgument(IReadOnlyList<JyroValue> arguments, int argumentIndex)
    {
        return GetArgument<JyroNumber>(arguments, argumentIndex).Value;
    }

    /// <summary>
    /// Convenience method for retrieving boolean arguments with automatic coercion.
    /// Converts the argument to a boolean value using Jyro truthiness rules.
    /// </summary>
    /// <param name="arguments">The collection of arguments passed to the function.</param>
    /// <param name="argumentIndex">The zero-based index of the argument to retrieve.</param>
    /// <returns>The boolean value of the argument based on truthiness evaluation.</returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the argument index is out of range.
    /// </exception>
    /// <remarks>
    /// This method applies Jyro's truthiness rules to convert any argument to a boolean:
    /// <list type="bullet">
    /// <item><description>null, empty strings, zero numbers evaluate to false</description></item>
    /// <item><description>All other values evaluate to true</description></item>
    /// </list>
    /// </remarks>
    protected bool GetBooleanArgument(IReadOnlyList<JyroValue> arguments, int argumentIndex)
    {
        return GetArgumentWithCoercion<JyroBoolean>(arguments, argumentIndex).Value;
    }

    /// <summary>
    /// Convenience method for retrieving array arguments with automatic coercion.
    /// Creates an empty array if the argument is null.
    /// </summary>
    /// <param name="arguments">The collection of arguments passed to the function.</param>
    /// <param name="argumentIndex">The zero-based index of the argument to retrieve.</param>
    /// <returns>The array argument or an empty array if the argument was null.</returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the argument index is out of range or the argument cannot be
    /// converted to an array.
    /// </exception>
    /// <remarks>
    /// This method provides automatic coercion of null values to empty arrays,
    /// which is often the desired behavior for array-manipulating functions.
    /// This allows functions to work more intuitively with uninitialized variables.
    /// </remarks>
    protected JyroArray GetArrayArgument(IReadOnlyList<JyroValue> arguments, int argumentIndex)
    {
        return GetArgumentWithCoercion<JyroArray>(arguments, argumentIndex);
    }

    /// <summary>
    /// Convenience method for retrieving object arguments with automatic coercion.
    /// Creates an empty object if the argument is null.
    /// </summary>
    /// <param name="arguments">The collection of arguments passed to the function.</param>
    /// <param name="argumentIndex">The zero-based index of the argument to retrieve.</param>
    /// <returns>The object argument or an empty object if the argument was null.</returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the argument index is out of range or the argument cannot be
    /// converted to an object.
    /// </exception>
    /// <remarks>
    /// This method provides automatic coercion of null values to empty objects,
    /// enabling functions to work with uninitialized object variables more gracefully.
    /// </remarks>
    protected JyroObject GetObjectArgument(IReadOnlyList<JyroValue> arguments, int argumentIndex)
    {
        return GetArgumentWithCoercion<JyroObject>(arguments, argumentIndex);
    }
}