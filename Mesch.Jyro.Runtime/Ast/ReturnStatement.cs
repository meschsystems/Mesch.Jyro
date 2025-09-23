namespace Mesch.Jyro;

/// <summary>
/// Represents a return statement in the abstract syntax tree.
/// Return statements are used to exit from functions or script execution, optionally providing
/// a return value. When executed, a return statement immediately terminates the current
/// execution context and transfers control back to the caller.
/// </summary>
/// <remarks>
/// Return statements support two forms:
/// <list type="bullet">
/// <item><description>Simple return: 'return' (exits without a value)</description></item>
/// <item><description>Return with value: 'return expression' (exits and provides a value)</description></item>
/// </list>
/// In the context of script execution, return statements can be used to exit the script
/// early. The return value, if provided, may be used by the execution environment or
/// ignored depending on the execution context.
/// </remarks>
public sealed class ReturnStatement : IJyroStatement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReturnStatement"/> class with the specified
    /// optional return value expression and source location information.
    /// </summary>
    /// <param name="value">
    /// The optional expression that provides the return value. If null, the return statement
    /// exits without providing a specific value (equivalent to returning null).
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this return statement appears.</param>
    /// <param name="columnPosition">The column position in the source code where this return statement begins.</param>
    public ReturnStatement(IExpression? value, int lineNumber, int columnPosition)
    {
        Value = value;
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the optional expression that provides the return value.
    /// When this property is null, the return statement exits without providing a specific value.
    /// When present, this expression is evaluated to determine the value returned to the caller.
    /// </summary>
    public IExpression? Value { get; }

    /// <summary>
    /// Gets the line number in the source code where this return statement appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this return statement begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this return statement.</param>
    public void Accept(IVisitor visitor) => visitor.VisitReturnStatement(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this return statement.</param>
    /// <returns>The result produced by the visitor's processing of this return statement.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitReturnStatement(this);
}