namespace Mesch.Jyro;

/// <summary>
/// Represents a single property definition within an object literal expression.
/// Object properties define key-value pairs that will be included in the resulting object,
/// where the key is a string identifier and the value is any valid expression.
/// </summary>
/// <remarks>
/// Object properties support different key specification patterns:
/// <list type="bullet">
/// <item><description>Simple identifiers: name: "John"</description></item>
/// <item><description>String literals: "complex key": value</description></item>
/// <item><description>Computed keys: [expression]: value (handled at the expression level)</description></item>
/// </list>
/// During object literal evaluation, the property's value expression is evaluated
/// and the result is stored in the object using the specified key string.
/// </remarks>
public sealed class ObjectProperty
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectProperty"/> class with the specified
    /// key, value expression, and source location information.
    /// </summary>
    /// <param name="key">
    /// The string key that identifies this property within the object.
    /// This key will be used to store and retrieve the property value in the resulting object.
    /// </param>
    /// <param name="value">
    /// The expression that will be evaluated to provide the value for this property.
    /// This can be any valid expression including literals, variables, or complex calculations.
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this property is defined.</param>
    /// <param name="columnPosition">The column position in the source code where this property begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when key or value parameter is null.</exception>
    public ObjectProperty(string key, IExpression value, int lineNumber, int columnPosition)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the string key that identifies this property within the object.
    /// This key is used to store the property value in the resulting object
    /// and enables property access using dot notation or bracket notation.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the expression that provides the value for this property.
    /// This expression is evaluated during object literal creation to determine
    /// the actual value that will be stored for this property key.
    /// </summary>
    public IExpression Value { get; }

    /// <summary>
    /// Gets the line number in the source code where this property is defined.
    /// This information is used for error reporting and debugging purposes.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this property is defined.
    /// This information is used for error reporting and debugging purposes.
    /// </summary>
    public int ColumnPosition { get; }
}