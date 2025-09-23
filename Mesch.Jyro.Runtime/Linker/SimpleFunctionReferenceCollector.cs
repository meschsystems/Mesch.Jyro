namespace Mesch.Jyro;

/// <summary>
/// Collects function call references during the linking phase without complex type inference.
/// This collector performs a straightforward traversal of the abstract syntax tree to identify
/// function calls, enabling the linker to validate function existence and basic argument counts
/// without the overhead of detailed type analysis.
/// </summary>
/// <remarks>
/// This simplified approach eliminates the complexity of compile-time type inference while
/// maintaining the benefits of early function validation. The collector focuses on:
/// <list type="bullet">
/// <item><description>Identifying all function calls in the code</description></item>
/// <item><description>Recording function names and argument counts</description></item>
/// <item><description>Capturing source location information for error reporting</description></item>
/// </list>
/// Type validation is deferred to runtime where functions can handle their own type checking
/// and coercion as appropriate. This design provides better error messages with source locations
/// while keeping the linking process simple and maintainable.
/// </remarks>
public sealed class SimpleFunctionReferenceCollector : IVisitor<object?>
{
    private readonly HashSet<FunctionReference> _functionReferences;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleFunctionReferenceCollector"/> class
    /// with a collection to store discovered function references.
    /// </summary>
    /// <param name="functionReferences">
    /// The collection that will receive discovered function references during AST traversal.
    /// Cannot be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="functionReferences"/> is null.
    /// </exception>
    public SimpleFunctionReferenceCollector(HashSet<FunctionReference> functionReferences)
    {
        _functionReferences = functionReferences ?? throw new ArgumentNullException(nameof(functionReferences));
    }

    #region Statement Visitors

    public object? VisitVariableDeclarationStatement(VariableDeclarationStatement statement)
    {
        statement.TypeHint?.Accept(this);
        statement.Initializer?.Accept(this);
        return null;
    }

    public object? VisitAssignmentStatement(AssignmentStatement statement)
    {
        statement.Target.Accept(this);
        statement.Value.Accept(this);
        return null;
    }

    public object? VisitExpressionStatement(ExpressionStatement statement)
    {
        statement.Expression.Accept(this);
        return null;
    }

    public object? VisitIfStatement(IfStatement statement)
    {
        statement.Condition.Accept(this);

        foreach (var thenStatement in statement.ThenBranch)
        {
            thenStatement.Accept(this);
        }

        foreach (var elseIfClause in statement.ElseIfClauses)
        {
            elseIfClause.Condition.Accept(this);
            foreach (var elseIfStatement in elseIfClause.Statements)
            {
                elseIfStatement.Accept(this);
            }
        }

        foreach (var elseStatement in statement.ElseBranch)
        {
            elseStatement.Accept(this);
        }

        return null;
    }

    public object? VisitSwitchStatement(SwitchStatement statement)
    {
        statement.Expression.Accept(this);

        foreach (var caseClause in statement.Cases)
        {
            caseClause.MatchExpression.Accept(this);
            foreach (var caseStatement in caseClause.Body)
            {
                caseStatement.Accept(this);
            }
        }

        foreach (var defaultStatement in statement.DefaultBranch)
        {
            defaultStatement.Accept(this);
        }

        return null;
    }

    public object? VisitWhileStatement(WhileStatement statement)
    {
        statement.Condition.Accept(this);
        foreach (var loopStatement in statement.Body)
        {
            loopStatement.Accept(this);
        }
        return null;
    }

    public object? VisitForEachStatement(ForEachStatement statement)
    {
        statement.Source.Accept(this);
        foreach (var loopStatement in statement.Body)
        {
            loopStatement.Accept(this);
        }
        return null;
    }

    public object? VisitReturnStatement(ReturnStatement statement)
    {
        statement.Value?.Accept(this);
        return null;
    }

    public object? VisitBreakStatement(BreakStatement statement) => null;

    public object? VisitContinueStatement(ContinueStatement statement) => null;

    #endregion

    #region Expression Visitors

    public object? VisitBinaryExpression(BinaryExpression expression)
    {
        expression.Left.Accept(this);
        expression.Right.Accept(this);
        return null;
    }

    public object? VisitUnaryExpression(UnaryExpression expression)
    {
        expression.Operand.Accept(this);
        return null;
    }

    public object? VisitTernaryExpression(TernaryExpression expression)
    {
        expression.Condition.Accept(this);
        expression.TrueExpression.Accept(this);
        expression.FalseExpression.Accept(this);
        return null;
    }

    public object? VisitLiteralExpression(LiteralExpression expression) => null;

    public object? VisitVariableExpression(VariableExpression expression) => null;

    public object? VisitTypeExpression(TypeExpression expression) => null;

    public object? VisitTypeCheckExpression(TypeCheckExpression expression)
    {
        expression.Target.Accept(this);
        return null;
    }

    public object? VisitPropertyAccessExpression(PropertyAccessExpression expression)
    {
        expression.Target.Accept(this);
        return null;
    }

    public object? VisitIndexAccessExpression(IndexAccessExpression expression)
    {
        expression.Target.Accept(this);
        expression.Index.Accept(this);
        return null;
    }

    public object? VisitFunctionCallExpression(FunctionCallExpression expression)
    {
        var functionName = ResolveFunctionName(expression.Target);

        var placeholderArguments = expression.Arguments
            .Select(_ => (JyroValue)JyroNull.Instance)
            .ToList();

        _functionReferences.Add(new FunctionReference(
            functionName,
            placeholderArguments,
            expression.LineNumber,
            expression.ColumnPosition));

        expression.Target.Accept(this);
        foreach (var argument in expression.Arguments)
        {
            argument.Accept(this);
        }

        return null;
    }

    public object? VisitArrayLiteralExpression(ArrayLiteralExpression expression)
    {
        foreach (var element in expression.Elements)
        {
            element.Accept(this);
        }
        return null;
    }

    public object? VisitObjectLiteralExpression(ObjectLiteralExpression expression)
    {
        foreach (var property in expression.Properties)
        {
            property.Value.Accept(this);
        }
        return null;
    }

    #endregion

    #region Helper Methods

    private static string ResolveFunctionName(IExpression calleeExpression)
    {
        return calleeExpression switch
        {
            VariableExpression variableExpression => variableExpression.Name,
            PropertyAccessExpression propertyAccessExpression => ResolvePropertyChain(propertyAccessExpression),
            _ => "<dynamic>"
        };
    }

    private static string ResolvePropertyChain(PropertyAccessExpression propertyAccessExpression)
    {
        var propertyNameParts = new Stack<string>();
        IExpression currentExpression = propertyAccessExpression;

        while (currentExpression is PropertyAccessExpression nestedPropertyAccess)
        {
            propertyNameParts.Push(nestedPropertyAccess.PropertyName);
            currentExpression = nestedPropertyAccess.Target;
        }

        if (currentExpression is VariableExpression rootVariable)
        {
            propertyNameParts.Push(rootVariable.Name);
        }

        return string.Join(".", propertyNameParts);
    }

    #endregion
}