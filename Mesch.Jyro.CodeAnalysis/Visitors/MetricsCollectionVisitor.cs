namespace Mesch.Jyro.CodeAnalysis;

/// <summary>
/// Abstract Syntax Tree visitor that collects quantitative metrics during tree traversal.
/// This visitor implements the Visitor pattern to systematically gather statistical
/// information about code structure, complexity, and usage patterns.
/// </summary>
internal sealed class MetricsCollectionVisitor : IVisitor
{
    private readonly AnalysisContext _analysisContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsCollectionVisitor"/> class.
    /// </summary>
    /// <param name="analysisContext">The context to accumulate metrics data during traversal.</param>
    /// <exception cref="ArgumentNullException">Thrown when analysisContext is null.</exception>
    public MetricsCollectionVisitor(AnalysisContext analysisContext)
    {
        _analysisContext = analysisContext ?? throw new ArgumentNullException(nameof(analysisContext));
    }

    #region Statement Visitors

    /// <summary>
    /// Visits a variable declaration statement and records associated metrics.
    /// </summary>
    /// <param name="variableDeclarationStatement">The variable declaration statement to analyze.</param>
    public void VisitVariableDeclarationStatement(VariableDeclarationStatement variableDeclarationStatement)
    {
        _analysisContext.RecordStatement();
        _analysisContext.RecordVariableDeclaration(variableDeclarationStatement.Name);

        variableDeclarationStatement.TypeHint?.Accept(this);
        variableDeclarationStatement.Initializer?.Accept(this);
    }

    /// <summary>
    /// Visits an assignment statement and records associated metrics.
    /// </summary>
    /// <param name="assignmentStatement">The assignment statement to analyze.</param>
    public void VisitAssignmentStatement(AssignmentStatement assignmentStatement)
    {
        _analysisContext.RecordStatement();
        _analysisContext.RecordAssignment();

        if (assignmentStatement.Target is VariableExpression variableExpression)
        {
            _analysisContext.RecordVariableAssignment(variableExpression.Name);
        }

        assignmentStatement.Target.Accept(this);
        assignmentStatement.Value.Accept(this);
    }

    /// <summary>
    /// Visits an expression statement and records associated metrics.
    /// </summary>
    /// <param name="expressionStatement">The expression statement to analyze.</param>
    public void VisitExpressionStatement(ExpressionStatement expressionStatement)
    {
        _analysisContext.RecordStatement();
        expressionStatement.Expression.Accept(this);
    }

    /// <summary>
    /// Visits an if statement and records control flow and complexity metrics.
    /// </summary>
    /// <param name="ifStatement">The if statement to analyze.</param>
    public void VisitIfStatement(IfStatement ifStatement)
    {
        _analysisContext.RecordStatement();
        _analysisContext.RecordControlFlow();
        _analysisContext.EnterScope();
        _analysisContext.RecordControlFlowPattern("if-then", "Conditional branching logic");

        ifStatement.Condition.Accept(this);

        foreach (var thenStatement in ifStatement.ThenBranch)
        {
            thenStatement.Accept(this);
        }

        foreach (var elseIfClause in ifStatement.ElseIfClauses)
        {
            _analysisContext.RecordBranch();
            elseIfClause.Condition.Accept(this);

            foreach (var elseIfStatement in elseIfClause.Statements)
            {
                elseIfStatement.Accept(this);
            }
        }

        if (ifStatement.ElseBranch.Count > 0)
        {
            _analysisContext.RecordBranch();
            foreach (var elseStatement in ifStatement.ElseBranch)
            {
                elseStatement.Accept(this);
            }
        }

        _analysisContext.ExitScope();
    }

    /// <summary>
    /// Visits a switch statement and records control flow and complexity metrics.
    /// </summary>
    /// <param name="switchStatement">The switch statement to analyze.</param>
    public void VisitSwitchStatement(SwitchStatement switchStatement)
    {
        _analysisContext.RecordStatement();
        _analysisContext.RecordControlFlow();
        _analysisContext.EnterScope();
        _analysisContext.RecordControlFlowPattern("switch", "Multi-way conditional branching");

        switchStatement.Expression.Accept(this);

        foreach (var caseClause in switchStatement.Cases)
        {
            _analysisContext.RecordBranch();
            caseClause.MatchExpression.Accept(this);

            foreach (var caseStatement in caseClause.Body)
            {
                caseStatement.Accept(this);
            }
        }

        if (switchStatement.DefaultBranch.Count > 0)
        {
            _analysisContext.RecordBranch();
            foreach (var defaultStatement in switchStatement.DefaultBranch)
            {
                defaultStatement.Accept(this);
            }
        }

        _analysisContext.ExitScope();
    }

    /// <summary>
    /// Visits a while statement and records loop and complexity metrics.
    /// </summary>
    /// <param name="whileStatement">The while statement to analyze.</param>
    public void VisitWhileStatement(WhileStatement whileStatement)
    {
        _analysisContext.RecordStatement();
        _analysisContext.RecordControlFlow();
        _analysisContext.EnterScope();
        _analysisContext.RecordControlFlowPattern("while", "Conditional iteration loop");

        whileStatement.Condition.Accept(this);

        foreach (var bodyStatement in whileStatement.Body)
        {
            bodyStatement.Accept(this);
        }

        _analysisContext.ExitScope();
    }

    /// <summary>
    /// Visits a foreach statement and records iteration and complexity metrics.
    /// </summary>
    /// <param name="forEachStatement">The foreach statement to analyze.</param>
    public void VisitForEachStatement(ForEachStatement forEachStatement)
    {
        _analysisContext.RecordStatement();
        _analysisContext.RecordControlFlow();
        _analysisContext.EnterScope();
        _analysisContext.RecordControlFlowPattern("foreach", "Collection iteration loop");

        _analysisContext.RecordVariableDeclaration(forEachStatement.IteratorName);
        forEachStatement.Source.Accept(this);

        foreach (var bodyStatement in forEachStatement.Body)
        {
            bodyStatement.Accept(this);
        }

        _analysisContext.ExitScope();
    }

    /// <summary>
    /// Visits a return statement and records associated metrics.
    /// </summary>
    /// <param name="returnStatement">The return statement to analyze.</param>
    public void VisitReturnStatement(ReturnStatement returnStatement)
    {
        _analysisContext.RecordStatement();
        returnStatement.Value?.Accept(this);
    }

    /// <summary>
    /// Visits a break statement and records associated metrics.
    /// </summary>
    /// <param name="breakStatement">The break statement to analyze.</param>
    public void VisitBreakStatement(BreakStatement breakStatement)
    {
        _analysisContext.RecordStatement();
    }

    /// <summary>
    /// Visits a continue statement and records associated metrics.
    /// </summary>
    /// <param name="continueStatement">The continue statement to analyze.</param>
    public void VisitContinueStatement(ContinueStatement continueStatement)
    {
        _analysisContext.RecordStatement();
    }

    #endregion

    #region Expression Visitors

    /// <summary>
    /// Visits a binary expression and records operator usage metrics.
    /// </summary>
    /// <param name="binaryExpression">The binary expression to analyze.</param>
    public void VisitBinaryExpression(BinaryExpression binaryExpression)
    {
        _analysisContext.RecordExpression();
        _analysisContext.RecordOperatorUsage(binaryExpression.Operator);

        binaryExpression.Left.Accept(this);
        binaryExpression.Right.Accept(this);
    }

    /// <summary>
    /// Visits a unary expression and records operator usage metrics.
    /// </summary>
    /// <param name="unaryExpression">The unary expression to analyze.</param>
    public void VisitUnaryExpression(UnaryExpression unaryExpression)
    {
        _analysisContext.RecordExpression();
        _analysisContext.RecordOperatorUsage(unaryExpression.Operator);

        unaryExpression.Operand.Accept(this);
    }

    /// <summary>
    /// Visits a ternary expression and records operator usage metrics.
    /// </summary>
    /// <param name="ternaryExpression">The unary expression to analyze.</param>
    public void VisitTernaryExpression(TernaryExpression ternaryExpression)
    {
        ternaryExpression.Condition.Accept(this);
        ternaryExpression.TrueExpression.Accept(this);
        ternaryExpression.FalseExpression.Accept(this);
    }

    /// <summary>
    /// Visits a literal expression and records associated metrics.
    /// </summary>
    /// <param name="literalExpression">The literal expression to analyze.</param>
    public void VisitLiteralExpression(LiteralExpression literalExpression)
    {
        _analysisContext.RecordExpression();
    }

    /// <summary>
    /// Visits a variable expression and records variable usage metrics.
    /// </summary>
    /// <param name="variableExpression">The variable expression to analyze.</param>
    public void VisitVariableExpression(VariableExpression variableExpression)
    {
        _analysisContext.RecordExpression();
        _analysisContext.RecordVariableUsage(variableExpression.Name);
    }

    /// <summary>
    /// Visits a type expression and records associated metrics.
    /// </summary>
    /// <param name="typeExpression">The type expression to analyze.</param>
    public void VisitTypeExpression(TypeExpression typeExpression)
    {
        _analysisContext.RecordExpression();
    }

    /// <summary>
    /// Visits a type check expression and records associated metrics.
    /// </summary>
    /// <param name="typeCheckExpression">The type check expression to analyze.</param>
    public void VisitTypeCheckExpression(TypeCheckExpression typeCheckExpression)
    {
        _analysisContext.RecordExpression();
        typeCheckExpression.Target.Accept(this);
    }

    /// <summary>
    /// Visits a property access expression and records associated metrics.
    /// </summary>
    /// <param name="propertyAccessExpression">The property access expression to analyze.</param>
    public void VisitPropertyAccessExpression(PropertyAccessExpression propertyAccessExpression)
    {
        _analysisContext.RecordExpression();
        propertyAccessExpression.Target.Accept(this);
    }

    /// <summary>
    /// Visits an index access expression and records associated metrics.
    /// </summary>
    /// <param name="indexAccessExpression">The index access expression to analyze.</param>
    public void VisitIndexAccessExpression(IndexAccessExpression indexAccessExpression)
    {
        _analysisContext.RecordExpression();
        indexAccessExpression.Target.Accept(this);
        indexAccessExpression.Index.Accept(this);
    }

    /// <summary>
    /// Visits a function call expression and records function call metrics.
    /// </summary>
    /// <param name="functionCallExpression">The function call expression to analyze.</param>
    public void VisitFunctionCallExpression(FunctionCallExpression functionCallExpression)
    {
        _analysisContext.RecordExpression();

        var functionName = ExtractFunctionName(functionCallExpression.Target);
        _analysisContext.RecordFunctionCall(functionName);

        functionCallExpression.Target.Accept(this);

        foreach (var argumentExpression in functionCallExpression.Arguments)
        {
            argumentExpression.Accept(this);
        }
    }

    /// <summary>
    /// Visits an array literal expression and records associated metrics.
    /// </summary>
    /// <param name="arrayLiteralExpression">The array literal expression to analyze.</param>
    public void VisitArrayLiteralExpression(ArrayLiteralExpression arrayLiteralExpression)
    {
        _analysisContext.RecordExpression();

        foreach (var elementExpression in arrayLiteralExpression.Elements)
        {
            elementExpression.Accept(this);
        }
    }

    /// <summary>
    /// Visits an object literal expression and records associated metrics.
    /// </summary>
    /// <param name="objectLiteralExpression">The object literal expression to analyze.</param>
    public void VisitObjectLiteralExpression(ObjectLiteralExpression objectLiteralExpression)
    {
        _analysisContext.RecordExpression();

        foreach (var objectProperty in objectLiteralExpression.Properties)
        {
            objectProperty.Value.Accept(this);
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Extracts a readable function name from a function call target expression.
    /// </summary>
    /// <param name="targetExpression">The target expression of the function call.</param>
    /// <returns>A string representation of the function name for metrics tracking.</returns>
    private static string ExtractFunctionName(IExpression targetExpression)
    {
        return targetExpression switch
        {
            VariableExpression variableExpression => variableExpression.Name,
            PropertyAccessExpression propertyAccessExpression => propertyAccessExpression.PropertyName,
            _ => "<complex_expression>"
        };
    }

    #endregion
}