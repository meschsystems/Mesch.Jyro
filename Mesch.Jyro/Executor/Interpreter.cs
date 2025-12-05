using System.Diagnostics;
using Antlr4.CodeGenerator;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace Mesch.Jyro;

/// <summary>
/// The interpreter that executes Jyro programs by traversing parse trees.
/// This class combines parsing, visiting, and execution into one native flow.
/// </summary>
public class Interpreter : JyroBaseVisitor<JyroValue>
{
    private JyroExecutionContext _context = null!;
    private ExecutionMetrics _metrics;

    /// <summary>
    /// Creates a JyroRuntimeException with proper location information from a parse context.
    /// </summary>
    private static JyroRuntimeException CreateException(MessageCode code, ParserRuleContext? context, string message)
    {
        var line = context?.Start?.Line ?? 0;
        var column = (context?.Start?.Column ?? -1) + 1; // Convert 0-based ANTLR column to 1-based display
        return new JyroRuntimeException(code, line, column, message);
    }

    /// <summary>
    /// Executes the linked program with the provided execution context.
    /// </summary>
    public JyroExecutionResult Execute(LinkedProgram linkedProgram, JyroExecutionContext executionContext)
    {
        ArgumentNullException.ThrowIfNull(linkedProgram);
        ArgumentNullException.ThrowIfNull(executionContext);

        var executionStartedAt = DateTimeOffset.UtcNow;
        var executionStopwatch = Stopwatch.StartNew();

        _context = executionContext;
        _metrics = new ExecutionMetrics();

        try
        {
            Visit(linkedProgram.ProgramContext);
        }
        catch (ReturnControlFlowException returnException)
        {
            if (returnException.ReturnValue != null)
            {
                _context.Variables.Declare("Return", returnException.ReturnValue);
            }
        }
        catch (OperationCanceledException)
        {
            _context.Messages.Add(new Message(
                MessageCode.CancelledByHost,
                0, 0,
                MessageSeverity.Error,
                ProcessingStage.Execution,
                "Execution was cancelled"));
        }
        catch (JyroRuntimeException ex)
        {
            _context.Messages.Add(new Message(
                ex.Code,
                ex.LineNumber,
                ex.ColumnPosition,
                MessageSeverity.Error,
                ProcessingStage.Execution,
                ex.Message));
        }
        catch (Exception unexpectedException)
        {
            _context.Messages.Add(new Message(
                MessageCode.UnknownExecutorError,
                0, 0,
                MessageSeverity.Error,
                ProcessingStage.Execution,
                unexpectedException.Message));
        }

        executionStopwatch.Stop();
        var executionSucceeded = !_context.Messages.Any(m => m.Severity == MessageSeverity.Error);

        var executionMetadata = new ExecutionMetadata(
            executionStopwatch.Elapsed,
            _metrics.StatementCount,
            _metrics.LoopCount,
            _metrics.FunctionCallCount,
            _metrics.MaxCallDepth,
            executionStartedAt);

        if (!_context.Variables.TryGet(JyroExecutionContext.RootIdentifier, out var finalRootData) || finalRootData is null)
        {
            finalRootData = JyroNull.Instance;
        }

        return new JyroExecutionResult(executionSucceeded, finalRootData, _context.Messages, executionMetadata);
    }

    // ===== Program & Statements =====

    public override JyroValue VisitProgram(JyroParser.ProgramContext context)
    {
        foreach (var stmt in context.statement())
        {
            Visit(stmt);
        }
        return JyroNull.Instance;
    }

    public override JyroValue VisitVariableDecl(JyroParser.VariableDeclContext context)
    {
        _context.Limiter.CheckAndCountStatement();
        _metrics.StatementCount++;

        var name = context.Identifier().GetText();
        var value = context.expression() != null
            ? Visit(context.expression())
            : JyroNull.Instance;
        _context.Variables.Declare(name, value);
        return value;
    }

    public override JyroValue VisitExprStmt(JyroParser.ExprStmtContext context)
    {
        _context.Limiter.CheckAndCountStatement();
        _metrics.StatementCount++;

        return Visit(context.expression());
    }

    public override JyroValue VisitIncDecStmt(JyroParser.IncDecStmtContext context)
    {
        _context.Limiter.CheckAndCountStatement();
        _metrics.StatementCount++;

        var target = context.assignmentTarget();
        var currentValue = EvaluateAssignmentTarget(target);

        if (currentValue is not JyroNumber num)
        {
            throw CreateException(MessageCode.IncrementDecrementNonNumber, context,
                $"Cannot increment/decrement non-number value of type '{currentValue.Type}'");
        }

        var newValue = context.INCR() != null
            ? new JyroNumber(num.Value + 1)
            : new JyroNumber(num.Value - 1);

        AssignToTarget(target, newValue);
        return newValue;
    }

    // ===== Control Flow =====

    public override JyroValue VisitIfStmt(JyroParser.IfStmtContext context)
    {
        _context.Limiter.CheckAndCountStatement();
        _metrics.StatementCount++;

        // Main if
        var mainCondition = Visit(context.expression(0));
        if (IsTruthy(mainCondition))
        {
            _context.Variables.PushScope();
            try
            {
                foreach (var stmt in GetMainIfStatements(context))
                {
                    Visit(stmt);
                }
            }
            finally
            {
                _context.Variables.PopScope();
            }
            return JyroNull.Instance;
        }

        // Else if clauses
        var elseIfCount = context.IF().Length - 1;
        for (int i = 0; i < elseIfCount; i++)
        {
            var condition = Visit(context.expression(i + 1));
            if (IsTruthy(condition))
            {
                _context.Variables.PushScope();
                try
                {
                    foreach (var stmt in GetElseIfStatements(context, i))
                    {
                        Visit(stmt);
                    }
                }
                finally
                {
                    _context.Variables.PopScope();
                }
                return JyroNull.Instance;
            }
        }

        // Else clause
        if (context.ELSE().Length > elseIfCount)
        {
            _context.Variables.PushScope();
            try
            {
                foreach (var stmt in GetElseStatements(context))
                {
                    Visit(stmt);
                }
            }
            finally
            {
                _context.Variables.PopScope();
            }
        }

        return JyroNull.Instance;
    }

    public override JyroValue VisitSwitchStmt(JyroParser.SwitchStmtContext context)
    {
        _context.Limiter.CheckAndCountStatement();
        _metrics.StatementCount++;

        var switchValue = Visit(context.expression(0));
        var caseExpressions = context.CASE().Length;
        bool matchFound = false;

        for (int i = 0; i < caseExpressions; i++)
        {
            var caseValue = Visit(context.expression(i + 1));

            if (AreEqual(switchValue, caseValue))
            {
                matchFound = true;
                _context.Variables.PushScope();
                try
                {
                    foreach (var stmt in GetCaseStatements(context, i))
                    {
                        Visit(stmt);
                    }
                }
                finally
                {
                    _context.Variables.PopScope();
                }
                break;
            }
        }

        if (!matchFound && context.DEFAULT() != null)
        {
            _context.Variables.PushScope();
            try
            {
                foreach (var stmt in GetDefaultStatements(context))
                {
                    Visit(stmt);
                }
            }
            finally
            {
                _context.Variables.PopScope();
            }
        }

        return JyroNull.Instance;
    }

    public override JyroValue VisitWhileStmt(JyroParser.WhileStmtContext context)
    {
        _context.Limiter.CheckAndCountStatement();
        _metrics.StatementCount++;

        while (true)
        {
            _context.CancellationToken.ThrowIfCancellationRequested();

            var condition = Visit(context.expression());
            if (!IsTruthy(condition))
            {
                break;
            }

            _metrics.LoopCount++;
            _context.Limiter.CheckAndEnterLoop();

            _context.Variables.PushScope();
            try
            {
                foreach (var stmt in context.statement())
                {
                    Visit(stmt);
                }
            }
            catch (BreakControlFlowException)
            {
                _context.Variables.PopScope();
                break;
            }
            catch (ContinueControlFlowException)
            {
                _context.Variables.PopScope();
                continue;
            }
            finally
            {
                _context.Limiter.ExitLoop();
                if (_context.Variables != null)
                {
                    // Scope may have been popped in catch
                }
            }
        }

        return JyroNull.Instance;
    }

    public override JyroValue VisitForEachStmt(JyroParser.ForEachStmtContext context)
    {
        _context.Limiter.CheckAndCountStatement();
        _metrics.StatementCount++;

        var iteratorName = context.Identifier().GetText();
        var expressionText = context.expression().GetText();
        var collection = Visit(context.expression());

        if (collection is JyroArray array)
        {
            foreach (var item in array)
            {
                _context.CancellationToken.ThrowIfCancellationRequested();
                _metrics.LoopCount++;
                _context.Limiter.CheckAndEnterLoop();

                _context.Variables.PushScope();
                try
                {
                    _context.Variables.Declare(iteratorName, item);
                    foreach (var stmt in context.statement())
                    {
                        Visit(stmt);
                    }
                }
                catch (BreakControlFlowException)
                {
                    _context.Variables.PopScope();
                    break;
                }
                catch (ContinueControlFlowException)
                {
                    _context.Variables.PopScope();
                    continue;
                }
                finally
                {
                    _context.Limiter.ExitLoop();
                    if (_context.Variables != null)
                    {
                        // Scope may have been popped in catch
                    }
                }
            }
        }
        else if (collection is JyroObject obj)
        {
            foreach (var kvp in obj)
            {
                _context.CancellationToken.ThrowIfCancellationRequested();
                _metrics.LoopCount++;
                _context.Limiter.CheckAndEnterLoop();

                _context.Variables.PushScope();
                try
                {
                    _context.Variables.Declare(iteratorName, new JyroString(kvp.Key));
                    foreach (var stmt in context.statement())
                    {
                        Visit(stmt);
                    }
                }
                catch (BreakControlFlowException)
                {
                    _context.Variables.PopScope();
                    break;
                }
                catch (ContinueControlFlowException)
                {
                    _context.Variables.PopScope();
                    continue;
                }
                finally
                {
                    _context.Limiter.ExitLoop();
                    if (_context.Variables != null)
                    {
                        // Scope may have been popped in catch
                    }
                }
            }
        }
        else
        {
            throw CreateException(MessageCode.NotIterable, context,
                $"Cannot iterate over non-iterable value of type '{collection.Type}'");
        }

        return JyroNull.Instance;
    }

    public override JyroValue VisitReturnStmt(JyroParser.ReturnStmtContext context)
    {
        _context.Limiter.CheckAndCountStatement();
        _metrics.StatementCount++;

        throw new ReturnControlFlowException(null);
    }

    public override JyroValue VisitBreakStmt(JyroParser.BreakStmtContext context)
    {
        _context.Limiter.CheckAndCountStatement();
        _metrics.StatementCount++;

        throw new BreakControlFlowException();
    }

    public override JyroValue VisitContinueStmt(JyroParser.ContinueStmtContext context)
    {
        _context.Limiter.CheckAndCountStatement();
        _metrics.StatementCount++;

        throw new ContinueControlFlowException();
    }

    // ===== Expressions =====

    public override JyroValue VisitAssignPass(JyroParser.AssignPassContext context)
    {
        return Visit(context.conditionalExpr());
    }

    public override JyroValue VisitAssignMake(JyroParser.AssignMakeContext context)
    {
        var value = Visit(context.assignmentExpr());
        AssignToTarget(context.assignmentTarget(), value);
        return value;
    }

    public override JyroValue VisitConditionalExpr(JyroParser.ConditionalExprContext context)
    {
        var condition = Visit(context.logicalOrExpr());

        if (context.QMARK() != null)
        {
            return IsTruthy(condition)
                ? Visit(context.conditionalExpr(0))
                : Visit(context.conditionalExpr(1));
        }

        return condition;
    }

    public override JyroValue VisitLogicalOrExpr(JyroParser.LogicalOrExprContext context)
    {
        var left = Visit(context.logicalAndExpr(0));

        // If there's only one operand (no OR operations), return it as-is
        if (context.logicalAndExpr().Length == 1)
        {
            return left;
        }

        // Multiple operands - perform OR logic and return boolean
        for (int i = 1; i < context.logicalAndExpr().Length; i++)
        {
            if (IsTruthy(left))
            {
                return JyroBoolean.FromBoolean(true);
            }
            left = Visit(context.logicalAndExpr(i));
        }

        return JyroBoolean.FromBoolean(IsTruthy(left));
    }

    public override JyroValue VisitLogicalAndExpr(JyroParser.LogicalAndExprContext context)
    {
        var left = Visit(context.equalityExpr(0));

        // If there's only one operand (no AND operations), return it as-is
        if (context.equalityExpr().Length == 1)
        {
            return left;
        }

        // Multiple operands - perform AND logic with short-circuit evaluation
        for (int i = 1; i < context.equalityExpr().Length; i++)
        {
            // Short-circuit: if left is falsy, return false immediately
            if (!IsTruthy(left))
            {
                return JyroBoolean.FromBoolean(false);
            }

            // Left is truthy, evaluate right operand
            var right = Visit(context.equalityExpr(i));

            // If right is falsy, the AND result is false
            if (!IsTruthy(right))
            {
                return JyroBoolean.FromBoolean(false);
            }

            // Both are truthy, continue with right as the new left for next iteration
            left = right;
        }

        // All operands were truthy
        return JyroBoolean.FromBoolean(true);
    }

    public override JyroValue VisitEqualityExpr(JyroParser.EqualityExprContext context)
    {
        var left = Visit(context.relationalExpr(0));

        for (int i = 1; i < context.relationalExpr().Length; i++)
        {
            var child = context.GetChild(i * 2 - 1);
            if (child == null)
            {
                throw CreateException(MessageCode.InvalidExpressionSyntax, context, "Invalid equality expression syntax");
            }

            var op = child.GetText();
            var right = Visit(context.relationalExpr(i));

            left = op switch
            {
                "==" => JyroBoolean.FromBoolean(AreEqual(left, right)),
                "!=" => JyroBoolean.FromBoolean(!AreEqual(left, right)),
                _ => throw CreateException(MessageCode.UnknownOperator, context, $"Unknown equality operator: {op}")
            };
        }

        return left;
    }

    public override JyroValue VisitRelationalExpr(JyroParser.RelationalExprContext context)
    {
        var left = Visit(context.additiveExpr(0));

        for (int i = 1; i < context.additiveExpr().Length; i++)
        {
            var child = context.GetChild(i * 2 - 1);
            if (child == null)
            {
                throw CreateException(MessageCode.InvalidExpressionSyntax, context, "Invalid relational expression syntax");
            }

            var op = child.GetText();
            var right = Visit(context.additiveExpr(i));

            if (op == "is")
            {
                left = JyroBoolean.FromBoolean(CheckTypeIs(left, right, context));
            }
            else if (op == "is not")
            {
                left = JyroBoolean.FromBoolean(!CheckTypeIs(left, right, context));
            }
            else
            {
                left = CompareRelational(left, right, op, context);
            }
        }

        return left;
    }

    public override JyroValue VisitAdditiveExpr(JyroParser.AdditiveExprContext context)
    {
        var left = Visit(context.multiplicativeExpr(0));

        for (int i = 1; i < context.multiplicativeExpr().Length; i++)
        {
            var child = context.GetChild(i * 2 - 1);
            if (child == null)
            {
                throw CreateException(MessageCode.InvalidExpressionSyntax, context, "Invalid additive expression syntax");
            }

            var op = child.GetText();
            var right = Visit(context.multiplicativeExpr(i));

            left = op switch
            {
                "+" => Add(left, right, context),
                "-" => Subtract(left, right, context),
                _ => throw CreateException(MessageCode.UnknownOperator, context, $"Unknown additive operator: {op}")
            };
        }

        return left;
    }

    public override JyroValue VisitMultiplicativeExpr(JyroParser.MultiplicativeExprContext context)
    {
        var left = Visit(context.unaryExpr(0));

        for (int i = 1; i < context.unaryExpr().Length; i++)
        {
            var child = context.GetChild(i * 2 - 1);
            if (child == null)
            {
                throw CreateException(MessageCode.InvalidExpressionSyntax, context, "Invalid multiplicative expression syntax");
            }

            var op = child.GetText();
            var right = Visit(context.unaryExpr(i));

            left = op switch
            {
                "*" => Multiply(left, right, context),
                "/" => Divide(left, right, context),
                "%" => Modulo(left, right, context),
                _ => throw CreateException(MessageCode.UnknownOperator, context, $"Unknown multiplicative operator: {op}")
            };
        }

        return left;
    }

    public override JyroValue VisitUnaryExpr(JyroParser.UnaryExprContext context)
    {
        if (context.NOT() != null)
        {
            var operand = Visit(context.unaryExpr());
            return JyroBoolean.FromBoolean(!IsTruthy(operand));
        }

        if (context.SUB() != null)
        {
            var operand = Visit(context.unaryExpr());
            if (operand is JyroNumber num)
            {
                return new JyroNumber(-num.Value);
            }
            throw CreateException(MessageCode.NegateNonNumber, context,
                $"Cannot negate non-number value of type '{operand.Type}'");
        }

        return Visit(context.postfixExpr());
    }

    public override JyroValue VisitPostfixExpr(JyroParser.PostfixExprContext context)
    {
        var value = Visit(context.primaryExpr());

        foreach (var suffix in context.postfixSuffix())
        {
            if (suffix.LPAREN() != null)
            {
                // Function call
                value = InvokeFunction(value, suffix.argList(), context);
            }
            else if (suffix.memberOrIndex() != null)
            {
                // Member access or indexing
                value = AccessMemberOrIndex(value, suffix.memberOrIndex(), context);
            }
        }

        return value;
    }

    public override JyroValue VisitPrimaryExpr(JyroParser.PrimaryExprContext context)
    {
        if (context.literal() != null)
        {
            return Visit(context.literal());
        }

        if (context.Identifier() != null)
        {
            var name = context.Identifier().GetText();

            // Try to resolve as a variable first
            if (_context.Variables.TryGet(name, out var value))
            {
                return value;
            }

            // If not a variable, return as a string (could be a function name)
            // The function call logic in VisitPostfixExpr will handle it
            return new JyroString(name);
        }

        if (context.typeKeyword() != null)
        {
            return Visit(context.typeKeyword());
        }

        if (context.DATA() != null)
        {
            JyroValue value;
            if (_context.Variables.TryGet(JyroExecutionContext.RootIdentifier, out var dataValue))
            {
                value = dataValue;
            }
            else
            {
                value = JyroNull.Instance;
            }

            foreach (var accessor in context.memberOrIndex())
            {
                value = AccessMemberOrIndex(value, accessor, context);
            }

            return value;
        }

        if (context.LPAREN() != null)
        {
            return Visit(context.expression());
        }

        if (context.objectLiteral() != null)
        {
            return Visit(context.objectLiteral());
        }

        if (context.arrayLiteral() != null)
        {
            return Visit(context.arrayLiteral());
        }

        return JyroNull.Instance;
    }

    public override JyroValue VisitTypeKeyword(JyroParser.TypeKeywordContext context)
    {
        // Convert type keyword to its string name for use in type checking
        var typeText = context.GetText();
        return new JyroString(typeText);
    }

    public override JyroValue VisitLiteral(JyroParser.LiteralContext context)
    {
        if (context.numberLiteral() != null)
        {
            return Visit(context.numberLiteral());
        }

        if (context.stringLiteral() != null)
        {
            return Visit(context.stringLiteral());
        }

        if (context.TRUE() != null)
        {
            return JyroBoolean.FromBoolean(true);
        }

        if (context.FALSE() != null)
        {
            return JyroBoolean.FromBoolean(false);
        }

        if (context.NULL() != null)
        {
            return JyroNull.Instance;
        }

        return JyroNull.Instance;
    }

    public override JyroValue VisitNumberLiteral(JyroParser.NumberLiteralContext context)
    {
        var text = context.Number().GetText();
        if (double.TryParse(text, out var number))
        {
            return new JyroNumber(number);
        }
        throw CreateException(MessageCode.InvalidNumberParse, context, $"Invalid number: '{text}'");
    }

    public override JyroValue VisitStringLiteral(JyroParser.StringLiteralContext context)
    {
        var text = context.String().GetText();
        // Remove quotes and handle escape sequences
        var unescaped = UnescapeString(text[1..^1]);
        return new JyroString(unescaped);
    }

    public override JyroValue VisitObjectLiteral(JyroParser.ObjectLiteralContext context)
    {
        var obj = new JyroObject();

        foreach (var entry in context.objectEntry())
        {
            string key;
            if (entry.String() != null)
            {
                key = UnescapeString(entry.String().GetText()[1..^1]);
            }
            else
            {
                var keyValue = Visit(entry.expression(0));
                key = keyValue.ToStringValue();
            }

            var value = Visit(entry.expression()[^1]);
            obj.SetProperty(key, value);
        }

        return obj;
    }

    public override JyroValue VisitArrayLiteral(JyroParser.ArrayLiteralContext context)
    {
        var array = new JyroArray();

        foreach (var expr in context.expression())
        {
            array.Add(Visit(expr));
        }

        return array;
    }

    // ===== Helper Methods =====

    private JyroValue EvaluateAssignmentTarget(JyroParser.AssignmentTargetContext context)
    {
        if (context.Identifier() != null)
        {
            var name = context.Identifier().GetText();
            var value = _context.Variables.TryGet(name, out var v) ? v : JyroNull.Instance;

            foreach (var accessor in context.memberOrIndex())
            {
                value = AccessMemberOrIndex(value, accessor, context);
            }

            return value;
        }

        if (context.DATA() != null)
        {
            var value = _context.Variables.TryGet(JyroExecutionContext.RootIdentifier, out var v)
                ? v
                : JyroNull.Instance;

            foreach (var accessor in context.memberOrIndex())
            {
                value = AccessMemberOrIndex(value, accessor, context);
            }

            return value;
        }

        return JyroNull.Instance;
    }

    private void AssignToTarget(JyroParser.AssignmentTargetContext context, JyroValue value)
    {
        if (context.Identifier() != null)
        {
            var name = context.Identifier().GetText();
            var accessors = context.memberOrIndex();

            if (accessors.Length == 0)
            {
                if (!_context.Variables.TrySet(name, value))
                {
                    _context.Variables.Declare(name, value);
                }
            }
            else
            {
                var target = _context.Variables.TryGet(name, out var v) ? v : JyroNull.Instance;
                AssignToNestedTarget(target, accessors, value, context);
            }
        }
        else if (context.DATA() != null)
        {
            var target = _context.Variables.TryGet(JyroExecutionContext.RootIdentifier, out var v)
                ? v
                : JyroNull.Instance;
            AssignToNestedTarget(target, context.memberOrIndex(), value, context);
        }
    }

    private void AssignToNestedTarget(JyroValue target, JyroParser.MemberOrIndexContext[] accessors, JyroValue value, ParserRuleContext parentContext)
    {
        for (int i = 0; i < accessors.Length - 1; i++)
        {
            target = AccessMemberOrIndex(target, accessors[i], parentContext);
        }

        var lastAccessor = accessors[^1];
        if (lastAccessor.DOT() != null)
        {
            var memberName = lastAccessor.propertyName().GetText();
            if (target is JyroObject obj)
            {
                obj.SetProperty(memberName, value);
            }
            else
            {
                throw CreateException(MessageCode.PropertyAccessInvalidType, lastAccessor,
                    $"Cannot set property '{memberName}' on type '{target.Type}'");
            }
        }
        else if (lastAccessor.LBRACK() != null)
        {
            var indexValue = Visit(lastAccessor.expression());
            if (target is JyroArray arr && indexValue is JyroNumber num)
            {
                var index = (int)num.Value;
                if (index >= 0 && index < arr.Length)
                {
                    arr[index] = value;
                }
                else
                {
                    throw CreateException(MessageCode.IndexOutOfRange, lastAccessor,
                        $"Array index {index} is out of bounds (array length: {arr.Length})");
                }
            }
            else if (target is JyroObject obj)
            {
                var key = indexValue.ToStringValue();
                obj.SetProperty(key, value);
            }
            else
            {
                throw CreateException(MessageCode.InvalidIndexTarget, lastAccessor,
                    $"Cannot index type '{target.Type}'");
            }
        }
    }

    private JyroValue AccessMemberOrIndex(JyroValue target, JyroParser.MemberOrIndexContext context, ParserRuleContext parentContext)
    {
        if (context.DOT() != null)
        {
            var memberName = context.propertyName().GetText();

            if (target is JyroNull)
            {
                throw CreateException(MessageCode.PropertyAccessOnNull, parentContext,
                    $"Cannot access property '{memberName}' on null");
            }

            if (target is JyroObject obj)
            {
                return obj.GetProperty(memberName);
            }

            throw CreateException(MessageCode.PropertyAccessInvalidType, parentContext,
                $"Cannot access property '{memberName}' on type '{target.Type}'");
        }

        if (context.LBRACK() != null)
        {
            var indexValue = Visit(context.expression());

            if (target is JyroArray arr && indexValue is JyroNumber num)
            {
                var index = (int)num.Value;

                if (index < 0)
                {
                    throw CreateException(MessageCode.NegativeIndex, parentContext,
                        $"Array index cannot be negative: {index}");
                }

                if (index >= arr.Length)
                {
                    throw CreateException(MessageCode.IndexOutOfRange, parentContext,
                        $"Array index {index} is out of bounds (array length: {arr.Length})");
                }

                return arr[index];
            }
            else if (target is JyroObject obj)
            {
                var key = indexValue.ToStringValue();
                return obj.GetProperty(key);
            }
            else if (target is JyroNull)
            {
                throw CreateException(MessageCode.IndexAccessOnNull, parentContext,
                    "Cannot access index on null");
            }

            throw CreateException(MessageCode.InvalidIndexTarget, parentContext,
                $"Cannot access index on type '{target.Type}'");
        }

        return JyroNull.Instance;
    }

    private JyroValue InvokeFunction(JyroValue target, JyroParser.ArgListContext? argListContext, ParserRuleContext parentContext)
    {
        _metrics.FunctionCallCount++;
        _metrics.CurrentCallDepth++;
        if (_metrics.CurrentCallDepth > _metrics.MaxCallDepth)
        {
            _metrics.MaxCallDepth = _metrics.CurrentCallDepth;
        }

        try
        {
            _context.Limiter.CheckAndEnterCall();

            if (target is not JyroString functionName)
            {
                throw CreateException(MessageCode.InvalidFunctionTarget, parentContext,
                    "Only named functions can be called");
            }

            if (!_context.Functions.TryGetValue(functionName.Value, out var function))
            {
                throw CreateException(MessageCode.UndefinedFunctionRuntime, parentContext,
                    $"Undefined function: '{functionName.Value}'");
            }

            var args = new List<JyroValue>();
            if (argListContext != null)
            {
                foreach (var expr in argListContext.expression())
                {
                    args.Add(Visit(expr));
                }
            }

            return function.Execute(args, _context);
        }
        finally
        {
            _context.Limiter.ExitCall();
            _metrics.CurrentCallDepth--;
        }
    }

    private bool IsTruthy(JyroValue value)
    {
        return value switch
        {
            JyroNull => false,
            JyroBoolean b => b.Value,
            JyroNumber n => n.Value != 0,
            JyroString s => !string.IsNullOrEmpty(s.Value),
            _ => true
        };
    }

    private bool AreEqual(JyroValue left, JyroValue right)
    {
        if (left is JyroNull && right is JyroNull)
        {
            return true;
        }

        if (left is JyroNull || right is JyroNull)
        {
            return false;
        }

        if (left is JyroNumber ln && right is JyroNumber rn)
        {
            return Math.Abs(ln.Value - rn.Value) < double.Epsilon;
        }

        if (left is JyroString ls && right is JyroString rs)
        {
            return ls.Value == rs.Value;
        }

        if (left is JyroBoolean lb && right is JyroBoolean rb)
        {
            return lb.Value == rb.Value;
        }

        return false;
    }

    private bool CheckTypeIs(JyroValue value, JyroValue typeValue, ParserRuleContext context)
    {
        // Handle null as a special case (since it's both a literal and a type name)
        if (typeValue is JyroNull)
        {
            return value is JyroNull;
        }

        if (typeValue is not JyroString typeName)
        {
            throw CreateException(MessageCode.InvalidTypeCheck, context,
                $"Type check requires a string type name, got '{typeValue.Type}'");
        }

        return typeName.Value.ToLowerInvariant() switch
        {
            "number" => value is JyroNumber,
            "string" => value is JyroString,
            "boolean" => value is JyroBoolean,
            "object" => value is JyroObject,
            "array" => value is JyroArray,
            "null" => value is JyroNull,
            _ => throw CreateException(MessageCode.UnknownTypeName, context,
                $"Unknown type name: '{typeName.Value}'")
        };
    }

    private JyroValue CompareRelational(JyroValue left, JyroValue right, string op, ParserRuleContext context)
    {
        if (left is JyroNumber ln && right is JyroNumber rn)
        {
            return op switch
            {
                "<" => JyroBoolean.FromBoolean(ln.Value < rn.Value),
                "<=" => JyroBoolean.FromBoolean(ln.Value <= rn.Value),
                ">" => JyroBoolean.FromBoolean(ln.Value > rn.Value),
                ">=" => JyroBoolean.FromBoolean(ln.Value >= rn.Value),
                _ => throw CreateException(MessageCode.UnknownOperator, context, $"Unknown relational operator: '{op}'")
            };
        }

        if (left is JyroString ls && right is JyroString rs)
        {
            var cmp = string.Compare(ls.Value, rs.Value, StringComparison.Ordinal);
            return op switch
            {
                "<" => JyroBoolean.FromBoolean(cmp < 0),
                "<=" => JyroBoolean.FromBoolean(cmp <= 0),
                ">" => JyroBoolean.FromBoolean(cmp > 0),
                ">=" => JyroBoolean.FromBoolean(cmp >= 0),
                _ => throw CreateException(MessageCode.UnknownOperator, context, $"Unknown relational operator: '{op}'")
            };
        }

        throw CreateException(MessageCode.IncompatibleComparison, context,
            $"Cannot compare types '{left.Type}' and '{right.Type}'");
    }

    private JyroValue Add(JyroValue left, JyroValue right, ParserRuleContext context)
    {
        if (left is JyroNumber ln && right is JyroNumber rn)
        {
            return new JyroNumber(ln.Value + rn.Value);
        }

        if (left is JyroString || right is JyroString)
        {
            return new JyroString(left.ToStringValue() + right.ToStringValue());
        }

        throw CreateException(MessageCode.IncompatibleOperandTypes, context,
            $"Cannot add types '{left.Type}' and '{right.Type}'");
    }

    private JyroValue Subtract(JyroValue left, JyroValue right, ParserRuleContext context)
    {
        if (left is JyroNumber ln && right is JyroNumber rn)
        {
            return new JyroNumber(ln.Value - rn.Value);
        }

        throw CreateException(MessageCode.IncompatibleOperandTypes, context,
            $"Cannot subtract types '{left.Type}' and '{right.Type}'");
    }

    private JyroValue Multiply(JyroValue left, JyroValue right, ParserRuleContext context)
    {
        if (left is JyroNumber ln && right is JyroNumber rn)
        {
            return new JyroNumber(ln.Value * rn.Value);
        }

        throw CreateException(MessageCode.IncompatibleOperandTypes, context,
            $"Cannot multiply types '{left.Type}' and '{right.Type}'");
    }

    private JyroValue Divide(JyroValue left, JyroValue right, ParserRuleContext context)
    {
        if (left is JyroNumber ln && right is JyroNumber rn)
        {
            if (Math.Abs(rn.Value) < double.Epsilon)
            {
                throw CreateException(MessageCode.DivisionByZero, context, "Division by zero");
            }
            return new JyroNumber(ln.Value / rn.Value);
        }

        throw CreateException(MessageCode.IncompatibleOperandTypes, context,
            $"Cannot divide types '{left.Type}' and '{right.Type}'");
    }

    private JyroValue Modulo(JyroValue left, JyroValue right, ParserRuleContext context)
    {
        if (left is JyroNumber ln && right is JyroNumber rn)
        {
            if (Math.Abs(rn.Value) < double.Epsilon)
            {
                throw CreateException(MessageCode.DivisionByZero, context, "Division by zero");
            }
            return new JyroNumber(ln.Value % rn.Value);
        }

        throw CreateException(MessageCode.IncompatibleOperandTypes, context,
            $"Cannot modulo types '{left.Type}' and '{right.Type}'");
    }

    private string UnescapeString(string escaped)
    {
        return escaped
            .Replace("\\n", "\n")
            .Replace("\\r", "\r")
            .Replace("\\t", "\t")
            .Replace("\\b", "\b")
            .Replace("\\f", "\f")
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\")
            .Replace("\\/", "/");
    }

    // Helper methods for parsing if/switch statement structures
    // These methods walk the parse tree to partition statements into their respective branches

    private IEnumerable<JyroParser.StatementContext> GetMainIfStatements(JyroParser.IfStmtContext context)
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

            // If we're past THEN, collect statements until we hit ELSE or END
            if (pastThen)
            {
                // Stop at ELSE or END
                if (child is ITerminalNode term && (term.Symbol.Type == JyroParser.ELSE || term.Symbol.Type == JyroParser.END))
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

    private int GetStatementsCount(JyroParser.IfStmtContext context, int branchIndex)
    {
        // Not currently used, but kept for potential future use
        return context.statement().Length;
    }

    private IEnumerable<JyroParser.StatementContext> GetElseIfStatements(JyroParser.IfStmtContext context, int elseIfIndex)
    {
        // Walk through children to find statements between the Nth "ELSE IF...THEN" and the next keyword
        var children = context.children;
        int elseIfCount = 0;
        bool inTargetBranch = false;

        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];

            // Check if this is "ELSE" followed by "IF"
            if (child is ITerminalNode terminal && terminal.Symbol.Type == JyroParser.ELSE)
            {
                if (i + 1 < children.Count && children[i + 1] is ITerminalNode nextTerminal
                    && nextTerminal.Symbol.Type == JyroParser.IF)
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
            }

            // If we're in the target branch and hit a THEN, start collecting statements
            if (inTargetBranch && child is ITerminalNode thenNode && thenNode.Symbol.Type == JyroParser.THEN)
            {
                inTargetBranch = false; // Now we're past THEN, collect statements
                for (int j = i + 1; j < children.Count; j++)
                {
                    var stmt = children[j];

                    // Stop at the next keyword (ELSE, END)
                    if (stmt is ITerminalNode term && (term.Symbol.Type == JyroParser.ELSE || term.Symbol.Type == JyroParser.END))
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

    private IEnumerable<JyroParser.StatementContext> GetElseStatements(JyroParser.IfStmtContext context)
    {
        // Find the final ELSE (not followed by IF) and collect statements until END
        var children = context.children;
        bool inElseBranch = false;

        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];

            // Check if this is "ELSE" NOT followed by "IF"
            if (child is ITerminalNode terminal && terminal.Symbol.Type == JyroParser.ELSE)
            {
                if (i + 1 < children.Count)
                {
                    var next = children[i + 1];
                    if (next is ITerminalNode nextTerminal && nextTerminal.Symbol.Type == JyroParser.IF)
                    {
                        continue; // This is ELSE IF, not the final ELSE
                    }
                }

                // This is the final ELSE
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

    private IEnumerable<JyroParser.StatementContext> GetCaseStatements(JyroParser.SwitchStmtContext context, int caseIndex)
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

    private IEnumerable<JyroParser.StatementContext> GetDefaultStatements(JyroParser.SwitchStmtContext context)
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

    // ===== Execution Metrics =====

    private struct ExecutionMetrics
    {
        public int StatementCount;
        public int LoopCount;
        public int FunctionCallCount;
        public int CurrentCallDepth;
        public int MaxCallDepth;
    }

    // ===== Control Flow Exceptions =====

    internal sealed class BreakControlFlowException : Exception { }

    internal sealed class ContinueControlFlowException : Exception { }

    internal sealed class ReturnControlFlowException : Exception
    {
        public JyroValue? ReturnValue { get; }
        public ReturnControlFlowException(JyroValue? returnValue = null) => ReturnValue = returnValue;
    }
}
