namespace Mesch.Jyro;

/// <summary>
/// Represents a string value in the Jyro runtime system.
/// Provides support for string operations, indexing, and concatenation.
/// </summary>
public sealed class JyroString : JyroValue
{
    /// <summary>
    /// Initializes a new instance of the JyroString class with the specified string value.
    /// </summary>
    /// <param name="value">The string value to represent.</param>
    /// <exception cref="ArgumentNullException">Thrown when the value parameter is null.</exception>
    public JyroString(string value) => Value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary>
    /// Gets the character at the specified index as a JyroString containing a single character.
    /// Returns JyroNull.Instance if the index is out of bounds.
    /// </summary>
    /// <param name="index">The zero-based index of the character to retrieve.</param>
    /// <returns>A JyroString containing the character at the specified index, or JyroNull if the index is invalid.</returns>
    public JyroValue this[int index]
    {
        get => (index >= 0 && index < Value.Length)
            ? new JyroString(Value[index].ToString())
            : JyroNull.Instance;
    }

    /// <summary>
    /// Gets the string value represented by this instance.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the type of this value, which is always String.
    /// </summary>
    public override JyroValueType Type => JyroValueType.String;

    /// <summary>
    /// Gets the length of the string in characters.
    /// </summary>
    public int Length => Value.Length;

    /// <summary>
    /// Converts this value to a JyroString instance.
    /// </summary>
    /// <returns>This instance.</returns>
    public override JyroString AsString() => this;

    /// <summary>
    /// Gets the character at the specified index for string indexing operations.
    /// Returns JyroNull.Instance if the index is not a number or is out of bounds.
    /// </summary>
    /// <param name="index">The index value used to access the character.</param>
    /// <returns>A JyroString containing the character at the specified index, or JyroNull if invalid.</returns>
    public override JyroValue GetIndex(JyroValue index)
    {
        if (index is JyroNumber numberIndex)
        {
            var indexValue = numberIndex.ToInteger();
            return (indexValue >= 0 && indexValue < Value.Length)
                ? new JyroString(Value[indexValue].ToString())
                : JyroNull.Instance;
        }
        return JyroNull.Instance;
    }

    /// <summary>
    /// Evaluates a binary operation with this string as the left operand.
    /// Supports concatenation with any value type and comparison with other strings.
    /// </summary>
    /// <param name="operator">The binary operator to apply.</param>
    /// <param name="right">The right operand for the binary operation.</param>
    /// <returns>The result of the binary operation.</returns>
    public override JyroValue EvaluateBinary(JyroTokenType @operator, JyroValue right)
    {
        if (@operator == JyroTokenType.Plus)
        {
            return new JyroString(Value + right.ToStringValue());
        }

        if (right is JyroString rightString)
        {
            return @operator switch
            {
                JyroTokenType.Less => JyroBoolean.FromBoolean(string.Compare(Value, rightString.Value, StringComparison.Ordinal) < 0),
                JyroTokenType.LessEqual => JyroBoolean.FromBoolean(string.Compare(Value, rightString.Value, StringComparison.Ordinal) <= 0),
                JyroTokenType.Greater => JyroBoolean.FromBoolean(string.Compare(Value, rightString.Value, StringComparison.Ordinal) > 0),
                JyroTokenType.GreaterEqual => JyroBoolean.FromBoolean(string.Compare(Value, rightString.Value, StringComparison.Ordinal) >= 0),
                _ => base.EvaluateBinary(@operator, right)
            };
        }

        return base.EvaluateBinary(@operator, right);
    }

    /// <summary>
    /// Converts this string to an enumerable sequence of single-character strings for iteration.
    /// Each character in the string becomes a separate JyroString value.
    /// </summary>
    /// <returns>An enumerable sequence of JyroString instances representing each character.</returns>
    public override IEnumerable<JyroValue> ToIterable()
    {
        return Value.Select(character => new JyroString(character.ToString()));
    }

    /// <summary>
    /// Converts this string value to its .NET object representation.
    /// </summary>
    /// <returns>The string value.</returns>
    public override object ToObjectValue() => Value;

    /// <summary>
    /// Converts this string value to its string representation.
    /// </summary>
    /// <returns>The string value.</returns>
    public override string ToStringValue() => Value;

    /// <summary>
    /// Converts this string value to a boolean using truthiness rules.
    /// Empty or null strings are considered false, all other strings are considered true.
    /// </summary>
    /// <returns>False if the string is null or empty, otherwise true.</returns>
    public override bool ToBoolean() => !string.IsNullOrEmpty(Value);

    /// <summary>
    /// Returns the hash code for this string value.
    /// </summary>
    /// <returns>The hash code of the underlying string value.</returns>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// Determines whether this string value is equal to another JyroValue.
    /// </summary>
    /// <param name="other">The JyroValue to compare with this instance.</param>
    /// <returns>True if the other value is a JyroString with the same string value, otherwise false.</returns>
    public override bool Equals(JyroValue? other) => other is JyroString stringValue && stringValue.Value == Value;
}