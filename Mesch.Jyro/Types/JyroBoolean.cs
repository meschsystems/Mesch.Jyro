namespace Mesch.Jyro;

/// <summary>
/// Represents a boolean value in the Jyro runtime system.
/// This class implements the flyweight pattern with static True and False instances for performance.
/// </summary>
public sealed class JyroBoolean : JyroValue
{
    /// <summary>
    /// Prevents external instantiation of JyroBoolean.
    /// </summary>
    /// <param name="value">The boolean value to represent.</param>
    internal JyroBoolean(bool value) => Value = value;

    /// <summary>
    /// Gets the boolean value represented by this instance.
    /// </summary>
    public bool Value { get; }

    /// <summary>
    /// Gets the type of this value, which is always Boolean.
    /// </summary>
    public override JyroValueType Type => JyroValueType.Boolean;

    /// <summary>
    /// Gets the singleton instance representing the boolean value true.
    /// </summary>
    public static JyroBoolean True { get; } = new JyroBoolean(true);

    /// <summary>
    /// Gets the singleton instance representing the boolean value false.
    /// </summary>
    public static JyroBoolean False { get; } = new JyroBoolean(false);

    /// <summary>
    /// Creates a JyroBoolean instance from a .NET boolean value.
    /// Returns one of the singleton instances for performance optimization.
    /// </summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <returns>The appropriate JyroBoolean singleton instance.</returns>
    public static JyroBoolean FromBoolean(bool value) => value ? True : False;

    /// <summary>
    /// Converts this value to a JyroBoolean instance.
    /// </summary>
    /// <returns>This instance.</returns>
    public override JyroBoolean AsBoolean() => this;

    /// <summary>
    /// Converts this boolean value to its .NET object representation.
    /// </summary>
    /// <returns>The boolean value as a .NET bool.</returns>
    public override object ToObjectValue() => Value;

    /// <summary>
    /// Converts this boolean value to a .NET boolean.
    /// </summary>
    /// <returns>The boolean value.</returns>
    public override bool ToBoolean() => Value;

    /// <summary>
    /// Converts this boolean value to its string representation.
    /// </summary>
    /// <returns>The string "True" or "False".</returns>
    public override string ToStringValue() => Value.ToString();

    /// <summary>
    /// Returns the hash code for this boolean value.
    /// </summary>
    /// <returns>The hash code of the underlying boolean value.</returns>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// Determines whether this boolean value is equal to another JyroValue.
    /// </summary>
    /// <param name="other">The JyroValue to compare with this instance.</param>
    /// <returns>True if the other value is a JyroBoolean with the same boolean value, otherwise false.</returns>
    public override bool Equals(JyroValue? other) => other is JyroBoolean booleanValue && booleanValue.Value == Value;
}