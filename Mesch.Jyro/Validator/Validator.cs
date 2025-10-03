using System.Diagnostics;
using Antlr4.CodeGenerator;
using Antlr4.Runtime.Tree;
using Microsoft.Extensions.Logging;

namespace Mesch.Jyro;

/// <summary>
/// Performs semantic validation over parse trees with enhanced error handling.
/// Validates variable scoping, control flow usage, reserved names, and code quality.
/// </summary>
public sealed class Validator
{
    private readonly ILogger<Validator> _logger;
    private readonly HashSet<string> _builtins;

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
        _builtins = new HashSet<string>(builtins ?? new[] { "Data" }, StringComparer.Ordinal);
    }

    /// <summary>
    /// Validates the specified parse tree for semantic correctness.
    /// </summary>
    /// <param name="programContext">The program context produced by the parser.</param>
    /// <returns>A <see cref="JyroValidationResult"/> describing the outcome.</returns>
    public JyroValidationResult Validate(JyroParser.ProgramContext programContext)
    {
        ArgumentNullException.ThrowIfNull(programContext);

        var startedAt = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogTrace("Validate: start; statements={Count}", programContext.statement().Length);

        var messages = new List<IMessage>();
        var visitor = new ValidationVisitor(_logger, _builtins, messages);

        try
        {
            visitor.Visit(programContext);
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
            isSuccessful, visitor.Metrics.VariableDeclarationCount, visitor.Metrics.MaxNestingDepth,
            messages.Count(m => m.Severity == MessageSeverity.Error),
            stopwatch.ElapsedMilliseconds);

        return new JyroValidationResult(isSuccessful, messages, stopwatch.Elapsed, metadata);
    }

    /// <summary>
    /// Internal visitor that performs semantic validation checks on the parse tree.
    /// </summary>
    private sealed class ValidationVisitor : JyroBaseVisitor<object?>
    {
        private readonly ILogger _logger;
        private readonly HashSet<string> _builtins;
        private readonly List<IMessage> _messages;
        private readonly ValidationContext _context = new();

        public ValidationMetrics Metrics { get; } = new();

        public ValidationVisitor(ILogger logger, HashSet<string> builtins, List<IMessage> messages)
        {
            _logger = logger;
            _builtins = builtins;
            _messages = messages;
        }

        // ===== Statements =====

        public override object? VisitProgram(JyroParser.ProgramContext context)
        {
            _context.EnterScope();
            Metrics.MaxNestingDepth = Math.Max(Metrics.MaxNestingDepth, _context.ScopeDepth);

            var statements = context.statement();
            for (int i = 0; i < statements.Length; i++)
            {
                Visit(statements[i]);

                // Check for unreachable code after control flow statements
                if (i < statements.Length - 1 && IsControlFlowStatement(statements[i]) && _context.LoopDepth > 0)
                {
                    var next = statements[i + 1];
                    _messages.Add(new Message(
                        MessageCode.UnreachableCode,
                        next.Start.Line,
                        next.Start.Column,
                        MessageSeverity.Warning,
                        ProcessingStage.Validation,
                        "Unreachable code after break/continue"));
                }
            }

            _context.ExitScope();
            return null;
        }

        public override object? VisitVariableDecl(JyroParser.VariableDeclContext context)
        {
            var name = context.Identifier().GetText();

            // Check for reserved names
            if (_builtins.Contains(name))
            {
                _messages.Add(new Message(
                    MessageCode.InvalidVariableReference,
                    context.Start.Line,
                    context.Start.Column,
                    MessageSeverity.Error,
                    ProcessingStage.Validation,
                    $"Cannot declare variable with reserved name: {name}"));
                return null;
            }

            // Check for redeclaration in current scope
            if (_context.IsCurrentScopeVariable(name))
            {
                _messages.Add(new Message(
                    MessageCode.InvalidVariableReference,
                    context.Start.Line,
                    context.Start.Column,
                    MessageSeverity.Error,
                    ProcessingStage.Validation,
                    $"Variable '{name}' is already declared in this scope"));
                return null;
            }

            _context.DeclareVariable(name);
            Metrics.VariableDeclarationCount++;

            // Validate initializer if present
            if (context.expression() != null)
            {
                Visit(context.expression());
            }

            return null;
        }

        public override object? VisitIfStmt(JyroParser.IfStmtContext context)
        {
            // Validate main condition
            Visit(context.expression(0));

            // Validate main then branch
            _context.EnterScope();
            VisitStatementBlock(context.statement().Take(GetMainThenCount(context)));
            _context.ExitScope();

            // Validate else-if conditions and branches
            var elseIfCount = context.IF().Length - 1;
            for (int i = 0; i < elseIfCount; i++)
            {
                Visit(context.expression(i + 1));
                _context.EnterScope();
                // TODO: Need to properly extract else-if statement blocks
                _context.ExitScope();
            }

            // Validate else branch if present
            if (context.ELSE().Length > elseIfCount)
            {
                _context.EnterScope();
                // TODO: Need to properly extract else statement blocks
                _context.ExitScope();
            }

            return null;
        }

        public override object? VisitSwitchStmt(JyroParser.SwitchStmtContext context)
        {
            // Validate switch expression
            Visit(context.expression(0));

            _context.PushSwitch();

            // Validate case expressions and bodies
            var caseCount = context.CASE().Length;
            for (int i = 0; i < caseCount; i++)
            {
                Visit(context.expression(i + 1));
                _context.EnterScope();
                // TODO: Need to properly extract case statement blocks
                _context.ExitScope();
            }

            // Validate default branch if present
            if (context.DEFAULT() != null)
            {
                _context.EnterScope();
                // TODO: Need to properly extract default statement blocks
                _context.ExitScope();
            }

            _context.PopSwitch();
            return null;
        }

        public override object? VisitWhileStmt(JyroParser.WhileStmtContext context)
        {
            Visit(context.expression());

            _context.PushLoop();
            CheckExcessiveNesting(context);

            _context.EnterScope();
            VisitStatementBlock(context.statement());
            _context.ExitScope();

            _context.PopLoop();
            return null;
        }

        public override object? VisitForEachStmt(JyroParser.ForEachStmtContext context)
        {
            var iteratorName = context.Identifier().GetText();
            Visit(context.expression());

            _context.PushLoop();
            CheckExcessiveNesting(context);

            _context.EnterScope();
            _context.PushIterator(iteratorName);
            VisitStatementBlock(context.statement());
            _context.PopIterator();
            _context.ExitScope();

            _context.PopLoop();
            return null;
        }

        public override object? VisitBreakStmt(JyroParser.BreakStmtContext context)
        {
            if (_context.LoopDepth == 0 && _context.SwitchDepth == 0)
            {
                _messages.Add(new Message(
                    MessageCode.LoopStatementOutsideOfLoop,
                    context.Start.Line,
                    context.Start.Column,
                    MessageSeverity.Error,
                    ProcessingStage.Validation,
                    "break statement outside of loop or switch"));
            }
            return null;
        }

        public override object? VisitContinueStmt(JyroParser.ContinueStmtContext context)
        {
            if (_context.LoopDepth == 0)
            {
                _messages.Add(new Message(
                    MessageCode.LoopStatementOutsideOfLoop,
                    context.Start.Line,
                    context.Start.Column,
                    MessageSeverity.Error,
                    ProcessingStage.Validation,
                    "continue statement outside of loop"));
            }
            return null;
        }

        public override object? VisitReturnStmt(JyroParser.ReturnStmtContext context)
        {
            // Return statements are always valid (they may have no value in Jyro)
            return null;
        }

        // ===== Expressions =====

        public override object? VisitAssignMake(JyroParser.AssignMakeContext context)
        {
            var target = context.assignmentTarget();

            // Check if trying to reassign Data
            if (target.Identifier() != null)
            {
                var name = target.Identifier().GetText();
                if (name == "Data" && target.memberOrIndex().Length == 0)
                {
                    _messages.Add(new Message(
                        MessageCode.InvalidAssignmentTarget,
                        target.Start.Line,
                        target.Start.Column,
                        MessageSeverity.Error,
                        ProcessingStage.Validation,
                        "Cannot reassign Data"));
                    return null;
                }

                // Ensure variable is declared
                EnsureVariableIsDeclared(name, target.Start.Line, target.Start.Column);
            }

            // Validate the value being assigned
            Visit(context.assignmentExpr());
            return null;
        }

        public override object? VisitPrimaryExpr(JyroParser.PrimaryExprContext context)
        {
            // Check variable references, but skip if this is part of a function call
            // (parent will be a postfixExpr with a function call suffix)
            if (context.Identifier() != null)
            {
                var name = context.Identifier().GetText();

                // Check if parent is a postfixExpr with function call syntax
                if (context.Parent is JyroParser.PostfixExprContext postfix)
                {
                    var suffixes = postfix.postfixSuffix();
                    if (suffixes.Length > 0 && suffixes[0].LPAREN() != null)
                    {
                        // This is a function call - skip variable validation
                        _logger.LogTrace("Function call detected: {FunctionName}", name);
                        return base.VisitPrimaryExpr(context);
                    }
                }

                // Not a function call, validate as variable
                EnsureVariableIsDeclared(name, context.Start.Line, context.Start.Column);
            }

            return base.VisitPrimaryExpr(context);
        }

        public override object? VisitPostfixExpr(JyroParser.PostfixExprContext context)
        {
            // Visit the postfix expression and its arguments
            // Function call validation happens in VisitPrimaryExpr by checking parent context
            return base.VisitPostfixExpr(context);
        }

        public override object? VisitIncDecStmt(JyroParser.IncDecStmtContext context)
        {
            var target = context.assignmentTarget();

            // Validate the target exists
            if (target.Identifier() != null)
            {
                var name = target.Identifier().GetText();
                EnsureVariableIsDeclared(name, target.Start.Line, target.Start.Column);
            }

            return null;
        }

        // ===== Helpers =====

        private void VisitStatementBlock(IEnumerable<JyroParser.StatementContext> statements)
        {
            foreach (var stmt in statements)
            {
                Visit(stmt);
            }
        }

        private void EnsureVariableIsDeclared(string name, int line, int column)
        {
            if (!_context.IsDeclaredVariable(name) &&
                !_context.IsIteratorVariable(name) &&
                !_builtins.Contains(name))
            {
                _messages.Add(new Message(
                    MessageCode.InvalidVariableReference,
                    line, column,
                    MessageSeverity.Error,
                    ProcessingStage.Validation,
                    $"Undeclared variable: {name}"));
            }
        }

        private void CheckExcessiveNesting(IParseTree context)
        {
            const int MaxRecommendedNesting = 3;

            if (_context.LoopDepth > MaxRecommendedNesting)
            {
                var token = context is Antlr4.Runtime.ParserRuleContext prc ? prc.Start : null;
                _messages.Add(new Message(
                    MessageCode.ExcessiveLoopNesting,
                    token?.Line ?? 0,
                    token?.Column ?? 0,
                    MessageSeverity.Warning,
                    ProcessingStage.Validation,
                    $"Loop nesting depth ({_context.LoopDepth}) exceeds recommended maximum ({MaxRecommendedNesting})"));
            }
        }

        private static bool IsControlFlowStatement(JyroParser.StatementContext statement)
        {
            return statement.breakStmt() != null ||
                   statement.continueStmt() != null ||
                   statement.returnStmt() != null;
        }

        private static int GetMainThenCount(JyroParser.IfStmtContext context)
        {
            // This is a simplified approximation
            // TODO: Properly parse if statement structure to extract exact statement counts
            return context.statement().Length;
        }
    }

    /// <summary>
    /// Tracks validation context including scopes, loops, and iterators.
    /// </summary>
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

    /// <summary>
    /// Metrics collected during validation.
    /// </summary>
    public sealed class ValidationMetrics
    {
        public int VariableDeclarationCount { get; set; }
        public int MaxNestingDepth { get; set; }
    }
}
