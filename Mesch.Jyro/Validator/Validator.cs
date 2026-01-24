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
                        "break/continue"));
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
                    MessageCode.ReservedIdentifier,
                    context.Start.Line,
                    context.Start.Column,
                    MessageSeverity.Error,
                    ProcessingStage.Validation,
                    name));
                return null;
            }

            // Check for redeclaration in current scope
            if (_context.IsCurrentScopeVariable(name))
            {
                _messages.Add(new Message(
                    MessageCode.VariableAlreadyDeclared,
                    context.Start.Line,
                    context.Start.Column,
                    MessageSeverity.Error,
                    ProcessingStage.Validation,
                    name));
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
            VisitStatementBlock(GetMainIfStatements(context));
            _context.ExitScope();

            // Validate else-if conditions and branches
            var elseIfCount = context.ELSEIF().Length;
            for (int i = 0; i < elseIfCount; i++)
            {
                Visit(context.expression(i + 1));
                _context.EnterScope();
                VisitStatementBlock(GetElseIfStatements(context, i));
                _context.ExitScope();
            }

            // Validate else branch if present
            if (context.ELSE() != null)
            {
                _context.EnterScope();
                VisitStatementBlock(GetElseStatements(context));
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
                VisitStatementBlock(GetCaseStatements(context, i));
                _context.ExitScope();
            }

            // Validate default branch if present
            if (context.DEFAULT() != null)
            {
                _context.EnterScope();
                VisitStatementBlock(GetDefaultStatements(context));
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
                    "break"));
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
                    "continue"));
            }
            return null;
        }

        public override object? VisitReturnStmt(JyroParser.ReturnStmtContext context)
        {
            if (context.expression() != null)
            {
                Visit(context.expression());
            }
            return null;
        }

        public override object? VisitFailStmt(JyroParser.FailStmtContext context)
        {
            if (context.expression() != null)
            {
                Visit(context.expression());
            }
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
                        "Data"));
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
                    MessageCode.UndeclaredVariable,
                    line, column,
                    MessageSeverity.Error,
                    ProcessingStage.Validation,
                    name));
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
                    MaxRecommendedNesting.ToString()));
            }
        }

        private static bool IsControlFlowStatement(JyroParser.StatementContext statement)
        {
            return statement.breakStmt() != null ||
                   statement.continueStmt() != null ||
                   statement.returnStmt() != null ||
                   statement.failStmt() != null;
        }

        // Helper methods for parsing if/switch statement structures
        // These methods walk the parse tree to partition statements into their respective branches

        private static IEnumerable<JyroParser.StatementContext> GetMainIfStatements(JyroParser.IfStmtContext context)
        {
            // Collect statements between the first "IF...THEN" and the first "ELSE" or "END"
            var children = context.children;
            bool pastThen = false;

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];

                // Once we hit THEN, start collecting statements
                if (child is ITerminalNode thenNode && thenNode.Symbol.Type == JyroParser.THEN && !pastThen)
                {
                    pastThen = true;
                    continue;
                }

                // If we're past THEN, collect statements until we hit ELSEIF, ELSE or END
                if (pastThen)
                {
                    // Stop at ELSEIF, ELSE or END
                    if (child is ITerminalNode term && (term.Symbol.Type == JyroParser.ELSEIF || term.Symbol.Type == JyroParser.ELSE || term.Symbol.Type == JyroParser.END))
                    {
                        yield break;
                    }

                    if (child is JyroParser.StatementContext stmtContext)
                    {
                        yield return stmtContext;
                    }
                }
            }
        }

        private static IEnumerable<JyroParser.StatementContext> GetElseIfStatements(JyroParser.IfStmtContext context, int elseIfIndex)
        {
            // Walk through children to find statements between the Nth "ELSEIF...THEN" and the next keyword
            var children = context.children;
            int elseIfCount = 0;
            bool inTargetBranch = false;

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];

                // Check if this is an ELSEIF token
                if (child is ITerminalNode terminal && terminal.Symbol.Type == JyroParser.ELSEIF)
                {
                    if (elseIfCount == elseIfIndex)
                    {
                        inTargetBranch = true;
                    }
                    else if (elseIfCount > elseIfIndex)
                    {
                        yield break; // Past our target branch
                    }
                    elseIfCount++;
                }

                // If we're in the target branch and hit a THEN, start collecting statements
                if (inTargetBranch && child is ITerminalNode thenNode && thenNode.Symbol.Type == JyroParser.THEN)
                {
                    inTargetBranch = false; // Now we're past THEN, collect statements
                    for (int j = i + 1; j < children.Count; j++)
                    {
                        var stmt = children[j];

                        // Stop at the next keyword (ELSEIF, ELSE, END)
                        if (stmt is ITerminalNode term && (term.Symbol.Type == JyroParser.ELSEIF || term.Symbol.Type == JyroParser.ELSE || term.Symbol.Type == JyroParser.END))
                        {
                            yield break;
                        }

                        if (stmt is JyroParser.StatementContext stmtContext)
                        {
                            yield return stmtContext;
                        }
                    }
                }
            }
        }

        private static IEnumerable<JyroParser.StatementContext> GetElseStatements(JyroParser.IfStmtContext context)
        {
            // Find the ELSE token and collect statements until END
            var children = context.children;
            bool inElseBranch = false;

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];

                // Check if this is an ELSE token (ELSEIF is a separate token now)
                if (child is ITerminalNode terminal && terminal.Symbol.Type == JyroParser.ELSE)
                {
                    inElseBranch = true;
                    continue;
                }

                if (inElseBranch)
                {
                    if (child is ITerminalNode term && term.Symbol.Type == JyroParser.END)
                    {
                        yield break;
                    }

                    if (child is JyroParser.StatementContext stmtContext)
                    {
                        yield return stmtContext;
                    }
                }
            }
        }

        private static IEnumerable<JyroParser.StatementContext> GetCaseStatements(JyroParser.SwitchStmtContext context, int caseIndex)
        {
            // Walk through children to find statements between the Nth CASE...THEN and the next keyword
            var children = context.children;
            int caseCount = 0;
            bool collectStatements = false;

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];

                // Found a CASE keyword
                if (child is ITerminalNode terminal && terminal.Symbol.Type == JyroParser.CASE)
                {
                    if (caseCount == caseIndex)
                    {
                        // This is our target case - find the THEN and start collecting after it
                        for (int j = i + 1; j < children.Count; j++)
                        {
                            if (children[j] is ITerminalNode thenNode && thenNode.Symbol.Type == JyroParser.THEN)
                            {
                                collectStatements = true;
                                i = j; // Move past THEN
                                break;
                            }
                        }
                    }
                    else if (caseCount > caseIndex)
                    {
                        // We've passed our target case
                        yield break;
                    }
                    caseCount++;
                }
                else if (collectStatements)
                {
                    // Stop at next CASE, DEFAULT, or END
                    if (child is ITerminalNode term &&
                        (term.Symbol.Type == JyroParser.CASE ||
                         term.Symbol.Type == JyroParser.DEFAULT ||
                         term.Symbol.Type == JyroParser.END))
                    {
                        yield break;
                    }

                    if (child is JyroParser.StatementContext stmtContext)
                    {
                        yield return stmtContext;
                    }
                }
            }
        }

        private static IEnumerable<JyroParser.StatementContext> GetDefaultStatements(JyroParser.SwitchStmtContext context)
        {
            // Find DEFAULT THEN and collect statements until END
            var children = context.children;
            bool collectStatements = false;

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];

                // Found DEFAULT keyword
                if (child is ITerminalNode terminal && terminal.Symbol.Type == JyroParser.DEFAULT)
                {
                    // Find the THEN and start collecting after it
                    for (int j = i + 1; j < children.Count; j++)
                    {
                        if (children[j] is ITerminalNode thenNode && thenNode.Symbol.Type == JyroParser.THEN)
                        {
                            collectStatements = true;
                            i = j; // Move past THEN
                            break;
                        }
                    }
                }
                else if (collectStatements)
                {
                    // Stop at END
                    if (child is ITerminalNode term && term.Symbol.Type == JyroParser.END)
                    {
                        yield break;
                    }

                    if (child is JyroParser.StatementContext stmtContext)
                    {
                        yield return stmtContext;
                    }
                }
            }
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
