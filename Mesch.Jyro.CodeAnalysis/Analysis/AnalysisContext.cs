namespace Mesch.Jyro.CodeAnalysis;

/// <summary>
/// Maintains state and accumulates data during code analysis traversal.
/// This context tracks metrics, patterns, and structural information as AST visitors
/// traverse the syntax tree, providing a centralized location for analysis data.
/// </summary>
internal sealed class AnalysisContext
{
    private readonly CodeAnalysisOptions _analysisOptions;
    private readonly Dictionary<string, int> _variableUsageCount = new();
    private readonly Dictionary<string, int> _functionCallFrequency = new();
    private readonly Dictionary<string, int> _operatorUsageCount = new();
    private readonly List<ControlFlowPattern> _controlFlowPatterns = new();
    private readonly Stack<int> _nestingDepthStack = new();
    private readonly List<int> _allNestingDepths = new();
    private readonly Dictionary<string, VariableAccessInfo> _variableAccessTracking = new();

    // Core metrics
    private int _totalStatementCount;
    private int _totalExpressionCount;
    private int _variableDeclarationCount;
    private int _assignmentStatementCount;
    private int _controlFlowStatementCount;
    private int _totalFunctionCallCount;
    private int _maxNestingDepth;
    private int _cyclomaticComplexity = 1; // Base complexity starts at 1
    private int _cognitiveComplexity;
    private int _totalBranchCount;
    private int _currentNestingDepth;
    private int _longestStatementChain;
    private int _currentStatementChain;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisContext"/> class.
    /// </summary>
    /// <param name="analysisOptions">The options controlling analysis behavior.</param>
    /// <exception cref="ArgumentNullException">Thrown when analysisOptions is null.</exception>
    public AnalysisContext(CodeAnalysisOptions analysisOptions)
    {
        _analysisOptions = analysisOptions ?? throw new ArgumentNullException(nameof(analysisOptions));
        _nestingDepthStack.Push(0);
    }

    /// <summary>
    /// Gets the analysis options controlling behavior.
    /// </summary>
    public CodeAnalysisOptions AnalysisOptions => _analysisOptions;

    #region Metrics Recording Methods

    /// <summary>
    /// Records the occurrence of a statement and updates statement chain tracking.
    /// </summary>
    public void RecordStatement()
    {
        _totalStatementCount++;
        _currentStatementChain++;
        _longestStatementChain = Math.Max(_longestStatementChain, _currentStatementChain);
    }

    /// <summary>
    /// Records the occurrence of an expression.
    /// </summary>
    public void RecordExpression()
    {
        _totalExpressionCount++;
    }

    /// <summary>
    /// Records a variable declaration and initializes tracking for the variable.
    /// </summary>
    /// <param name="variableName">The name of the declared variable.</param>
    public void RecordVariableDeclaration(string variableName)
    {
        _variableDeclarationCount++;

        if (!_variableAccessTracking.TryGetValue(variableName, out var variableAccessInfo))
        {
            variableAccessInfo = new VariableAccessInfo();
            _variableAccessTracking[variableName] = variableAccessInfo;
        }

        variableAccessInfo.IsDeclared = true;
    }

    /// <summary>
    /// Records a variable usage and updates usage frequency tracking.
    /// </summary>
    /// <param name="variableName">The name of the variable being used.</param>
    public void RecordVariableUsage(string variableName)
    {
        _variableUsageCount[variableName] = _variableUsageCount.GetValueOrDefault(variableName) + 1;

        if (!_variableAccessTracking.TryGetValue(variableName, out var variableAccessInfo))
        {
            variableAccessInfo = new VariableAccessInfo();
            _variableAccessTracking[variableName] = variableAccessInfo;
        }

        variableAccessInfo.ReadAccessCount++;
    }

    /// <summary>
    /// Records a variable assignment and updates mutation tracking.
    /// </summary>
    /// <param name="variableName">The name of the variable being assigned.</param>
    public void RecordVariableAssignment(string variableName)
    {
        if (!_variableAccessTracking.TryGetValue(variableName, out var variableAccessInfo))
        {
            variableAccessInfo = new VariableAccessInfo();
            _variableAccessTracking[variableName] = variableAccessInfo;
        }

        variableAccessInfo.WriteAccessCount++;
    }

    /// <summary>
    /// Records an assignment statement occurrence.
    /// </summary>
    public void RecordAssignment()
    {
        _assignmentStatementCount++;
    }

    /// <summary>
    /// Records a control flow statement and updates complexity metrics.
    /// </summary>
    public void RecordControlFlow()
    {
        _controlFlowStatementCount++;
        _cyclomaticComplexity++;
        _currentStatementChain = 0; // Reset statement chain at control flow boundaries
    }

    /// <summary>
    /// Records a branch occurrence and updates complexity metrics.
    /// </summary>
    public void RecordBranch()
    {
        _totalBranchCount++;
        _cyclomaticComplexity++;
        _cognitiveComplexity += _currentNestingDepth; // Cognitive complexity increases with nesting level
    }

    /// <summary>
    /// Records a function call and updates call frequency tracking.
    /// </summary>
    /// <param name="functionName">The name of the function being called.</param>
    public void RecordFunctionCall(string functionName)
    {
        _totalFunctionCallCount++;
        _functionCallFrequency[functionName] = _functionCallFrequency.GetValueOrDefault(functionName) + 1;
    }

    /// <summary>
    /// Records operator usage for frequency analysis.
    /// </summary>
    /// <param name="operatorTokenType">The type of operator being used.</param>
    public void RecordOperatorUsage(JyroTokenType operatorTokenType)
    {
        var operatorName = operatorTokenType.ToString();
        _operatorUsageCount[operatorName] = _operatorUsageCount.GetValueOrDefault(operatorName) + 1;
    }

    /// <summary>
    /// Enters a new scope and updates nesting depth tracking.
    /// </summary>
    public void EnterScope()
    {
        _currentNestingDepth++;
        _maxNestingDepth = Math.Max(_maxNestingDepth, _currentNestingDepth);
        _allNestingDepths.Add(_currentNestingDepth);
        _nestingDepthStack.Push(_currentNestingDepth);
        _cognitiveComplexity += _currentNestingDepth; // Nested scopes increase cognitive load
    }

    /// <summary>
    /// Exits the current scope and updates nesting depth tracking.
    /// </summary>
    public void ExitScope()
    {
        if (_nestingDepthStack.Count > 1)
        {
            _nestingDepthStack.Pop();
            _currentNestingDepth--;
        }
    }

    /// <summary>
    /// Records a control flow pattern for frequency analysis.
    /// </summary>
    /// <param name="patternType">The type of control flow pattern.</param>
    /// <param name="patternDescription">A description of the pattern's purpose.</param>
    public void RecordControlFlowPattern(string patternType, string patternDescription)
    {
        var existingPatternIndex = _controlFlowPatterns.FindIndex(pattern => pattern.PatternType == patternType);

        if (existingPatternIndex >= 0)
        {
            var existingPattern = _controlFlowPatterns[existingPatternIndex];
            _controlFlowPatterns[existingPatternIndex] = existingPattern with
            {
                Frequency = existingPattern.Frequency + 1
            };
        }
        else
        {
            _controlFlowPatterns.Add(new ControlFlowPattern(patternType, 1, patternDescription));
        }
    }

    #endregion

    #region Result Generation Methods

    /// <summary>
    /// Generates comprehensive code metrics from collected data.
    /// </summary>
    /// <returns>A <see cref="CodeMetrics"/> object containing all calculated metrics.</returns>
    public CodeMetrics GetMetrics()
    {
        var averageNestingDepth = CalculateAverageNestingDepth();

        return new CodeMetrics(
    totalStatements: _totalStatementCount,
    totalExpressions: _totalExpressionCount,
    variableDeclarations: _variableDeclarationCount,
    assignmentStatements: _assignmentStatementCount,
    controlFlowStatements: _controlFlowStatementCount,
    functionCalls: _totalFunctionCallCount,
    maxNestingDepth: _maxNestingDepth,
    averageNestingDepth: averageNestingDepth,
    cyclomaticComplexity: _cyclomaticComplexity,
    cognitiveComplexity: _cognitiveComplexity,
    totalBranches: _totalBranchCount,
    uniqueVariableNames: _variableUsageCount.Count,
    longestStatementChain: _longestStatementChain);
    }

    #endregion

    #region Private Calculation Methods

    /// <summary>
    /// Calculates the average nesting depth from recorded nesting data.
    /// </summary>
    /// <returns>The average nesting depth as an integer.</returns>
    private int CalculateAverageNestingDepth()
    {
        return _allNestingDepths.Count > 0
            ? (int)Math.Round(_allNestingDepths.Average())
            : 0;
    }

    /// <summary>
    /// Extracts the most frequently used variable names.
    /// </summary>
    /// <returns>A list of variable names ordered by usage frequency.</returns>
    private List<string> ExtractMostUsedVariableNames()
    {
        const int maxVariablesToReturn = 10;

        return _variableUsageCount
            .OrderByDescending(variableUsage => variableUsage.Value)
            .Take(maxVariablesToReturn)
            .Select(variableUsage => variableUsage.Key)
            .ToList();
    }

    /// <summary>
    /// Extracts the most frequently called function names.
    /// </summary>
    /// <returns>A list of function names ordered by call frequency.</returns>
    private List<string> ExtractMostCalledFunctionNames()
    {
        const int maxFunctionsToReturn = 10;

        return _functionCallFrequency
            .OrderByDescending(functionCall => functionCall.Value)
            .Take(maxFunctionsToReturn)
            .Select(functionCall => functionCall.Key)
            .ToList();
    }

    /// <summary>
    /// Calculates data flow characteristics from variable access patterns.
    /// </summary>
    /// <returns>A <see cref="DataFlowCharacteristics"/> object with calculated metrics.</returns>
    private DataFlowCharacteristics CalculateDataFlowCharacteristics()
    {
        var readOnlyVariableCount = _variableAccessTracking.Values
            .Count(variableAccess => variableAccess.ReadAccessCount > 0 && variableAccess.WriteAccessCount == 0);

        var writeOnlyVariableCount = _variableAccessTracking.Values
            .Count(variableAccess => variableAccess.ReadAccessCount == 0 && variableAccess.WriteAccessCount > 0);

        var readWriteVariableCount = _variableAccessTracking.Values
            .Count(variableAccess => variableAccess.ReadAccessCount > 0 && variableAccess.WriteAccessCount > 0);

        var variableReuseRatio = CalculateVariableReuseRatio();

        return new DataFlowCharacteristics(
            readOnlyVariables: readOnlyVariableCount,
            writeOnlyVariables: writeOnlyVariableCount,
            readWriteVariables: readWriteVariableCount,
            variableReuseRatio: variableReuseRatio);
    }

    /// <summary>
    /// Calculates the ratio of variables that are reused multiple times.
    /// </summary>
    /// <returns>A ratio between 0.0 and 1.0 representing variable reuse.</returns>
    private double CalculateVariableReuseRatio()
    {
        var totalVariableCount = _variableAccessTracking.Count;

        if (totalVariableCount == 0)
        {
            return 0.0;
        }

        var multipleUsageVariableCount = _variableUsageCount.Values
            .Count(usageCount => usageCount > 1);

        return (double)multipleUsageVariableCount / totalVariableCount;
    }

    #endregion

    #region Nested Types

    /// <summary>
    /// Tracks access information for individual variables during analysis.
    /// </summary>
    private sealed class VariableAccessInfo
    {
        /// <summary>
        /// Gets or sets a value indicating whether the variable has been explicitly declared.
        /// </summary>
        public bool IsDeclared { get; set; }

        /// <summary>
        /// Gets or sets the number of times the variable has been read.
        /// </summary>
        public int ReadAccessCount { get; set; }

        /// <summary>
        /// Gets or sets the number of times the variable has been written to.
        /// </summary>
        public int WriteAccessCount { get; set; }
    }

    #endregion
}