using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Mesch.Jyro;

/// <summary>
/// Primary executor implementation that processes linked Jyro programs using the visitor pattern.
/// Delegates type-specific operations to JyroValue implementations and manages execution
/// context including variable scopes, function calls, and control flow.
/// </summary>
public sealed class Executor : IExecutor, IVisitor<JyroValue>
{
    private readonly ILogger<Executor> _logger;
    private JyroExecutionContext? _currentExecutionContext;
    private List<IMessage>? _diagnosticMessages;
    private ExecutionMetrics _executionMetrics;

    /// <summary>
    /// Initializes a new instance of the <see cref="Executor"/> class with the specified logger.
    /// </summary>
    /// <param name="logger">
    /// The logger instance for tracking execution operations and diagnostics.
    /// Cannot be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> is null.
    /// </exception>
    public Executor(ILogger<Executor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public JyroExecutionResult Execute(ILinkedProgram linkedProgram, JyroExecutionContext executionContext)
    {
        ArgumentNullException.ThrowIfNull(linkedProgram);
        ArgumentNullException.ThrowIfNull(executionContext);

        var executionStartedAt = DateTimeOffset.UtcNow;
        var executionStopwatch = Stopwatch.StartNew();

        _logger.LogTrace("Beginning execution with {StatementCount} statements", linkedProgram.Statements.Count);

        _currentExecutionContext = executionContext;
        _diagnosticMessages = [];
        _executionMetrics = new ExecutionMetrics();

        EnsureRootDataVariableExists(executionContext);

        try
        {
            foreach (var programStatement in linkedProgram.Statements)
            {
                executionContext.CancellationToken.ThrowIfCancellationRequested();
                ExecuteStatement(programStatement);
            }
        }
        catch (ReturnControlFlowException returnException)
        {
            _logger.LogTrace("Return statement encountered, terminating execution early");
            if (returnException.ReturnValue != null)
            {
                executionContext.Variables.Declare("Return", returnException.ReturnValue);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Execution cancelled by cancellation token");
            AddExecutionError(MessageCode.CancelledByHost, 0, 0, "Execution was cancelled");
        }
        catch (JyroRuntimeException jyroRuntimeException)
        {
            _logger.LogError(jyroRuntimeException, "Runtime error occurred during execution");
            AddExecutionError(MessageCode.RuntimeError, 0, 0, jyroRuntimeException.Message);
        }
        catch (Exception unexpectedException)
        {
            _logger.LogError(unexpectedException, "Unexpected error occurred during execution");
            AddExecutionError(MessageCode.UnknownExecutorError, 0, 0, unexpectedException.Message);
        }

        executionStopwatch.Stop();
        var executionSucceeded = !_diagnosticMessages.Any(message => message.Severity == MessageSeverity.Error);

        var executionMetadata = new ExecutionMetadata(
            executionStopwatch.Elapsed,
            _executionMetrics.StatementCount,
            _executionMetrics.LoopCount,
            _executionMetrics.FunctionCallCount,
            _executionMetrics.MaxCallDepth,
            executionStartedAt);

        _logger.LogTrace("Execution completed: success={Success}, statements={Statements}, loops={Loops}, functionCalls={Calls}, maxDepth={Depth}, errors={Errors}, elapsed={ElapsedMs}ms",
            executionSucceeded, _executionMetrics.StatementCount, _executionMetrics.LoopCount, _executionMetrics.FunctionCallCount, _executionMetrics.MaxCallDepth,
            _diagnosticMessages.Count(message => message.Severity == MessageSeverity.Error),
            executionStopwatch.ElapsedMilliseconds);

        if (!executionContext.Variables.TryGet(JyroExecutionContext.RootIdentifier, out var finalRootData) || finalRootData is null)
        {
            finalRootData = JyroNull.Instance;
        }

        return new JyroExecutionResult(executionSucceeded, finalRootData, _diagnosticMessages, executionMetadata);
    }

    #region Statement Visitors

    /// <inheritdoc />
    public JyroValue VisitVariableDeclarationStatement(VariableDeclarationStatement statement)
    {
        _currentExecutionContext!.Limiter.CheckAndCountStatement();
        _executionMetrics.StatementCount++;

        var initialValue = statement.Initializer?.Accept(this) ?? JyroNull.Instance;

        if (statement.TypeHint != null)
        {
            var expectedValueType = GetExpectedTypeFromHint(statement.TypeHint);
            if (!IsValueCompatibleWithType(initialValue, expectedValueType))
            {
                AddExecutionError(MessageCode.RuntimeError, statement.LineNumber, statement.ColumnPosition,
                    $"Type mismatch: expected {expectedValueType}, but received {initialValue.Type}");
                return JyroNull.Instance;
            }
        }

        _currentExecutionContext!.Variables.Declare(statement.Name, initialValue);
        return JyroNull.Instance;
    }

    /// <inheritdoc />
    public JyroValue VisitAssignmentStatement(AssignmentStatement statement)
    {
        _currentExecutionContext!.Limiter.CheckAndCountStatement();
        _executionMetrics.StatementCount++;

        var assignedValue = statement.Value.Accept(this);
        AssignValueToTarget(statement.Target, assignedValue);
        return JyroNull.Instance;
    }

    /// <inheritdoc />
    public JyroValue VisitExpressionStatement(ExpressionStatement statement)
    {
        _currentExecutionContext!.Limiter.CheckAndCountStatement();
        _executionMetrics.StatementCount++;
        statement.Expression.Accept(this);
        return JyroNull.Instance;
    }

    /// <inheritdoc />
    public JyroValue VisitIfStatement(IfStatement statement)
    {
        _currentExecutionContext!.Limiter.CheckAndCountStatement();
        _executionMetrics.StatementCount++;

        var conditionResult = statement.Condition.Accept(this);
        var shouldExecuteThenBranch = conditionResult.ToBooleanTruthiness();

        _logger.LogTrace("If condition result={Result}, executing={Branch}",
            shouldExecuteThenBranch, shouldExecuteThenBranch ? "then" : "else");

        _currentExecutionContext!.Variables.PushScope();
        try
        {
            if (shouldExecuteThenBranch)
            {
                foreach (var thenStatement in statement.ThenBranch)
                {
                    ExecuteStatement(thenStatement);
                }
                return JyroNull.Instance;
            }

            foreach (var elseIfClause in statement.ElseIfClauses)
            {
                var elseIfConditionResult = elseIfClause.Condition.Accept(this);
                var shouldExecuteElseIfBranch = elseIfConditionResult.ToBooleanTruthiness();

                _logger.LogTrace("Else-if condition result={Result}", shouldExecuteElseIfBranch);

                if (shouldExecuteElseIfBranch)
                {
                    foreach (var elseIfStatement in elseIfClause.Statements)
                    {
                        ExecuteStatement(elseIfStatement);
                    }
                    return JyroNull.Instance;
                }
            }

            if (statement.ElseBranch.Count > 0)
            {
                _logger.LogTrace("Executing final else clause");
                foreach (var elseStatement in statement.ElseBranch)
                {
                    ExecuteStatement(elseStatement);
                }
            }
        }
        finally
        {
            _currentExecutionContext.Variables.PopScope();
        }

        return JyroNull.Instance;
    }

    /// <inheritdoc />
    public JyroValue VisitSwitchStatement(SwitchStatement statement)
    {
        _currentExecutionContext!.Limiter.CheckAndCountStatement();
        _executionMetrics.StatementCount++;

        var switchExpressionValue = statement.Expression.Accept(this);
        var foundMatchingCase = false;

        _currentExecutionContext!.Variables.PushScope();
        try
        {
            foreach (var caseClause in statement.Cases)
            {
                var caseMatchValue = caseClause.MatchExpression.Accept(this);
                if (AreValuesEqual(switchExpressionValue, caseMatchValue))
                {
                    foundMatchingCase = true;
                    foreach (var caseStatement in caseClause.Body)
                    {
                        ExecuteStatement(caseStatement);
                    }
                    break;
                }
            }

            if (!foundMatchingCase && statement.DefaultBranch.Count > 0)
            {
                foreach (var defaultStatement in statement.DefaultBranch)
                {
                    ExecuteStatement(defaultStatement);
                }
            }
        }
        catch (BreakControlFlowException)
        {
            // Clean exit from switch statement
        }
        catch (ContinueControlFlowException)
        {
            throw; // Bubble up to outer loop
        }
        finally
        {
            _currentExecutionContext.Variables.PopScope();
        }

        return JyroNull.Instance;
    }

    /// <inheritdoc />
    public JyroValue VisitWhileStatement(WhileStatement statement)
    {
        _currentExecutionContext!.Limiter.CheckAndCountStatement();
        _executionMetrics.StatementCount++;

        while (true)
        {
            var conditionResult = statement.Condition.Accept(this);
            if (!conditionResult.ToBooleanTruthiness())
            {
                break;
            }

            _currentExecutionContext!.Limiter.CheckAndEnterLoop();
            _executionMetrics.LoopCount++;
            _logger.LogTrace("While loop iteration; totalLoops={LoopCount}", _executionMetrics.LoopCount);

            _currentExecutionContext!.Variables.PushScope();
            try
            {
                foreach (var loopStatement in statement.Body)
                {
                    ExecuteStatement(loopStatement);
                }
            }
            catch (BreakControlFlowException)
            {
                break;
            }
            catch (ContinueControlFlowException)
            {
                continue;
            }
            finally
            {
                _currentExecutionContext.Variables.PopScope();
                _currentExecutionContext.Limiter.ExitLoop();
            }
        }

        return JyroNull.Instance;
    }

    /// <inheritdoc />
    public JyroValue VisitForEachStatement(ForEachStatement statement)
    {
        _currentExecutionContext!.Limiter.CheckAndCountStatement();
        _executionMetrics.StatementCount++;

        var sourceCollectionValue = statement.Source.Accept(this);
        var iterableCollection = sourceCollectionValue.ToIterable();

        foreach (var iterationItem in iterableCollection)
        {
            _currentExecutionContext!.Limiter.CheckAndEnterLoop();
            _executionMetrics.LoopCount++;
            _logger.LogTrace("Foreach iteration; iterator={Iterator}, totalLoops={LoopCount}",
                statement.IteratorName, _executionMetrics.LoopCount);

            _currentExecutionContext!.Variables.PushScope();
            try
            {
                _currentExecutionContext.Variables.Declare(statement.IteratorName, iterationItem);
                foreach (var loopStatement in statement.Body)
                {
                    ExecuteStatement(loopStatement);
                }
            }
            catch (BreakControlFlowException)
            {
                break;
            }
            catch (ContinueControlFlowException)
            {
                continue;
            }
            finally
            {
                _currentExecutionContext.Variables.PopScope();
                _currentExecutionContext.Limiter.ExitLoop();
            }
        }

        return JyroNull.Instance;
    }

    /// <inheritdoc />
    public JyroValue VisitReturnStatement(ReturnStatement statement)
    {
        _currentExecutionContext!.Limiter.CheckAndCountStatement();
        _executionMetrics.StatementCount++;
        var returnValue = statement.Value?.Accept(this);
        throw new ReturnControlFlowException(returnValue);
    }

    /// <inheritdoc />
    public JyroValue VisitBreakStatement(BreakStatement statement)
    {
        _currentExecutionContext!.Limiter.CheckAndCountStatement();
        _executionMetrics.StatementCount++;
        throw new BreakControlFlowException();
    }

    /// <inheritdoc />
    public JyroValue VisitContinueStatement(ContinueStatement statement)
    {
        _currentExecutionContext!.Limiter.CheckAndCountStatement();
        _executionMetrics.StatementCount++;
        throw new ContinueControlFlowException();
    }

    #endregion

    #region Expression Visitors

    /// <inheritdoc />
    public JyroValue VisitBinaryExpression(BinaryExpression expression)
    {
        var leftOperand = expression.Left.Accept(this);
        var rightOperand = expression.Right.Accept(this);
        return leftOperand.EvaluateBinary(expression.Operator, rightOperand);
    }

    /// <inheritdoc />
    public JyroValue VisitUnaryExpression(UnaryExpression expression)
    {
        var operandValue = expression.Operand.Accept(this);
        return operandValue.EvaluateUnary(expression.Operator);
    }

    /// <inheritdoc />
    public JyroValue VisitTernaryExpression(TernaryExpression expression)
    {
        var conditionValue = expression.Condition.Accept(this);
        var conditionResult = conditionValue.ToBooleanTruthiness();

        return conditionResult
            ? expression.TrueExpression.Accept(this)
            : expression.FalseExpression.Accept(this);
    }

    /// <inheritdoc />
    public JyroValue VisitLiteralExpression(LiteralExpression expression)
    {
        return JyroValue.FromObject(expression.Value);
    }

    /// <inheritdoc />
    public JyroValue VisitVariableExpression(VariableExpression expression)
    {
        return _currentExecutionContext!.Variables.TryGet(expression.Name, out var variableValue) ? variableValue : JyroNull.Instance;
    }

    /// <inheritdoc />
    public JyroValue VisitTypeExpression(TypeExpression expression)
    {
        return JyroNull.Instance;
    }

    /// <inheritdoc />
    public JyroValue VisitTypeCheckExpression(TypeCheckExpression expression)
    {
        var targetValue = expression.Target.Accept(this);
        var expectedValueType = MapTokenTypeToValueType(expression.CheckedType);
        var typesMatch = targetValue.Type == expectedValueType;
        return JyroBoolean.FromBoolean(typesMatch);
    }

    /// <inheritdoc />
    public JyroValue VisitPropertyAccessExpression(PropertyAccessExpression expression)
    {
        var targetObject = expression.Target.Accept(this);
        return targetObject.GetProperty(expression.PropertyName);
    }

    /// <inheritdoc />
    public JyroValue VisitIndexAccessExpression(IndexAccessExpression expression)
    {
        var targetCollection = expression.Target.Accept(this);
        var indexValue = expression.Index.Accept(this);
        return targetCollection.GetIndex(indexValue);
    }

    /// <inheritdoc />
    public JyroValue VisitFunctionCallExpression(FunctionCallExpression expression)
    {
        var resolvedFunctionName = ResolveFunctionName(expression.Target);

        _currentExecutionContext!.Limiter.CheckAndEnterCall();
        _executionMetrics.FunctionCallCount++;
        _executionMetrics.CurrentCallDepth++;
        _executionMetrics.MaxCallDepth = Math.Max(_executionMetrics.MaxCallDepth, _executionMetrics.CurrentCallDepth);

        _logger.LogTrace("Function call {Function} with {ArgumentCount} arguments",
            resolvedFunctionName, expression.Arguments.Count);

        try
        {
            if (!_currentExecutionContext!.Functions.TryGetValue(resolvedFunctionName, out var targetFunction))
            {
                AddExecutionError(MessageCode.UndefinedFunction, expression.LineNumber, expression.ColumnPosition, resolvedFunctionName);
                return JyroNull.Instance;
            }

            var evaluatedArguments = expression.Arguments.Select(argumentExpression => argumentExpression.Accept(this)).ToList();
            return targetFunction.Execute(evaluatedArguments, _currentExecutionContext);
        }
        catch (Exception functionException)
        {
            _logger.LogError(functionException, "Error in function {Function}", resolvedFunctionName);
            AddExecutionError(MessageCode.RuntimeError, expression.LineNumber, expression.ColumnPosition, functionException.Message);
            return JyroNull.Instance;
        }
        finally
        {
            _executionMetrics.CurrentCallDepth--;
            _currentExecutionContext.Limiter.ExitCall();
        }
    }

    /// <inheritdoc />
    public JyroValue VisitArrayLiteralExpression(ArrayLiteralExpression expression)
    {
        var resultArray = new JyroArray();
        foreach (var elementExpression in expression.Elements)
        {
            resultArray.Add(elementExpression.Accept(this));
        }
        return resultArray;
    }

    public JyroValue VisitObjectLiteralExpression(ObjectLiteralExpression expression)
    {
        var resultObject = new JyroObject();
        foreach (var propertyDefinition in expression.Properties)
        {
            var propertyValue = propertyDefinition.Value.Accept(this);
            resultObject.SetProperty(propertyDefinition.Key, propertyValue);
        }
        return resultObject;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Executes a single statement, handling control flow exceptions appropriately.
    /// </summary>
    /// <param name="statement">The statement to execute.</param>
    private void ExecuteStatement(IJyroStatement statement)
    {
        try
        {
            statement.Accept(this);
        }
        catch (BreakControlFlowException)
        {
            throw;
        }
        catch (ContinueControlFlowException)
        {
            throw;
        }
        catch (ReturnControlFlowException)
        {
            throw;
        }
    }

    /// <summary>
    /// Assigns a value to the specified assignment target, handling different target types.
    /// </summary>
    /// <param name="assignmentTarget">The target expression to assign to.</param>
    /// <param name="assignedValue">The value to assign.</param>
    private void AssignValueToTarget(IExpression assignmentTarget, JyroValue assignedValue)
    {
        switch (assignmentTarget)
        {
            case VariableExpression variableTarget:
                if (variableTarget.Name == JyroExecutionContext.RootIdentifier)
                {
                    AddExecutionError(MessageCode.InvalidAssignmentTarget, variableTarget.LineNumber, variableTarget.ColumnPosition,
                        $"Cannot reassign {JyroExecutionContext.RootIdentifier}");
                    return;
                }

                if (!_currentExecutionContext!.Variables.TrySet(variableTarget.Name, assignedValue))
                {
                    _currentExecutionContext.Variables.Declare(variableTarget.Name, assignedValue);
                }
                break;

            case PropertyAccessExpression propertyAccessTarget:
                var parentObject = propertyAccessTarget.Target.Accept(this);
                if (parentObject.IsNull)
                {
                    parentObject = new JyroObject();
                    AssignValueToTarget(propertyAccessTarget.Target, parentObject);
                }
                parentObject.SetProperty(propertyAccessTarget.PropertyName, assignedValue);
                break;

            case IndexAccessExpression indexAccessTarget:
                var targetCollection = indexAccessTarget.Target.Accept(this);
                var indexValue = indexAccessTarget.Index.Accept(this);

                if (targetCollection.IsNull)
                {
                    targetCollection = indexValue is JyroNumber ? new JyroArray() : new JyroObject();
                    AssignValueToTarget(indexAccessTarget.Target, targetCollection);
                }

                targetCollection.SetIndex(indexValue, assignedValue);
                break;

            default:
                AddExecutionError(MessageCode.InvalidAssignmentTarget, assignmentTarget.LineNumber, assignmentTarget.ColumnPosition,
                    "Invalid assignment target");
                break;
        }
    }

    /// <summary>
    /// Resolves the function name from a call target expression.
    /// </summary>
    /// <param name="calleeExpression">The expression representing the function being called.</param>
    /// <returns>The resolved function name or a placeholder for dynamic calls.</returns>
    private static string ResolveFunctionName(IExpression calleeExpression)
    {
        return calleeExpression switch
        {
            VariableExpression variableExpression => variableExpression.Name,
            PropertyAccessExpression propertyAccessExpression => ResolvePropertyChain(propertyAccessExpression),
            _ => "<dynamic>"
        };
    }

    /// <summary>
    /// Resolves a property chain into a dotted function name.
    /// </summary>
    /// <param name="propertyAccessExpression">The property access expression to resolve.</param>
    /// <returns>The complete property chain as a dotted string.</returns>
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

    /// <summary>
    /// Gets the expected value type from a type hint expression.
    /// </summary>
    /// <param name="typeHintExpression">The type hint expression to analyze.</param>
    /// <returns>The corresponding JyroValueType.</returns>
    private static JyroValueType GetExpectedTypeFromHint(IExpression typeHintExpression)
    {
        if (typeHintExpression is TypeExpression typeExpression)
        {
            return MapTokenTypeToValueType(typeExpression.Type);
        }
        return JyroValueType.Null;
    }

    /// <summary>
    /// Checks if a value is compatible with the expected type.
    /// </summary>
    /// <param name="actualValue">The value to check.</param>
    /// <param name="expectedType">The expected type.</param>
    /// <returns>True if compatible, false otherwise.</returns>
    private static bool IsValueCompatibleWithType(JyroValue actualValue, JyroValueType expectedType)
    {
        return actualValue.Type == expectedType || actualValue.IsNull;
    }

    /// <summary>
    /// Maps a token type to the corresponding value type.
    /// </summary>
    /// <param name="tokenType">The token type to map.</param>
    /// <returns>The corresponding JyroValueType.</returns>
    private static JyroValueType MapTokenTypeToValueType(JyroTokenType tokenType)
    {
        return tokenType switch
        {
            JyroTokenType.NumberType => JyroValueType.Number,
            JyroTokenType.StringType => JyroValueType.String,
            JyroTokenType.BooleanType => JyroValueType.Boolean,
            JyroTokenType.ObjectType => JyroValueType.Object,
            JyroTokenType.ArrayType => JyroValueType.Array,
            _ => JyroValueType.Null
        };
    }

    /// <summary>
    /// Checks if two values are equal using Jyro equality semantics.
    /// </summary>
    /// <param name="leftValue">The left value to compare.</param>
    /// <param name="rightValue">The right value to compare.</param>
    /// <returns>True if the values are equal, false otherwise.</returns>
    private static bool AreValuesEqual(JyroValue leftValue, JyroValue rightValue)
    {
        return leftValue.Equals(rightValue);
    }

    /// <summary>
    /// Ensures that the root Data variable exists in the execution context.
    /// </summary>
    /// <param name="executionContext">The execution context to check.</param>
    private static void EnsureRootDataVariableExists(JyroExecutionContext executionContext)
    {
        if (!executionContext.Variables.TryGet(JyroExecutionContext.RootIdentifier, out _))
        {
            executionContext.Variables.Declare(JyroExecutionContext.RootIdentifier, new JyroObject());
        }
    }

    /// <summary>
    /// Adds an error message to the diagnostic collection.
    /// </summary>
    /// <param name="messageCode">The error code.</param>
    /// <param name="lineNumber">The source line number.</param>
    /// <param name="columnPosition">The source column position.</param>
    /// <param name="errorMessage">The error message.</param>
    private void AddExecutionError(MessageCode messageCode, int lineNumber, int columnPosition, string errorMessage)
    {
        _diagnosticMessages!.Add(new Message(messageCode, lineNumber, columnPosition, MessageSeverity.Error, ProcessingStage.Execution, errorMessage));
    }

    #endregion

    #region Flow Control Exceptions

    /// <summary>
    /// Exception used to implement break statement control flow.
    /// </summary>
    internal sealed class BreakControlFlowException : Exception
    {
    }

    /// <summary>
    /// Exception used to implement continue statement control flow.
    /// </summary>
    internal sealed class ContinueControlFlowException : Exception
    {
    }

    /// <summary>
    /// Exception used to implement return statement control flow.
    /// </summary>
    internal sealed class ReturnControlFlowException : Exception
    {
        /// <summary>
        /// Gets the value to return from the function or script.
        /// </summary>
        public JyroValue? ReturnValue { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReturnControlFlowException"/> class.
        /// </summary>
        /// <param name="returnValue">The value to return, or null for void returns.</param>
        public ReturnControlFlowException(JyroValue? returnValue = null) => ReturnValue = returnValue;
    }

    #endregion

    #region Execution Metrics

    /// <summary>
    /// Structure for tracking execution performance metrics.
    /// </summary>
    private struct ExecutionMetrics
    {
        /// <summary>
        /// The total number of statements executed.
        /// </summary>
        public int StatementCount;

        /// <summary>
        /// The total number of loop iterations performed.
        /// </summary>
        public int LoopCount;

        /// <summary>
        /// The total number of function calls made.
        /// </summary>
        public int FunctionCallCount;

        /// <summary>
        /// The current call stack depth.
        /// </summary>
        public int CurrentCallDepth;

        /// <summary>
        /// The maximum call stack depth reached.
        /// </summary>
        public int MaxCallDepth;
    }

    #endregion
}