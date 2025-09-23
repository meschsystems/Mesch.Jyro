namespace Mesch.Jyro;

/// <summary>
/// Represents a null value in the Jyro runtime system.
/// This class implements the singleton pattern to ensure only one null instance exists.
/// </summary>
public sealed class JyroNull : JyroValue
{
    /// <summary>
    /// Prevents external instantiation of JyroNull.
    /// </summary>
    private JyroNull() { }

    /// <summary>
    /// Gets the singleton instance of JyroNull.
    /// </summary>
    public static JyroNull Instance { get; } = new JyroNull();

    /// <summary>
    /// Gets the type of this value, which is always Null.
    /// </summary>
    public override JyroValueType Type => JyroValueType.Null;

    /// <summary>
    /// Gets a value indicating whether this represents a null value.
    /// Always returns true for JyroNull instances.
    /// </summary>
    public override bool IsNull => true;

    /// <summary>
    /// Converts this null value to its .NET object representation.
    /// </summary>
    /// <returns>Always returns null.</returns>
    public override object? ToObjectValue() => null;

    /// <summary>
    /// Converts this null value to its string representation.
    /// </summary>
    /// <returns>Always returns an empty string.</returns>
    public override string ToStringValue() => string.Empty;

    /// <summary>
    /// Returns the hash code for this null value.
    /// </summary>
    /// <returns>Always returns 0 for null values.</returns>
    public override int GetHashCode() => 0;

    /// <summary>
    /// Determines whether this null value is equal to another JyroValue.
    /// </summary>
    /// <param name="other">The JyroValue to compare with this instance.</param>
    /// <returns>True if the other value is also a JyroNull, otherwise false.</returns>
    public override bool Equals(JyroValue? other) => other is JyroNull;
}