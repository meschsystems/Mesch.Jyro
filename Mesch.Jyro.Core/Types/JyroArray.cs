namespace Mesch.Jyro;

/// <summary>
/// Represents an array value in the Jyro runtime system.
/// Provides dynamic indexing, iteration support, and array manipulation operations.
/// Arrays automatically expand when elements are assigned to indices beyond the current length.
/// </summary>
public sealed class JyroArray : JyroValue, IEnumerable<JyroValue>
{
    private readonly List<JyroValue> _items = new();

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// Reading from an invalid index returns JyroNull.Instance.
    /// Setting to an index beyond the current length expands the array with null values.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index, or JyroNull.Instance if the index is invalid.</returns>
    public JyroValue this[int index]
    {
        get => (index >= 0 && index < _items.Count) ? _items[index] : JyroNull.Instance;
        set
        {
            if (index >= 0)
            {
                while (_items.Count <= index)
                {
                    _items.Add(JyroNull.Instance);
                }
                _items[index] = value ?? JyroNull.Instance;
            }
        }
    }

    /// <summary>
    /// Gets the type of this value, which is always Array.
    /// </summary>
    public override JyroValueType Type => JyroValueType.Array;

    /// <summary>
    /// Gets the number of elements in the array.
    /// </summary>
    public int Length => _items.Count;

    /// <summary>
    /// Adds an element to the end of the array.
    /// </summary>
    /// <param name="value">The value to add. Null values are converted to JyroNull.Instance.</param>
    public void Add(JyroValue value) => _items.Add(value ?? JyroNull.Instance);

    /// <summary>
    /// Inserts an element at the specified index, shifting subsequent elements to higher indices.
    /// </summary>
    /// <param name="index">The zero-based index at which to insert the element.</param>
    /// <param name="value">The value to insert. Null values are converted to JyroNull.Instance.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is negative or greater than Length.</exception>
    public void Insert(int index, JyroValue value) => _items.Insert(index, value ?? JyroNull.Instance);

    /// <summary>
    /// Removes the element at the specified index, shifting subsequent elements to lower indices.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
    public void RemoveAt(int index) => _items.RemoveAt(index);

    /// <summary>
    /// Removes all elements from the array.
    /// </summary>
    public void Clear() => _items.Clear();

    /// <summary>
    /// Gets the number of elements in the array.
    /// This method provides compatibility with collection interfaces.
    /// </summary>
    /// <returns>The number of elements in the array.</returns>
    public int Count() => _items.Count;

    /// <summary>
    /// Creates a JyroArray from an enumerable collection of .NET objects.
    /// Each object is converted to its corresponding JyroValue representation.
    /// </summary>
    /// <param name="items">The collection of objects to convert.</param>
    /// <returns>A new JyroArray containing the converted values.</returns>
    public static JyroArray FromEnumerable(IEnumerable<object?> items)
    {
        var arrayResult = new JyroArray();
        foreach (var item in items)
        {
            arrayResult.Add(JyroValue.FromObject(item));
        }
        return arrayResult;
    }

    /// <summary>
    /// Converts this value to a JyroArray instance.
    /// </summary>
    /// <returns>This instance.</returns>
    public override JyroArray AsArray() => this;

    /// <summary>
    /// Gets the element at the specified index for array indexing operations.
    /// Returns JyroNull.Instance if the index is not a number or is out of bounds.
    /// </summary>
    /// <param name="index">The index value used to access the element.</param>
    /// <returns>The element at the specified index, or JyroNull if invalid.</returns>
    public override JyroValue GetIndex(JyroValue index)
    {
        if (index is JyroNumber numberIndex)
        {
            var indexValue = numberIndex.ToInteger();
            return (indexValue >= 0 && indexValue < _items.Count) ? _items[indexValue] : JyroNull.Instance;
        }
        return JyroNull.Instance;
    }

    /// <summary>
    /// Sets the element at the specified index for array indexing operations.
    /// Automatically expands the array if the index is beyond the current length.
    /// Ignores operations with non-numeric indices or negative indices.
    /// </summary>
    /// <param name="index">The index value used to set the element.</param>
    /// <param name="value">The value to assign at the specified index.</param>
    public override void SetIndex(JyroValue index, JyroValue value)
    {
        if (index is JyroNumber numberIndex)
        {
            var indexValue = numberIndex.ToInteger();
            if (indexValue >= 0)
            {
                while (_items.Count <= indexValue)
                {
                    _items.Add(JyroNull.Instance);
                }
                _items[indexValue] = value ?? JyroNull.Instance;
            }
        }
    }

    /// <summary>
    /// Converts this array to an enumerable sequence for iteration in foreach loops.
    /// </summary>
    /// <returns>An enumerable sequence of the array elements.</returns>
    public override IEnumerable<JyroValue> ToIterable() => _items;

    /// <summary>
    /// Evaluates a binary operation with this array as the left operand.
    /// Supports concatenation with other arrays using the plus operator.
    /// </summary>
    /// <param name="operator">The binary operator to apply.</param>
    /// <param name="right">The right operand for the binary operation.</param>
    /// <returns>The result of the binary operation.</returns>
    public override JyroValue EvaluateBinary(JyroTokenType @operator, JyroValue right)
    {
        if (@operator == JyroTokenType.Plus && right is JyroArray rightArray)
        {
            var result = new JyroArray();
            foreach (var item in _items)
            {
                result.Add(item);
            }
            foreach (var item in rightArray._items)
            {
                result.Add(item);
            }
            return result;
        }

        return base.EvaluateBinary(@operator, right);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the array elements.
    /// </summary>
    /// <returns>An enumerator for the array elements.</returns>
    public IEnumerator<JyroValue> GetEnumerator() => _items.GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the array elements.
    /// </summary>
    /// <returns>An enumerator for the array elements.</returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _items.GetEnumerator();

    /// <summary>
    /// Converts this array to its .NET object representation as a List of objects.
    /// </summary>
    /// <returns>A List containing the .NET object representations of all array elements.</returns>
    public override object ToObjectValue() => _items.Select(valueItem => valueItem.ToObjectValue()).ToList();

    /// <summary>
    /// Converts this array to its string representation in JSON-like format.
    /// </summary>
    /// <returns>A string representation of the array showing all elements.</returns>
    public override string ToStringValue() => $"[{string.Join(", ", _items.Select(item => item.ToStringValue()))}]";

    /// <summary>
    /// Converts this array to a boolean using truthiness rules.
    /// Empty arrays are considered false, arrays with elements are considered true.
    /// </summary>
    /// <returns>False if the array is empty, otherwise true.</returns>
    public override bool ToBoolean() => _items.Count > 0;

    /// <summary>
    /// Returns the hash code for this array value.
    /// </summary>
    /// <returns>The hash code of the underlying list.</returns>
    public override int GetHashCode() => _items.GetHashCode();

    /// <summary>
    /// Determines whether this array is equal to another JyroValue.
    /// Arrays are considered equal if they contain the same elements in the same order.
    /// </summary>
    /// <param name="other">The JyroValue to compare with this instance.</param>
    /// <returns>True if the other value is a JyroArray with equivalent elements, otherwise false.</returns>
    public override bool Equals(JyroValue? other) =>
        other is JyroArray arrayValue && _items.SequenceEqual(arrayValue._items);
}