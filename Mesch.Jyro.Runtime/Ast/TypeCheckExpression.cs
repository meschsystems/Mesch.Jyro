namespace Mesch.Jyro;

/// <summary>
/// Represents a type checking expression in the abstract syntax tree.
/// Type check expressions use the 'is' operator to test whether a value is of a specific type,
/// such as 'value is number' or 'data is array'. These expressions evaluate to boolean values
/// indicating whether the runtime type matches the specified type.
/// </summary>
/// <remarks>
/// Type check expressions provide runtime type testing capabilities that enable conditional
/// logic based on value types. This is particularly useful in dynamic scenarios where the
/// type of a value may not be known at compile time, such as when working with data from
/// external sources or function parameters.
/// 
/// <para>
/// The type checking uses the JyroValue type system, testing against the fundamental
/// Jyro types: number, string, boolean, array, object, and null.
/// </para>
/// </remarks>
public sealed class TypeCheckExpression : IExpression
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeCheckExpression"/> class with the specified
    /// target expression, type to check, and source location information.
    /// </summary>
    /// <param name="target">
    /// The expression whose runtime type will be checked.
    /// This expression is evaluated first to obtain the value for type testing.
    /// </param>
    /// <param name="checkedType">
    /// The type token that specifies which type to test against.
    /// This must be one of the Jyro type keywords (number, string, boolean, array, object).
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this type check appears.</param>
    /// <param name="columnPosition">The column position in the source code where this type check begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when the target parameter is null.</exception>
    public TypeCheckExpression(IExpression target, JyroTokenType checkedType, int lineNumber, int columnPosition)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
        CheckedType = checkedType;
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the target expression whose runtime type will be checked.
    /// This expression is evaluated to obtain the value that will be tested
    /// against the specified type.
    /// </summary>
    public IExpression Target { get; }

    /// <summary>
    /// Gets the type token that specifies which type to test against.
    /// This represents one of the fundamental Jyro types such as NumberType,
    /// StringType, BooleanType, ArrayType, or ObjectType.
    /// </summary>
    public JyroTokenType CheckedType { get; }

    /// <summary>
    /// Gets the line number in the source code where this type check expression appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this type check expression begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this type check expression.</param>
    public void Accept(IVisitor visitor) => visitor.VisitTypeCheckExpression(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this type check expression.</param>
    /// <returns>The result produced by the visitor's processing of this type check expression.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitTypeCheckExpression(this);
}