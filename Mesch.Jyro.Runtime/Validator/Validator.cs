using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Mesch.Jyro;

/// <summary>
/// Performs semantic validation over a Jyro AST with enhanced error handling and architectural alignment.
/// </summary>
public sealed class Validator : IValidator
{
    private readonly ILogger<Validator> _logger;
    private readonly HashSet<string> _builtins;
    private ValidationMetrics _metrics;

    /// <summary>
    /// Creates a new validator.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="builtins">
    /// A set of built-in identifiers that should always be considered valid,
    /// such as <c>Data</c> or names of standard library functions.
    /// </param>
    public Validator(ILogger<Validator> logger, IEnumerable<string>? builtins = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _builtins = new HashSet<string>(builtins ?? [], StringComparer.Ordinal);
    }

    /// <inheritdoc />
    public JyroValidationResult Validate(IReadOnlyList<IJyroStatement> ast)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var startedAt = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogTrace("Validate: start; statements={Count}", ast.Count);

        var messages = new List<IMessage>();
        var context = new ValidationContext();
        _metrics = new ValidationMetrics();

        try
        {
            ValidateStatements(ast, context, messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected validator error");
            messages.Add(new Message(
                MessageCode.UnknownValidatorError,
                0, 0,
                MessageSeverity.Error,
                ProcessingStage.Validation,
                ex.Message));
        }

        stopwatch.Stop();
        var isSuccessful = !messages.Any(m => m.Severity == MessageSeverity.Error);

        var metadata = new ValidationMetadata(stopwatch.Elapsed, startedAt);

        _logger.LogTrace("Validate: done; success={Success}, variables={VariableCount}, depth={Depth}, errors={ErrorCount}, elapsed={ElapsedMs}ms",
            isSuccessful, _metrics.VariableDeclarationCount, _metrics.MaxNestingDepth,
            messages.Count(m => m.Severity == MessageSeverity.Error),
            stopwatch.ElapsedMilliseconds);

        return new JyroValidationResult(isSuccessful, messages, stopwatch.Elapsed, metadata);
    }

    #region Core validation 

    private void ValidateStatements(IReadOnlyList<IJyroStatement> statements, ValidationContext context, List<IMessage> messages)
    {
        context.EnterScope();
        _metrics.MaxNestingDepth = Math.Max(_metrics.MaxNestingDepth, context.ScopeDepth);

        for (int i = 0; i < statements.Count; i++)
        {
            var statement = statements[i];
            ValidateStatement(statement, context, messages);

            // Check for unreachable code after control flow statements
            if (i < statements.Count - 1 && IsControlFlowStatement(statement) && context.LoopDepth > 0)
            {
                var next = statements[i + 1];
                messages.Add(new Message(
                    MessageCode.UnreachableCode,
                    next.LineNumber,
                    next.ColumnPosition,
                    MessageSeverity.Warning,
                    ProcessingStage.Validation,
                    "Unreachable code after break/continue"));
            }
        }

        context.ExitScope();
    }

    private void ValidateStatement(IJyroStatement statement, ValidationContext context, List<IMessage> messages)
    {
        _logger.LogTrace("Validate: statement {Type} at {Line}:{Col}",
            statement.GetType().Name, statement.LineNumber, statement.ColumnPosition);

        switch (statement)
        {
            case VariableDeclarationStatement v:
                ValidateVariableDeclaration(v, context, messages);
                break;

            case AssignmentStatement a:
                ValidateAssignment(a, context, messages);
                break;

            case ExpressionStatement e:
                ValidateExpression(e.Expression, context, messages);
                break;

            case IfStatement i:
                ValidateIfStatement(i, context, messages);
                break;

            case WhileStatement w:
                ValidateWhileStatement(w, context, messages);
                break;

            case ForEachStatement f:
                ValidateForEachStatement(f, context, messages);
                break;

            case SwitchStatement s:
                ValidateSwitchStatement(s, context, messages);
                break;

            case BreakStatement br:
                ValidateBreakStatement(br, context, messages);
                break;

            case ContinueStatement c:
                ValidateContinueStatement(c, context, messages);
                break;

            case ReturnStatement r:
                ValidateReturnStatement(r, context, messages);
                break;

            default:
                _logger.LogWarning("Unknown statement type: {Type}", statement.GetType().Name);
                break;
        }
    }

    private void ValidateVariableDeclaration(VariableDeclarationStatement declaration, ValidationContext context, List<IMessage> messages)
    {
        // Check for reserved names
        if (_builtins.Contains(declaration.Name))
        {
            messages.Add(new Message(
                MessageCode.InvalidVariableReference,
                declaration.LineNumber,
                declaration.ColumnPosition,
                MessageSeverity.Error,
                ProcessingStage.Validation,
                $"Cannot declare variable with reserved name: {declaration.Name}"));
            return;
        }

        // Check for redeclaration in current scope
        if (context.IsCurrentScopeVariable(declaration.Name))
        {
            messages.Add(new Message(
                MessageCode.InvalidVariableReference,
                declaration.LineNumber,
                declaration.ColumnPosition,
                MessageSeverity.Error,
                ProcessingStage.Validation,
                $"Variable '{declaration.Name}' is already declared in this scope"));
            return;
        }

        context.DeclareVariable(declaration.Name);
        _metrics.VariableDeclarationCount++;

        if (declaration.Initializer is not null)
        {
            ValidateExpression(declaration.Initializer, context, messages);
        }
    }

    private void ValidateAssignment(AssignmentStatement assignment, ValidationContext context, List<IMessage> messages)
    {
        if (assignment.Target is VariableExpression ve)
        {
            // Special check for Data reassignment
            if (ve.Name == "Data")
            {
                messages.Add(new Message(
                    MessageCode.InvalidAssignmentTarget,
                    ve.LineNumber,
                    ve.ColumnPosition,
                    MessageSeverity.Error,
                    ProcessingStage.Validation,
                    "Cannot reassign Data"));
                return;
            }

            EnsureVariableIsDeclared(ve.Name, ve.LineNumber, ve.ColumnPosition, context, messages);
        }
        else if (assignment.Target is PropertyAccessExpression or IndexAccessExpression)
        {
            ValidateExpression(assignment.Target, context, messages);
        }
        else
        {
            messages.Add(new Message(
                MessageCode.InvalidAssignmentTarget,
                assignment.LineNumber,
                assignment.ColumnPosition,
                MessageSeverity.Error,
                ProcessingStage.Validation,
                $"Invalid assignment target: {assignment.Target.GetType().Name}"));
        }

        ValidateExpression(assignment.Value, context, messages);
    }

    private void ValidateIfStatement(IfStatement ifStatement, ValidationContext context, List<IMessage> messages)
    {
        ValidateExpression(ifStatement.Condition, context, messages);

        context.EnterScope();
        ValidateStatements(ifStatement.ThenBranch, context, messages);
        context.ExitScope();

        if (ifStatement.ElseBranch.Count > 0)
        {
            context.EnterScope();
            ValidateStatements(ifStatement.ElseBranch, context, messages);
            context.ExitScope();
        }
    }

    private void ValidateWhileStatement(WhileStatement whileStatement, ValidationContext context, List<IMessage> messages)
    {
        ValidateExpression(whileStatement.Condition, context, messages);

        context.PushLoop();
        CheckExcessiveNesting(whileStatement, context, messages);

        context.EnterScope();
        ValidateStatements(whileStatement.Body, context, messages);
        context.ExitScope();

        context.PopLoop();
    }

    private void ValidateForEachStatement(ForEachStatement forEachStatement, ValidationContext context, List<IMessage> messages)
    {
        ValidateExpression(forEachStatement.Source, context, messages);

        context.PushLoop();
        CheckExcessiveNesting(forEachStatement, context, messages);

        context.EnterScope();
        context.PushIterator(forEachStatement.IteratorName);
        ValidateStatements(forEachStatement.Body, context, messages);
        context.PopIterator();
        context.ExitScope();

        context.PopLoop();
    }

    private void ValidateSwitchStatement(SwitchStatement switchStatement, ValidationContext context, List<IMessage> messages)
    {
        ValidateExpression(switchStatement.Expression, context, messages);

        context.PushSwitch();

        foreach (var caseClause in switchStatement.Cases)
        {
            ValidateExpression(caseClause.MatchExpression, context, messages);

            context.EnterScope();
            ValidateStatements(caseClause.Body, context, messages);
            context.ExitScope();
        }

        if (switchStatement.DefaultBranch.Count > 0)
        {
            context.EnterScope();
            ValidateStatements(switchStatement.DefaultBranch, context, messages);
            context.ExitScope();
        }

        context.PopSwitch();
    }

    private static void ValidateBreakStatement(BreakStatement breakStatement, ValidationContext context, List<IMessage> messages)
    {
        if (context.LoopDepth == 0 && context.SwitchDepth == 0)
        {
            messages.Add(new Message(
                MessageCode.LoopStatementOutsideOfLoop,
                breakStatement.LineNumber,
                breakStatement.ColumnPosition,
                MessageSeverity.Error,
                ProcessingStage.Validation,
                "break statement outside of loop or switch"));
        }
    }

    private static void ValidateContinueStatement(ContinueStatement continueStatement, ValidationContext context, List<IMessage> messages)
    {
        if (context.LoopDepth == 0)
        {
            messages.Add(new Message(
                MessageCode.LoopStatementOutsideOfLoop,
                continueStatement.LineNumber,
                continueStatement.ColumnPosition,
                MessageSeverity.Error,
                ProcessingStage.Validation,
                "continue statement outside of loop"));
        }
    }

    private void ValidateReturnStatement(ReturnStatement returnStatement, ValidationContext context, List<IMessage> messages)
    {
        if (returnStatement.Value is not null)
        {
            ValidateExpression(returnStatement.Value, context, messages);
        }
    }

    private void ValidateExpression(IExpression expression, ValidationContext context, List<IMessage> messages)
    {
        switch (expression)
        {
            case VariableExpression v:
                EnsureVariableIsDeclared(v.Name, v.LineNumber, v.ColumnPosition, context, messages);
                break;

            case PropertyAccessExpression p:
                ValidateExpression(p.Target, context, messages);
                break;

            case IndexAccessExpression i:
                ValidateExpression(i.Target, context, messages);
                ValidateExpression(i.Index, context, messages);
                break;

            case FunctionCallExpression call:
                // Function calls in Jyro are always Foo(args), never obj.foo()
                // So call.Target should always be a VariableExpression representing the function name
                if (call.Target is VariableExpression funcName)
                {
                    // Don't validate function names as variables - they're host functions
                    _logger.LogTrace("Function call: {FunctionName}", funcName.Name);
                }
                else
                {
                    // This shouldn't happen in valid Jyro code, but validate anyway
                    _logger.LogWarning("Unexpected function call target type: {Type}", call.Target.GetType().Name);
                    ValidateExpression(call.Target, context, messages);
                }

                // Validate all arguments
                foreach (var arg in call.Arguments)
                {
                    ValidateExpression(arg, context, messages);
                }
                break;

            case BinaryExpression b:
                ValidateExpression(b.Left, context, messages);
                ValidateExpression(b.Right, context, messages);
                break;

            case UnaryExpression u:
                ValidateExpression(u.Operand, context, messages);
                break;

            case TernaryExpression t:
                ValidateExpression(t.Condition, context, messages);
                ValidateExpression(t.TrueExpression, context, messages);
                ValidateExpression(t.FalseExpression, context, messages);
                break;

            case ArrayLiteralExpression arr:
                foreach (var el in arr.Elements)
                {
                    ValidateExpression(el, context, messages);
                }
                break;

            case ObjectLiteralExpression obj:
                foreach (var prop in obj.Properties)
                {
                    ValidateExpression(prop.Value, context, messages);
                }
                break;

            case TypeCheckExpression t:
                ValidateExpression(t.Target, context, messages);
                break;

            case LiteralExpression:
                // Literals require no validation
                break;

            case TypeExpression:
                // Type expressions are valid by construction
                break;

            default:
                _logger.LogWarning("Unknown expression type: {Type}", expression.GetType().Name);
                break;
        }
    }

    private void EnsureVariableIsDeclared(string name, int line, int column, ValidationContext context, List<IMessage> messages)
    {
        if (!context.IsDeclaredVariable(name) &&
            !context.IsIteratorVariable(name) &&
            !_builtins.Contains(name))
        {
            messages.Add(new Message(
                MessageCode.InvalidVariableReference,
                line, column,
                MessageSeverity.Error,
                ProcessingStage.Validation,
                $"Undeclared variable: {name}"));
        }
    }

    private static void CheckExcessiveNesting(IJyroStatement statement, ValidationContext context, List<IMessage> messages)
    {
        const int MaxRecommendedNesting = 3;

        if (context.LoopDepth > MaxRecommendedNesting)
        {
            messages.Add(new Message(
                MessageCode.ExcessiveLoopNesting,
                statement.LineNumber,
                statement.ColumnPosition,
                MessageSeverity.Warning,
                ProcessingStage.Validation,
                $"Loop nesting depth ({context.LoopDepth}) exceeds recommended maximum ({MaxRecommendedNesting})"));
        }
    }

    private static bool IsControlFlowStatement(IJyroStatement statement) =>
        statement is BreakStatement or ContinueStatement or ReturnStatement;

    #endregion

    #region Validation context

    private sealed class ValidationContext
    {
        private readonly Stack<HashSet<string>> _scopes = new();
        private readonly Stack<string> _iteratorVariables = new();

        public int LoopDepth { get; private set; }
        public int SwitchDepth { get; private set; }
        public int ScopeDepth => _scopes.Count;

        public void EnterScope() => _scopes.Push(new HashSet<string>(StringComparer.Ordinal));

        public void ExitScope()
        {
            if (_scopes.Count > 0)
            {
                _scopes.Pop();
            }
        }

        public void DeclareVariable(string name)
        {
            if (_scopes.Count == 0)
            {
                _scopes.Push(new HashSet<string>(StringComparer.Ordinal));
            }
            _scopes.Peek().Add(name);
        }

        public bool IsDeclaredVariable(string name) => _scopes.Any(s => s.Contains(name));

        public bool IsCurrentScopeVariable(string name) =>
            _scopes.Count > 0 && _scopes.Peek().Contains(name);

        public void PushIterator(string iteratorName) => _iteratorVariables.Push(iteratorName);

        public void PopIterator()
        {
            if (_iteratorVariables.Count > 0)
            {
                _iteratorVariables.Pop();
            }
        }

        public bool IsIteratorVariable(string name) => _iteratorVariables.Contains(name);

        public void PushLoop() => LoopDepth++;
        public void PopLoop() => LoopDepth--;
        public void PushSwitch() => SwitchDepth++;
        public void PopSwitch() => SwitchDepth--;
    }

    #endregion

    #region Validation metrics

    private struct ValidationMetrics
    {
        public int VariableDeclarationCount;
        public int MaxNestingDepth;
    }

    #endregion
}