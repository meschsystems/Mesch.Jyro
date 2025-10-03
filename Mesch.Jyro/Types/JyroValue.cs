using System.Text.Json;
using Antlr4.CodeGenerator;

namespace Mesch.Jyro;

/// <summary>
/// Represents the base class for all runtime values in the Jyro language system.
/// Provides core functionality for type checking, conversions, operations, and host interoperability.
/// </summary>
public abstract class JyroValue : IEquatable<JyroValue>
{
    /// <summary>
    /// Gets the specific runtime type of this Jyro value.
    /// </summary>
    public abstract JyroValueType Type { get; }

    /// <summary>
    /// Gets a value indicating whether this instance represents a null value.
    /// </summary>
    public virtual bool IsNull => false;

    #region Property and Index Access

    /// <summary>
    /// Gets a property value by the specified key for object types.
    /// Returns <see cref="JyroNull.Instance"/> for types that do not support property access.
    /// </summary>
    /// <param name="key">The property key to retrieve.</param>
    /// <returns>The property value, or null if the property does not exist or is not supported.</returns>
    public virtual JyroValue GetProperty(string key) => JyroNull.Instance;

    /// <summary>
    /// Sets a property value by the specified key for object types.
    /// This operation is ignored for types that do not support property assignment.
    /// </summary>
    /// <param name="key">The property key to set.</param>
    /// <param name="value">The value to assign to the property.</param>
    public virtual void SetProperty(string key, JyroValue value) { }

    /// <summary>
    /// Gets an element by the specified index for indexable types (arrays, strings, objects).
    /// Returns <see cref="JyroNull.Instance"/> for types that do not support indexing.
    /// </summary>
    /// <param name="index">The index value used to access the element.</param>
    /// <returns>The element at the specified index, or null if the index is invalid or not supported.</returns>
    public virtual JyroValue GetIndex(JyroValue index) => JyroNull.Instance;

    /// <summary>
    /// Sets an element by the specified index for indexable types.
    /// This operation is ignored for types that do not support index assignment.
    /// </summary>
    /// <param name="index">The index value used to set the element.</param>
    /// <param name="value">The value to assign at the specified index.</param>
    public virtual void SetIndex(JyroValue index, JyroValue value) { }

    /// <summary>
    /// Converts this value to an enumerable sequence for use in foreach loops.
    /// Non-iterable types return an empty sequence.
    /// </summary>
    /// <returns>An enumerable sequence of JyroValue instances.</returns>
    public virtual IEnumerable<JyroValue> ToIterable()
    {
        return Enumerable.Empty<JyroValue>();
    }

    #endregion

    #region Binary Operations

    public virtual JyroValue EvaluateBinary(int @operator, JyroValue right)
    {
        return @operator switch
        {
            JyroParser.EQ => JyroBoolean.FromBoolean(this.Equals(right)),
            JyroParser.NEQ => JyroBoolean.FromBoolean(!this.Equals(right)),
            JyroParser.AND => JyroBoolean.FromBoolean(this.ToBooleanTruthiness() && right.ToBooleanTruthiness()),
            JyroParser.OR => JyroBoolean.FromBoolean(this.ToBooleanTruthiness() || right.ToBooleanTruthiness()),
            _ => throw new InvalidOperationException($"Unsupported binary operation {@operator} for {Type}")
        };
    }

    public virtual JyroValue EvaluateUnary(int @operator)
    {
        return @operator switch
        {
            JyroParser.NOT => JyroBoolean.FromBoolean(!this.ToBooleanTruthiness()),
            _ => throw new InvalidOperationException($"Unsupported unary operation {@operator} for {Type}")
        };
    }

    #endregion

    #region Type Conversions

    /// <summary>
    /// Converts this value to a <see cref="JyroObject"/> instance.
    /// </summary>
    /// <returns>This value as a JyroObject.</returns>
    /// <exception cref="InvalidCastException">Thrown when this value cannot be converted to an object.</exception>
    public virtual JyroObject AsObject() => throw new InvalidCastException($"Cannot cast {Type} to Object");

    /// <summary>
    /// Converts this value to a <see cref="JyroArray"/> instance.
    /// </summary>
    /// <returns>This value as a JyroArray.</returns>
    /// <exception cref="InvalidCastException">Thrown when this value cannot be converted to an array.</exception>
    public virtual JyroArray AsArray() => throw new InvalidCastException($"Cannot cast {Type} to Array");

    /// <summary>
    /// Converts this value to a <see cref="JyroString"/> instance.
    /// </summary>
    /// <returns>This value as a JyroString.</returns>
    /// <exception cref="InvalidCastException">Thrown when this value cannot be converted to a string.</exception>
    public virtual JyroString AsString() => throw new InvalidCastException($"Cannot cast {Type} to String");

    /// <summary>
    /// Converts this value to a <see cref="JyroNumber"/> instance.
    /// </summary>
    /// <returns>This value as a JyroNumber.</returns>
    /// <exception cref="InvalidCastException">Thrown when this value cannot be converted to a number.</exception>
    public virtual JyroNumber AsNumber() => throw new InvalidCastException($"Cannot cast {Type} to Number");

    /// <summary>
    /// Converts this value to a <see cref="JyroBoolean"/> instance.
    /// </summary>
    /// <returns>This value as a JyroBoolean.</returns>
    /// <exception cref="InvalidCastException">Thrown when this value cannot be converted to a boolean.</exception>
    public virtual JyroBoolean AsBoolean() => throw new InvalidCastException($"Cannot cast {Type} to Boolean");

    /// <summary>
    /// Converts this value to a <see cref="JyroArray"/> or returns an empty array if conversion fails.
    /// </summary>
    /// <returns>This value as a JyroArray, or a new empty JyroArray.</returns>
    public JyroArray AsArrayOrEmpty() => this is JyroArray arrayValue ? arrayValue : new JyroArray();

    /// <summary>
    /// Converts this value to a <see cref="JyroObject"/> or returns a new empty object if conversion fails.
    /// </summary>
    /// <returns>This value as a JyroObject, or a new empty JyroObject.</returns>
    public JyroObject AsObjectOrNew() => this is JyroObject objectValue ? objectValue : new JyroObject();

    /// <summary>
    /// Converts this value to a string or returns an empty string if conversion fails.
    /// </summary>
    /// <returns>This value as a string, or an empty string.</returns>
    public string AsStringOrEmpty() => this is JyroString stringValue ? stringValue.Value : string.Empty;

    /// <summary>
    /// Converts this value to a number or returns the specified fallback value if conversion fails.
    /// </summary>
    /// <param name="fallback">The value to return if this instance cannot be converted to a number.</param>
    /// <returns>This value as a double, or the fallback value.</returns>
    public double AsNumberOr(double fallback) => this is JyroNumber numberValue ? numberValue.Value : fallback;

    #endregion

    #region Host Interoperability

    /// <summary>
    /// Converts this Jyro value to the closest equivalent .NET object representation.
    /// This enables seamless integration with host application code.
    /// </summary>
    /// <returns>A .NET object representing this value, or null for JyroNull.</returns>
    public abstract object? ToObjectValue();

    /// <summary>
    /// Serializes this value to a JSON string representation using the specified options.
    /// </summary>
    /// <param name="options">Optional JSON serialization settings.</param>
    /// <returns>A JSON string representing this value.</returns>
    public string ToJson(JsonSerializerOptions? options = null) =>
        JsonSerializer.Serialize(ToObjectValue(), options);

    /// <summary>
    /// Creates a JyroValue instance from a .NET object, automatically determining the appropriate Jyro type.
    /// Supports all primitive types, collections, and complex objects.
    /// </summary>
    /// <param name="value">The .NET object to convert.</param>
    /// <returns>A JyroValue representing the input object.</returns>
    public static JyroValue FromObject(object? value)
    {
        switch (value)
        {
            case null:
                return JyroNull.Instance;
            case JyroValue existingJyroValue:
                return existingJyroValue;
            case bool booleanValue:
                return JyroBoolean.FromBoolean(booleanValue);
            case byte byteValue:
                return new JyroNumber(byteValue);
            case sbyte signedByteValue:
                return new JyroNumber(signedByteValue);
            case short shortValue:
                return new JyroNumber(shortValue);
            case ushort unsignedShortValue:
                return new JyroNumber(unsignedShortValue);
            case int intValue:
                return new JyroNumber(intValue);
            case uint unsignedIntValue:
                return new JyroNumber(unsignedIntValue);
            case long longValue:
                return new JyroNumber(longValue);
            case ulong unsignedLongValue:
                return new JyroNumber((double)unsignedLongValue);
            case float floatValue:
                return new JyroNumber(floatValue);
            case double doubleValue:
                return new JyroNumber(doubleValue);
            case decimal decimalValue:
                return new JyroNumber((double)decimalValue);
            case string stringValue:
                return new JyroString(stringValue);
            case JsonElement jsonElement:
                return FromJsonElement(jsonElement);
            case IDictionary<string, object?> objectDictionary:
                return JyroObject.FromDictionary(objectDictionary);
            case IDictionary<string, JsonElement> jsonDictionary:
                return JyroObject.FromDictionary(jsonDictionary.ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => (object?)keyValuePair.Value));
            case IEnumerable<object?> objectSequence:
                return JyroArray.FromEnumerable(objectSequence);
            case System.Collections.IEnumerable nonGenericEnumerable:
                var itemList = new List<object?>();
                foreach (var item in nonGenericEnumerable)
                {
                    itemList.Add(item);
                }
                return JyroArray.FromEnumerable(itemList);
        }

        return new JyroString(value.ToString() ?? string.Empty);
    }

    /// <summary>
    /// Parses a JSON string and creates the corresponding JyroValue representation.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <param name="options">Optional JSON parsing settings.</param>
    /// <returns>A JyroValue representing the parsed JSON.</returns>
    /// <exception cref="JsonException">Thrown when the JSON string is malformed.</exception>
    public static JyroValue FromJson(string json, JsonSerializerOptions? options = null)
    {
        using var document = JsonDocument.Parse(json, new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        });
        return FromJsonElement(document.RootElement);
    }

    /// <summary>
    /// Converts a JsonElement to its corresponding JyroValue representation.
    /// </summary>
    /// <param name="element">The JsonElement to convert.</param>
    /// <returns>A JyroValue representing the JsonElement.</returns>
    private static JyroValue FromJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return JyroNull.Instance;
            case JsonValueKind.True:
                return JyroBoolean.FromBoolean(true);
            case JsonValueKind.False:
                return JyroBoolean.FromBoolean(false);
            case JsonValueKind.Number:
                if (element.TryGetInt64(out var longValue))
                {
                    return new JyroNumber(longValue);
                }
                if (element.TryGetDouble(out var doubleValue))
                {
                    return new JyroNumber(doubleValue);
                }
                return new JyroNumber(0);
            case JsonValueKind.String:
                return new JyroString(element.GetString() ?? string.Empty);
            case JsonValueKind.Array:
                var arrayResult = new JyroArray();
                foreach (var arrayElement in element.EnumerateArray())
                {
                    arrayResult.Add(FromJsonElement(arrayElement));
                }
                return arrayResult;
            case JsonValueKind.Object:
                var objectResult = new JyroObject();
                foreach (var property in element.EnumerateObject())
                {
                    objectResult[property.Name] = FromJsonElement(property.Value);
                }
                return objectResult;
            default:
                return JyroNull.Instance;
        }
    }

    #endregion

    #region Host Primitive Conversions

    /// <summary>
    /// Converts this value to a 32-bit signed integer.
    /// </summary>
    /// <returns>The integer representation of this value.</returns>
    /// <exception cref="InvalidCastException">Thrown when this value cannot be converted to an integer.</exception>
    public virtual int ToInt32() => throw new InvalidCastException($"Cannot cast {Type} to int");

    /// <summary>
    /// Converts this value to a 64-bit signed integer.
    /// </summary>
    /// <returns>The long integer representation of this value.</returns>
    /// <exception cref="InvalidCastException">Thrown when this value cannot be converted to a long integer.</exception>
    public virtual long ToInt64() => throw new InvalidCastException($"Cannot cast {Type} to long");

    /// <summary>
    /// Converts this value to a double-precision floating-point number.
    /// </summary>
    /// <returns>The double representation of this value.</returns>
    /// <exception cref="InvalidCastException">Thrown when this value cannot be converted to a double.</exception>
    public virtual double ToDouble() => throw new InvalidCastException($"Cannot cast {Type} to double");

    /// <summary>
    /// Converts this value to a decimal number.
    /// </summary>
    /// <returns>The decimal representation of this value.</returns>
    /// <exception cref="InvalidCastException">Thrown when this value cannot be converted to a decimal.</exception>
    public virtual decimal ToDecimal() => throw new InvalidCastException($"Cannot cast {Type} to decimal");

    /// <summary>
    /// Converts this value to a boolean.
    /// </summary>
    /// <returns>The boolean representation of this value.</returns>
    /// <exception cref="InvalidCastException">Thrown when this value cannot be converted to a boolean.</exception>
    public virtual bool ToBoolean() => throw new InvalidCastException($"Cannot cast {Type} to bool");

    /// <summary>
    /// Converts this value to its string representation.
    /// </summary>
    /// <returns>The string representation of this value.</returns>
    /// <exception cref="InvalidCastException">Thrown when this value cannot be converted to a string.</exception>
    public virtual string ToStringValue() => throw new InvalidCastException($"Cannot cast {Type} to string");

    #endregion

    #region Truthiness Evaluation

    /// <summary>
    /// Evaluates the truthiness of this value for use in conditional expressions.
    /// Different value types have different truthiness semantics following JavaScript-like rules.
    /// </summary>
    /// <returns>True if this value is considered truthy in a boolean context, otherwise false.</returns>
    public virtual bool ToBooleanTruthiness()
    {
        return this switch
        {
            JyroNull => false,
            JyroBoolean booleanValue => booleanValue.Value,
            JyroNumber numberValue => numberValue.ToDouble() != 0,
            JyroString stringValue => !string.IsNullOrEmpty(stringValue.Value),
            JyroArray arrayValue => arrayValue.Length > 0,
            JyroObject objectValue => objectValue.Count > 0,
            _ => false
        };
    }

    #endregion

    #region Equality Operations

    /// <summary>
    /// Returns the hash code for this JyroValue instance.
    /// </summary>
    /// <returns>A hash code for this instance.</returns>
    public abstract override int GetHashCode();

    /// <summary>
    /// Determines whether this JyroValue is equal to another JyroValue.
    /// </summary>
    /// <param name="other">The JyroValue to compare with this instance.</param>
    /// <returns>True if the values are equal, otherwise false.</returns>
    public abstract bool Equals(JyroValue? other);

    /// <summary>
    /// Determines whether this JyroValue is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>True if the object is a JyroValue and the values are equal, otherwise false.</returns>
    public override bool Equals(object? obj) => Equals(obj as JyroValue);

    /// <summary>
    /// Determines whether two JyroValue instances are equal.
    /// </summary>
    /// <param name="left">The first JyroValue to compare.</param>
    /// <param name="right">The second JyroValue to compare.</param>
    /// <returns>True if the values are equal, otherwise false.</returns>
    public static bool operator ==(JyroValue? left, JyroValue? right) =>
        ReferenceEquals(left, right) || (left?.Equals(right) ?? false);

    /// <summary>
    /// Determines whether two JyroValue instances are not equal.
    /// </summary>
    /// <param name="left">The first JyroValue to compare.</param>
    /// <param name="right">The second JyroValue to compare.</param>
    /// <returns>True if the values are not equal, otherwise false.</returns>
    public static bool operator !=(JyroValue? left, JyroValue? right) => !(left == right);

    #endregion
}