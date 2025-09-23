namespace Mesch.Jyro;

/// <summary>
/// Represents a numeric value in the Jyro runtime system.
/// All numbers are stored as double-precision floating-point values to provide consistent arithmetic behavior.
/// </summary>
public sealed class JyroNumber : JyroValue
{
    /// <summary>
    /// Initializes a new instance of the JyroNumber class with the specified numeric value.
    /// </summary>
    /// <param name="value">The numeric value to represent.</param>
    public JyroNumber(double value) => Value = value;

    /// <summary>
    /// Gets the numeric value represented by this instance.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Gets the type of this value, which is always Number.
    /// </summary>
    public override JyroValueType Type => JyroValueType.Number;

    /// <summary>
    /// Gets a value indicating whether this number represents an integer value.
    /// </summary>
    public bool IsInteger => Math.Abs(Value % 1) < double.Epsilon;

    /// <summary>
    /// Converts this number to a 32-bit integer, truncating any fractional part.
    /// </summary>
    /// <returns>The integer representation of this number.</returns>
    public int ToInteger() => Convert.ToInt32(Value);

    /// <summary>
    /// Converts this value to a JyroNumber instance.
    /// </summary>
    /// <returns>This instance.</returns>
    public override JyroNumber AsNumber() => this;

    /// <summary>
    /// Evaluates a binary operation with this number as the left operand.
    /// Supports arithmetic operations with other numbers and string concatenation.
    /// </summary>
    /// <param name="operator">The binary operator to apply.</param>
    /// <param name="right">The right operand for the binary operation.</param>
    /// <returns>The result of the binary operation.</returns>
    /// <exception cref="DivideByZeroException">Thrown when attempting division or modulo by zero.</exception>
    public override JyroValue EvaluateBinary(JyroTokenType @operator, JyroValue right)
    {
        if (right is JyroNumber rightNumber)
        {
            return @operator switch
            {
                JyroTokenType.Plus => new JyroNumber(Value + rightNumber.Value),
                JyroTokenType.Minus => new JyroNumber(Value - rightNumber.Value),
                JyroTokenType.Star => new JyroNumber(Value * rightNumber.Value),
                JyroTokenType.Slash => rightNumber.Value != 0
                    ? new JyroNumber(Value / rightNumber.Value)
                    : throw new DivideByZeroException("Division by zero"),
                JyroTokenType.Percent => rightNumber.Value != 0
                    ? new JyroNumber(Value % rightNumber.Value)
                    : throw new DivideByZeroException("Modulo by zero"),
                JyroTokenType.Less => JyroBoolean.FromBoolean(Value < rightNumber.Value),
                JyroTokenType.LessEqual => JyroBoolean.FromBoolean(Value <= rightNumber.Value),
                JyroTokenType.Greater => JyroBoolean.FromBoolean(Value > rightNumber.Value),
                JyroTokenType.GreaterEqual => JyroBoolean.FromBoolean(Value >= rightNumber.Value),
                _ => base.EvaluateBinary(@operator, right)
            };
        }

        if (@operator == JyroTokenType.Plus && right is JyroString)
        {
            return new JyroString(Value.ToString() + right.AsStringOrEmpty());
        }

        return base.EvaluateBinary(@operator, right);
    }

    /// <summary>
    /// Evaluates a unary operation with this number as the operand.
    /// Supports arithmetic negation.
    /// </summary>
    /// <param name="operator">The unary operator to apply.</param>
    /// <returns>The result of the unary operation.</returns>
    public override JyroValue EvaluateUnary(JyroTokenType @operator)
    {
        return @operator switch
        {
            JyroTokenType.Minus => new JyroNumber(-Value),
            _ => base.EvaluateUnary(@operator)
        };
    }

    /// <summary>
    /// Converts this numeric value to its .NET object representation.
    /// </summary>
    /// <returns>The numeric value as a .NET double.</returns>
    public override object ToObjectValue() => Value;

    /// <summary>
    /// Converts this numeric value to a 32-bit signed integer.
    /// </summary>
    /// <returns>The integer representation of this value.</returns>
    public override int ToInt32() => Convert.ToInt32(Value);

    /// <summary>
    /// Converts this numeric value to a 64-bit signed integer.
    /// </summary>
    /// <returns>The long integer representation of this value.</returns>
    public override long ToInt64() => Convert.ToInt64(Value);

    /// <summary>
    /// Converts this numeric value to a double-precision floating-point number.
    /// </summary>
    /// <returns>The numeric value.</returns>
    public override double ToDouble() => Value;

    /// <summary>
    /// Converts this numeric value to a decimal number.
    /// </summary>
    /// <returns>The decimal representation of this value.</returns>
    public override decimal ToDecimal() => Convert.ToDecimal(Value);

    /// <summary>
    /// Converts this numeric value to a boolean using truthiness rules.
    /// Zero is considered false, all other values are considered true.
    /// </summary>
    /// <returns>False if the value is zero, otherwise true.</returns>
    public override bool ToBoolean() => Math.Abs(Value) > double.Epsilon;

    /// <summary>
    /// Converts this numeric value to its string representation.
    /// </summary>
    /// <returns>The string representation of the numeric value.</returns>
    public override string ToStringValue() => Value.ToString();

    /// <summary>
    /// Returns the hash code for this numeric value.
    /// </summary>
    /// <returns>The hash code of the underlying double value.</returns>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// Determines whether this numeric value is equal to another JyroValue.
    /// Uses epsilon comparison for floating-point equality.
    /// </summary>
    /// <param name="other">The JyroValue to compare with this instance.</param>
    /// <returns>True if the other value is a JyroNumber with an equivalent numeric value, otherwise false.</returns>
    public override bool Equals(JyroValue? other) =>
        other is JyroNumber numberValue && Math.Abs(numberValue.Value - Value) < double.Epsilon;
}