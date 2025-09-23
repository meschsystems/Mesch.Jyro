namespace Mesch.Jyro;

/// <summary>
/// Defines the visitor interface for traversing the Jyro abstract syntax tree without return values.
/// This interface implements the Visitor pattern, enabling various operations to be performed on AST nodes
/// such as validation, code analysis, or transformation without modifying the node structure itself.
/// </summary>
/// <remarks>
/// The Visitor pattern provides a clean separation between the AST structure and the operations
/// performed on it. This allows new operations to be added without modifying existing AST node classes.
/// Common implementations include semantic validators, code generators, interpreters, and analysis tools.
/// 
/// <para>
/// Each visit method corresponds to a specific AST node type and is called when that node
/// type is encountered during tree traversal. The visitor can maintain state and perform
/// side effects as needed for the specific operation being implemented.
/// </para>
/// </remarks>
public interface IVisitor
{
    #region Statements

    /// <summary>
    /// Visits a variable declaration statement node.
    /// </summary>
    /// <param name="statement">The variable declaration statement to process.</param>
    void VisitVariableDeclarationStatement(VariableDeclarationStatement statement);

    /// <summary>
    /// Visits an assignment statement node.
    /// </summary>
    /// <param name="statement">The assignment statement to process.</param>
    void VisitAssignmentStatement(AssignmentStatement statement);

    /// <summary>
    /// Visits an expression statement node.
    /// </summary>
    /// <param name="statement">The expression statement to process.</param>
    void VisitExpressionStatement(ExpressionStatement statement);

    /// <summary>
    /// Visits an if statement node.
    /// </summary>
    /// <param name="statement">The if statement to process.</param>
    void VisitIfStatement(IfStatement statement);

    /// <summary>
    /// Visits a switch statement node.
    /// </summary>
    /// <param name="statement">The switch statement to process.</param>
    void VisitSwitchStatement(SwitchStatement statement);

    /// <summary>
    /// Visits a while statement node.
    /// </summary>
    /// <param name="statement">The while statement to process.</param>
    void VisitWhileStatement(WhileStatement statement);

    /// <summary>
    /// Visits a foreach statement node.
    /// </summary>
    /// <param name="statement">The foreach statement to process.</param>
    void VisitForEachStatement(ForEachStatement statement);

    /// <summary>
    /// Visits a return statement node.
    /// </summary>
    /// <param name="statement">The return statement to process.</param>
    void VisitReturnStatement(ReturnStatement statement);

    /// <summary>
    /// Visits a break statement node.
    /// </summary>
    /// <param name="statement">The break statement to process.</param>
    void VisitBreakStatement(BreakStatement statement);

    /// <summary>
    /// Visits a continue statement node.
    /// </summary>
    /// <param name="statement">The continue statement to process.</param>
    void VisitContinueStatement(ContinueStatement statement);

    #endregion

    #region Expressions

    /// <summary>
    /// Visits a binary expression node.
    /// </summary>
    /// <param name="expression">The binary expression to process.</param>
    void VisitBinaryExpression(BinaryExpression expression);

    /// <summary>
    /// Visits a unary expression node.
    /// </summary>
    /// <param name="expression">The unary expression to process.</param>
    void VisitUnaryExpression(UnaryExpression expression);

    /// <summary>
    /// Visits a ternary expression node.
    /// </summary>
    /// <param name="expression">The ternary expression to process.</param>
    void VisitTernaryExpression(TernaryExpression expression);

    /// <summary>
    /// Visits a literal expression node.
    /// </summary>
    /// <param name="expression">The literal expression to process.</param>
    void VisitLiteralExpression(LiteralExpression expression);

    /// <summary>
    /// Visits a variable expression node.
    /// </summary>
    /// <param name="expression">The variable expression to process.</param>
    void VisitVariableExpression(VariableExpression expression);

    /// <summary>
    /// Visits a type expression node.
    /// </summary>
    /// <param name="expression">The type expression to process.</param>
    void VisitTypeExpression(TypeExpression expression);

    /// <summary>
    /// Visits a type check expression node.
    /// </summary>
    /// <param name="expression">The type check expression to process.</param>
    void VisitTypeCheckExpression(TypeCheckExpression expression);

    /// <summary>
    /// Visits a property access expression node.
    /// </summary>
    /// <param name="expression">The property access expression to process.</param>
    void VisitPropertyAccessExpression(PropertyAccessExpression expression);

    /// <summary>
    /// Visits an index access expression node.
    /// </summary>
    /// <param name="expression">The index access expression to process.</param>
    void VisitIndexAccessExpression(IndexAccessExpression expression);

    /// <summary>
    /// Visits a function call expression node.
    /// </summary>
    /// <param name="expression">The function call expression to process.</param>
    void VisitFunctionCallExpression(FunctionCallExpression expression);

    /// <summary>
    /// Visits an array literal expression node.
    /// </summary>
    /// <param name="expression">The array literal expression to process.</param>
    void VisitArrayLiteralExpression(ArrayLiteralExpression expression);

    /// <summary>
    /// Visits an object literal expression node.
    /// </summary>
    /// <param name="expression">The object literal expression to process.</param>
    void VisitObjectLiteralExpression(ObjectLiteralExpression expression);

    #endregion
}

/// <summary>
/// Defines the generic visitor interface for traversing the Jyro abstract syntax tree with return values.
/// This interface implements the Visitor pattern for operations that produce results, such as interpretation,
/// compilation, or value computation during AST traversal.
/// </summary>
/// <typeparam name="T">The type of value returned by visit operations.</typeparam>
/// <remarks>
/// This generic visitor interface enables operations that need to return computed values during
/// AST traversal. Common use cases include interpreters that evaluate expressions to produce values,
/// compilers that generate target code, or analysis tools that compute metrics or statistics.
/// 
/// <para>
/// The return type T can be any type appropriate for the operation being performed, such as
/// JyroValue for interpretation, string for code generation, or custom result types for analysis.
/// Each visit method returns a value of type T that represents the result of processing that node.
/// </para>
/// </remarks>
public interface IVisitor<out T>
{
    #region Statements

    /// <summary>
    /// Visits a variable declaration statement node and returns a result.
    /// </summary>
    /// <param name="statement">The variable declaration statement to process.</param>
    /// <returns>The result of processing the variable declaration statement.</returns>
    T VisitVariableDeclarationStatement(VariableDeclarationStatement statement);

    /// <summary>
    /// Visits an assignment statement node and returns a result.
    /// </summary>
    /// <param name="statement">The assignment statement to process.</param>
    /// <returns>The result of processing the assignment statement.</returns>
    T VisitAssignmentStatement(AssignmentStatement statement);

    /// <summary>
    /// Visits an expression statement node and returns a result.
    /// </summary>
    /// <param name="statement">The expression statement to process.</param>
    /// <returns>The result of processing the expression statement.</returns>
    T VisitExpressionStatement(ExpressionStatement statement);

    /// <summary>
    /// Visits an if statement node and returns a result.
    /// </summary>
    /// <param name="statement">The if statement to process.</param>
    /// <returns>The result of processing the if statement.</returns>
    T VisitIfStatement(IfStatement statement);

    /// <summary>
    /// Visits a switch statement node and returns a result.
    /// </summary>
    /// <param name="statement">The switch statement to process.</param>
    /// <returns>The result of processing the switch statement.</returns>
    T VisitSwitchStatement(SwitchStatement statement);

    /// <summary>
    /// Visits a while statement node and returns a result.
    /// </summary>
    /// <param name="statement">The while statement to process.</param>
    /// <returns>The result of processing the while statement.</returns>
    T VisitWhileStatement(WhileStatement statement);

    /// <summary>
    /// Visits a foreach statement node and returns a result.
    /// </summary>
    /// <param name="statement">The foreach statement to process.</param>
    /// <returns>The result of processing the foreach statement.</returns>
    T VisitForEachStatement(ForEachStatement statement);

    /// <summary>
    /// Visits a return statement node and returns a result.
    /// </summary>
    /// <param name="statement">The return statement to process.</param>
    /// <returns>The result of processing the return statement.</returns>
    T VisitReturnStatement(ReturnStatement statement);

    /// <summary>
    /// Visits a break statement node and returns a result.
    /// </summary>
    /// <param name="statement">The break statement to process.</param>
    /// <returns>The result of processing the break statement.</returns>
    T VisitBreakStatement(BreakStatement statement);

    /// <summary>
    /// Visits a continue statement node and returns a result.
    /// </summary>
    /// <param name="statement">The continue statement to process.</param>
    /// <returns>The result of processing the continue statement.</returns>
    T VisitContinueStatement(ContinueStatement statement);

    #endregion

    #region Expressions

    /// <summary>
    /// Visits a binary expression node and returns a result.
    /// </summary>
    /// <param name="expression">The binary expression to process.</param>
    /// <returns>The result of processing the binary expression.</returns>
    T VisitBinaryExpression(BinaryExpression expression);

    /// <summary>
    /// Visits a unary expression node and returns a result.
    /// </summary>
    /// <param name="expression">The unary expression to process.</param>
    /// <returns>The result of processing the unary expression.</returns>
    T VisitUnaryExpression(UnaryExpression expression);

    /// <summary>
    /// Visits a ternary expression node and returns a result.
    /// </summary>
    /// <param name="expression">The ternary expression to process.</param>
    /// <returns>The result of processing the ternary expression.</returns>
    T VisitTernaryExpression(TernaryExpression expression);

    /// <summary>
    /// Visits a literal expression node and returns a result.
    /// </summary>
    /// <param name="expression">The literal expression to process.</param>
    /// <returns>The result of processing the literal expression.</returns>
    T VisitLiteralExpression(LiteralExpression expression);

    /// <summary>
    /// Visits a variable expression node and returns a result.
    /// </summary>
    /// <param name="expression">The variable expression to process.</param>
    /// <returns>The result of processing the variable expression.</returns>
    T VisitVariableExpression(VariableExpression expression);

    /// <summary>
    /// Visits a type expression node and returns a result.
    /// </summary>
    /// <param name="expression">The type expression to process.</param>
    /// <returns>The result of processing the type expression.</returns>
    T VisitTypeExpression(TypeExpression expression);

    /// <summary>
    /// Visits a type check expression node and returns a result.
    /// </summary>
    /// <param name="expression">The type check expression to process.</param>
    /// <returns>The result of processing the type check expression.</returns>
    T VisitTypeCheckExpression(TypeCheckExpression expression);

    /// <summary>
    /// Visits a property access expression node and returns a result.
    /// </summary>
    /// <param name="expression">The property access expression to process.</param>
    /// <returns>The result of processing the property access expression.</returns>
    T VisitPropertyAccessExpression(PropertyAccessExpression expression);

    /// <summary>
    /// Visits an index access expression node and returns a result.
    /// </summary>
    /// <param name="expression">The index access expression to process.</param>
    /// <returns>The result of processing the index access expression.</returns>
    T VisitIndexAccessExpression(IndexAccessExpression expression);

    /// <summary>
    /// Visits a function call expression node and returns a result.
    /// </summary>
    /// <param name="expression">The function call expression to process.</param>
    /// <returns>The result of processing the function call expression.</returns>
    T VisitFunctionCallExpression(FunctionCallExpression expression);

    /// <summary>
    /// Visits an array literal expression node and returns a result.
    /// </summary>
    /// <param name="expression">The array literal expression to process.</param>
    /// <returns>The result of processing the array literal expression.</returns>
    T VisitArrayLiteralExpression(ArrayLiteralExpression expression);

    /// <summary>
    /// Visits an object literal expression node and returns a result.
    /// </summary>
    /// <param name="expression">The object literal expression to process.</param>
    /// <returns>The result of processing the object literal expression.</returns>
    T VisitObjectLiteralExpression(ObjectLiteralExpression expression);

    #endregion
}