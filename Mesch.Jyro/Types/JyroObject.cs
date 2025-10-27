namespace Mesch.Jyro;

/// <summary>
/// Represents an object value in the Jyro runtime system.
/// Provides property access, manipulation, and iteration over key-value pairs.
/// Objects can be indexed by string keys and support dynamic property assignment.
/// </summary>
public sealed class JyroObject : JyroValue, IEnumerable<KeyValuePair<string, JyroValue>>
{
    private readonly Dictionary<string, JyroValue> _properties = new();

    /// <summary>
    /// Gets the type of this value, which is always Object.
    /// </summary>
    public override JyroValueType Type => JyroValueType.Object;

    /// <summary>
    /// Gets the number of properties in this object.
    /// </summary>
    public int Count => _properties.Count;

    /// <summary>
    /// Gets or sets the property value for the specified key.
    /// Returns JyroNull.Instance for non-existent properties.
    /// Setting a property to null stores JyroNull.Instance.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <returns>The property value, or JyroNull.Instance if the property does not exist.</returns>
    public JyroValue this[string key]
    {
        get => GetProperty(key);
        set => SetProperty(key, value);
    }

    /// <summary>
    /// Converts this value to a JyroObject instance.
    /// </summary>
    /// <returns>This instance.</returns>
    public override JyroObject AsObject() => this;

    /// <summary>
    /// Gets the value of the property with the specified key.
    /// Supports nested path notation using dots (e.g., "address.city").
    /// </summary>
    /// <param name="key">The property key or path to retrieve. Use dot notation for nested properties.</param>
    /// <returns>
    /// The property value, or JyroNull.Instance if the property does not exist or
    /// any intermediate path segment is not an object.
    /// </returns>
    public override JyroValue GetProperty(string key)
    {
        // Check if the key contains a dot, indicating a nested path
        if (key.Contains('.'))
        {
            var pathSegments = key.Split('.');
            var currentValue = (JyroValue)this;

            foreach (var segment in pathSegments)
            {
                if (currentValue is JyroObject currentObject)
                {
                    currentValue = currentObject._properties.TryGetValue(segment, out var value)
                        ? value
                        : JyroNull.Instance;
                }
                else
                {
                    // Path traversal failed - intermediate value is not an object
                    return JyroNull.Instance;
                }
            }

            return currentValue;
        }

        // Simple property lookup (no nested path)
        return _properties.TryGetValue(key, out var propertyValue) ? propertyValue : JyroNull.Instance;
    }

    /// <summary>
    /// Sets the value of the property with the specified key.
    /// If the property does not exist, it is created.
    /// </summary>
    /// <param name="key">The property key to set.</param>
    /// <param name="value">The value to assign. Null values are converted to JyroNull.Instance.</param>
    public override void SetProperty(string key, JyroValue value)
    {
        _properties[key] = value ?? JyroNull.Instance;
    }

    /// <summary>
    /// Gets the property value for string-based indexing operations.
    /// Returns JyroNull.Instance if the index is not a string.
    /// </summary>
    /// <param name="index">The index value used to access the property (must be a JyroString).</param>
    /// <returns>The property value, or JyroNull if the index is invalid or property does not exist.</returns>
    public override JyroValue GetIndex(JyroValue index)
    {
        if (index is JyroString stringIndex)
        {
            return GetProperty(stringIndex.Value);
        }
        return JyroNull.Instance;
    }

    /// <summary>
    /// Sets the property value for string-based indexing operations.
    /// Ignores operations with non-string indices.
    /// </summary>
    /// <param name="index">The index value used to set the property (must be a JyroString).</param>
    /// <param name="value">The value to assign to the property.</param>
    public override void SetIndex(JyroValue index, JyroValue value)
    {
        if (index is JyroString stringIndex)
        {
            SetProperty(stringIndex.Value, value);
        }
    }

    /// <summary>
    /// Converts this object to an enumerable sequence of its property values for iteration.
    /// When used in foreach loops, iterates over the values, not the key-value pairs.
    /// </summary>
    /// <returns>An enumerable sequence of the object's property values.</returns>
    public override IEnumerable<JyroValue> ToIterable()
    {
        return _properties.Values;
    }

    /// <summary>
    /// Creates a JyroObject from a dictionary of .NET objects.
    /// Each value is converted to its corresponding JyroValue representation.
    /// </summary>
    /// <param name="dictionary">The dictionary to convert.</param>
    /// <returns>A new JyroObject containing the converted key-value pairs.</returns>
    public static JyroObject FromDictionary(IDictionary<string, object?> dictionary)
    {
        var objectResult = new JyroObject();
        foreach (var keyValuePair in dictionary)
        {
            var key = keyValuePair.Key ?? string.Empty;
            objectResult.SetProperty(key, JyroValue.FromObject(keyValuePair.Value));
        }
        return objectResult;
    }

    /// <summary>
    /// Creates a JyroObject from a .NET object using reflection to extract public properties.
    /// Only readable properties are included in the resulting object.
    /// </summary>
    /// <param name="value">The .NET object to convert.</param>
    /// <returns>A new JyroObject containing the object's public properties.</returns>
    public static JyroObject FromComplexObject(object value)
    {
        var objectResult = new JyroObject();
        var properties = value.GetType().GetProperties();
        foreach (var property in properties)
        {
            if (property.CanRead)
            {
                var propertyValue = property.GetValue(value);
                objectResult.SetProperty(property.Name, JyroValue.FromObject(propertyValue));
            }
        }
        return objectResult;
    }

    /// <summary>
    /// Converts this object to a .NET Dictionary with object values.
    /// Each JyroValue is converted to its .NET object representation.
    /// </summary>
    /// <returns>A Dictionary containing the .NET object representations of all properties.</returns>
    public Dictionary<string, object?> ToDictionary()
    {
        var dictionary = new Dictionary<string, object?>();
        foreach (var keyValuePair in _properties)
        {
            dictionary[keyValuePair.Key] = keyValuePair.Value.ToObjectValue();
        }
        return dictionary;
    }

    /// <summary>
    /// Attempts to get the value of the specified property.
    /// </summary>
    /// <param name="key">The property key to retrieve.</param>
    /// <param name="value">When this method returns, contains the property value if found, or JyroNull.Instance if not found.</param>
    /// <returns>True if the property exists, otherwise false.</returns>
    public bool TryGet(string key, out JyroValue value)
    {
        if (_properties.TryGetValue(key, out value!))
        {
            return true;
        }
        value = JyroNull.Instance;
        return false;
    }

    /// <summary>
    /// Attempts to get the value of the specified property.
    /// This method provides compatibility with dictionary interfaces.
    /// </summary>
    /// <param name="key">The property key to retrieve.</param>
    /// <param name="value">When this method returns, contains the property value if found, or JyroNull.Instance if not found.</param>
    /// <returns>True if the property exists, otherwise false.</returns>
    public bool TryGetValue(string key, out JyroValue value) => TryGet(key, out value);

    /// <summary>
    /// Removes the property with the specified key from the object.
    /// </summary>
    /// <param name="key">The key of the property to remove.</param>
    /// <returns>True if the property was found and removed, otherwise false.</returns>
    public bool Remove(string key) => _properties.Remove(key);

    /// <summary>
    /// Removes all properties from the object.
    /// </summary>
    public void Clear() => _properties.Clear();

    /// <summary>
    /// Returns an enumerator that iterates through the object's key-value pairs.
    /// </summary>
    /// <returns>An enumerator for the object's properties.</returns>
    public IEnumerator<KeyValuePair<string, JyroValue>> GetEnumerator() => _properties.GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the object's key-value pairs.
    /// </summary>
    /// <returns>An enumerator for the object's properties.</returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _properties.GetEnumerator();

    /// <summary>
    /// Converts this object to its .NET object representation as a Dictionary.
    /// </summary>
    /// <returns>A Dictionary containing the .NET object representations of all properties.</returns>
    public override object ToObjectValue() => _properties.ToDictionary(property => property.Key, property => property.Value.ToObjectValue());

    /// <summary>
    /// Converts this object to its string representation in JSON-like format.
    /// </summary>
    /// <returns>A string representation of the object showing all key-value pairs.</returns>
    public override string ToStringValue() => $"{{{string.Join(", ", _properties.Select(property => $"{property.Key}: {property.Value.ToStringValue()}"))}}}";

    /// <summary>
    /// Converts this object to a boolean using truthiness rules.
    /// Empty objects are considered false, objects with properties are considered true.
    /// </summary>
    /// <returns>False if the object has no properties, otherwise true.</returns>
    public override bool ToBoolean() => _properties.Count > 0;

    /// <summary>
    /// Returns the hash code for this object value.
    /// </summary>
    /// <returns>The hash code of the underlying dictionary.</returns>
    public override int GetHashCode() => _properties.GetHashCode();

    /// <summary>
    /// Determines whether this object is equal to another JyroValue.
    /// Objects are considered equal if they contain the same key-value pairs.
    /// The order of properties does not affect equality comparison.
    /// </summary>
    /// <param name="other">The JyroValue to compare with this instance.</param>
    /// <returns>True if the other value is a JyroObject with equivalent properties, otherwise false.</returns>
    public override bool Equals(JyroValue? other) =>
        other is JyroObject objectValue &&
        _properties.OrderBy(property => property.Key).SequenceEqual(objectValue._properties.OrderBy(property => property.Key));
}