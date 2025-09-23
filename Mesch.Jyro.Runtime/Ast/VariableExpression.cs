namespace Mesch.Jyro;

/// <summary>
/// Represents a variable reference expression in the abstract syntax tree.
/// Variable expressions refer to previously declared variables by name and resolve
/// to the current value stored in that variable within the appropriate scope.
/// </summary>
/// <remarks>
/// Variable expressions enable access to values stored in variables declared earlier
/// in the same scope or in enclosing scopes. The variable name is resolved at runtime
/// by searching through the scope chain, starting with the innermost scope and moving
/// outward until the variable is found or all scopes are exhausted.
/// 
/// <para>
/// If a variable name cannot be resolved in any accessible scope, this typically
/// results in a runtime error during execution. The scope resolution follows standard
/// lexical scoping rules where inner scopes can access variables from outer scopes
/// but not vice versa.
/// </para>
/// </remarks>
public sealed class VariableExpression : IExpression
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VariableExpression"/> class with the specified
    /// variable name and source location information.
    /// </summary>
    /// <param name="name">
    /// The name of the variable to reference. This name will be used to look up
    /// the variable's current value in the appropriate scope during execution.
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this variable reference appears.</param>
    /// <param name="columnPosition">The column position in the source code where this variable reference begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when the name parameter is null.</exception>
    public VariableExpression(string name, int lineNumber, int columnPosition)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the name of the variable being referenced.
    /// This name is used during execution to look up the variable's current value
    /// in the scope chain, starting from the innermost scope.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the line number in the source code where this variable expression appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this variable expression begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this variable expression.</param>
    public void Accept(IVisitor visitor) => visitor.VisitVariableExpression(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this variable expression.</param>
    /// <returns>The result produced by the visitor's processing of this variable expression.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitVariableExpression(this);
}