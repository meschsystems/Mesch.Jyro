using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace Mesch.Jyro.Runtime.Tests.Parser;

public class ParserTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mesch.Jyro.Lexer _lexer;
    private readonly Mesch.Jyro.Parser _parser;

    public ParserTests(ITestOutputHelper output)
    {
        _output = output;
        _lexer = new Mesch.Jyro.Lexer(NullLogger<Mesch.Jyro.Lexer>.Instance);
        _parser = new Mesch.Jyro.Parser(NullLogger<Mesch.Jyro.Parser>.Instance);
    }

    #region Helper Methods

    private JyroParsingResult ParseSource(string source)
    {
        var lexingResult = _lexer.Tokenize(source);
        if (!lexingResult.IsSuccessful)
        {
            _output.WriteLine($"Lexing failed: {string.Join(", ", lexingResult.Messages)}");
            return new JyroParsingResult(false, [], lexingResult.Messages,
                new ParsingMetadata(TimeSpan.Zero, 0, 0, DateTimeOffset.UtcNow));
        }
        return _parser.Parse(lexingResult.Tokens);
    }

    private void LogParsingResult(JyroParsingResult result)
    {
        _output.WriteLine($"Success: {result.IsSuccessful}, Statements: {result.Statements.Count}, Errors: {result.ErrorCount}");
        foreach (var statement in result.Statements)
        {
            _output.WriteLine($"  {statement.GetType().Name} at {statement.LineNumber}:{statement.ColumnPosition}");
        }
        foreach (var message in result.Messages)
        {
            _output.WriteLine($"  {message.Severity}: {message.Code} - {message}");
        }
    }

    #endregion

    #region Basic Parsing

    [Fact]
    public void Parse_EmptyInput_ReturnsSuccessWithNoStatements()
    {
        var result = ParseSource("");

        Assert.True(result.IsSuccessful);
        Assert.Empty(result.Statements);
        Assert.Equal(0, result.ErrorCount);
        Assert.NotNull(result.Metadata);
    }

    [Fact]
    public void Parse_WhitespaceOnly_ReturnsSuccessWithNoStatements()
    {
        var result = ParseSource("   \t\n  ");

        Assert.True(result.IsSuccessful);
        Assert.Empty(result.Statements);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Parse_CommentsOnly_ReturnsSuccessWithNoStatements()
    {
        var result = ParseSource("# This is a comment\n# Another comment");

        Assert.True(result.IsSuccessful);
        Assert.Empty(result.Statements);
        Assert.Equal(0, result.ErrorCount);
    }

    #endregion

    #region Variable Declarations

    [Fact]
    public void Parse_SimpleVariableDeclaration_ParsedCorrectly()
    {
        var result = ParseSource("var x");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var varDecl = Assert.IsType<VariableDeclarationStatement>(result.Statements[0]);
        Assert.Equal("x", varDecl.Name);
        Assert.Null(varDecl.TypeHint);
        Assert.Null(varDecl.Initializer);
    }

    [Fact]
    public void Parse_VariableDeclarationWithTypeHint_ParsedCorrectly()
    {
        var result = ParseSource("var x: number");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var varDecl = Assert.IsType<VariableDeclarationStatement>(result.Statements[0]);
        Assert.Equal("x", varDecl.Name);
        Assert.NotNull(varDecl.TypeHint);
        Assert.IsType<TypeExpression>(varDecl.TypeHint);
        Assert.Null(varDecl.Initializer);
    }

    [Fact]
    public void Parse_VariableDeclarationWithInitializer_ParsedCorrectly()
    {
        var result = ParseSource("var x = 42");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var varDecl = Assert.IsType<VariableDeclarationStatement>(result.Statements[0]);
        Assert.Equal("x", varDecl.Name);
        Assert.Null(varDecl.TypeHint);
        Assert.NotNull(varDecl.Initializer);
        Assert.IsType<LiteralExpression>(varDecl.Initializer);
    }

    [Fact]
    public void Parse_VariableDeclarationWithTypeAndInitializer_ParsedCorrectly()
    {
        var result = ParseSource("var x: number = 42");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var varDecl = Assert.IsType<VariableDeclarationStatement>(result.Statements[0]);
        Assert.Equal("x", varDecl.Name);
        Assert.NotNull(varDecl.TypeHint);
        Assert.NotNull(varDecl.Initializer);
    }

    [Theory]
    [InlineData("number")]
    [InlineData("string")]
    [InlineData("boolean")]
    [InlineData("object")]
    [InlineData("array")]
    public void Parse_VariableDeclarationWithAllTypes_ParsedCorrectly(string typeName)
    {
        var result = ParseSource($"var x: {typeName}");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var varDecl = Assert.IsType<VariableDeclarationStatement>(result.Statements[0]);
        Assert.Equal("x", varDecl.Name);
        Assert.NotNull(varDecl.TypeHint);
    }

    #endregion

    #region Assignment Statements

    [Fact]
    public void Parse_SimpleAssignment_ParsedCorrectly()
    {
        var result = ParseSource("x = 42");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var assignment = Assert.IsType<AssignmentStatement>(result.Statements[0]);
        Assert.IsType<VariableExpression>(assignment.Target);
        Assert.IsType<LiteralExpression>(assignment.Value);
    }

    [Fact]
    public void Parse_PropertyAssignment_ParsedCorrectly()
    {
        var result = ParseSource("Data.name = \"John\"");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var assignment = Assert.IsType<AssignmentStatement>(result.Statements[0]);
        Assert.IsType<PropertyAccessExpression>(assignment.Target);
        Assert.IsType<LiteralExpression>(assignment.Value);
    }

    [Fact]
    public void Parse_IndexAssignment_ParsedCorrectly()
    {
        var result = ParseSource("Data[0] = \"value\"");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var assignment = Assert.IsType<AssignmentStatement>(result.Statements[0]);
        Assert.IsType<IndexAccessExpression>(assignment.Target);
        Assert.IsType<LiteralExpression>(assignment.Value);
    }

    #endregion

    #region Expression Statements

    [Fact]
    public void Parse_LiteralExpression_ParsedCorrectly()
    {
        var result = ParseSource("42");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var literal = Assert.IsType<LiteralExpression>(exprStmt.Expression);
        Assert.Equal(42.0, literal.Value);
    }

    [Fact]
    public void Parse_StringLiteral_ParsedCorrectly()
    {
        var result = ParseSource("\"hello world\"");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var literal = Assert.IsType<LiteralExpression>(exprStmt.Expression);
        Assert.Equal("hello world", literal.Value);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void Parse_BooleanLiterals_ParsedCorrectly(string input, bool expectedValue)
    {
        var result = ParseSource(input);

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var literal = Assert.IsType<LiteralExpression>(exprStmt.Expression);
        Assert.Equal(expectedValue, literal.Value);
    }

    [Fact]
    public void Parse_NullLiteral_ParsedCorrectly()
    {
        var result = ParseSource("null");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var literal = Assert.IsType<LiteralExpression>(exprStmt.Expression);
        Assert.Null(literal.Value);
    }

    #endregion

    #region Binary Expressions

    [Theory]
    [InlineData("1 + 2", JyroTokenType.Plus)]
    [InlineData("1 - 2", JyroTokenType.Minus)]
    [InlineData("1 * 2", JyroTokenType.Star)]
    [InlineData("1 / 2", JyroTokenType.Slash)]
    [InlineData("1 % 2", JyroTokenType.Percent)]
    public void Parse_ArithmeticExpressions_ParsedCorrectly(string input, JyroTokenType expectedOperator)
    {
        var result = ParseSource(input);

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var binary = Assert.IsType<BinaryExpression>(exprStmt.Expression);
        Assert.Equal(expectedOperator, binary.Operator);
        Assert.IsType<LiteralExpression>(binary.Left);
        Assert.IsType<LiteralExpression>(binary.Right);
    }

    [Theory]
    [InlineData("1 == 2", JyroTokenType.EqualEqual)]
    [InlineData("1 != 2", JyroTokenType.BangEqual)]
    [InlineData("1 < 2", JyroTokenType.Less)]
    [InlineData("1 <= 2", JyroTokenType.LessEqual)]
    [InlineData("1 > 2", JyroTokenType.Greater)]
    [InlineData("1 >= 2", JyroTokenType.GreaterEqual)]
    public void Parse_ComparisonExpressions_ParsedCorrectly(string input, JyroTokenType expectedOperator)
    {
        var result = ParseSource(input);

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var binary = Assert.IsType<BinaryExpression>(exprStmt.Expression);
        Assert.Equal(expectedOperator, binary.Operator);
    }

    [Theory]
    [InlineData("true and false", JyroTokenType.And)]
    [InlineData("true or false", JyroTokenType.Or)]
    public void Parse_LogicalExpressions_ParsedCorrectly(string input, JyroTokenType expectedOperator)
    {
        var result = ParseSource(input);

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var binary = Assert.IsType<BinaryExpression>(exprStmt.Expression);
        Assert.Equal(expectedOperator, binary.Operator);
    }

    [Fact]
    public void Parse_OperatorPrecedence_RespectedCorrectly()
    {
        var result = ParseSource("1 + 2 * 3");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var outerBinary = Assert.IsType<BinaryExpression>(exprStmt.Expression);

        // Should be parsed as 1 + (2 * 3)
        Assert.Equal(JyroTokenType.Plus, outerBinary.Operator);
        Assert.IsType<LiteralExpression>(outerBinary.Left);
        Assert.IsType<BinaryExpression>(outerBinary.Right);

        var rightBinary = Assert.IsType<BinaryExpression>(outerBinary.Right);
        Assert.Equal(JyroTokenType.Star, rightBinary.Operator);
    }

    #endregion

    #region Unary Expressions

    [Theory]
    [InlineData("-42", JyroTokenType.Minus)]
    [InlineData("not true", JyroTokenType.Not)]
    public void Parse_UnaryExpressions_ParsedCorrectly(string input, JyroTokenType expectedOperator)
    {
        var result = ParseSource(input);

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var unary = Assert.IsType<UnaryExpression>(exprStmt.Expression);
        Assert.Equal(expectedOperator, unary.Operator);
        Assert.IsType<LiteralExpression>(unary.Operand);
    }

    #endregion

    #region Ternary Operator Tests

    [Fact]
    public void Parse_SimpleTernaryExpression_ParsedCorrectly()
    {
        var result = ParseSource("true ? 1 : 2");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var ternary = Assert.IsType<TernaryExpression>(exprStmt.Expression);

        Assert.IsType<LiteralExpression>(ternary.Condition);
        Assert.IsType<LiteralExpression>(ternary.TrueExpression);
        Assert.IsType<LiteralExpression>(ternary.FalseExpression);
    }

    [Fact]
    public void Parse_TernaryWithVariables_ParsedCorrectly()
    {
        var result = ParseSource("x > 0 ? positiveValue : negativeValue");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var ternary = Assert.IsType<TernaryExpression>(exprStmt.Expression);

        Assert.IsType<BinaryExpression>(ternary.Condition);
        Assert.IsType<VariableExpression>(ternary.TrueExpression);
        Assert.IsType<VariableExpression>(ternary.FalseExpression);
    }

    [Fact]
    public void Parse_TernaryWithFunctionCalls_ParsedCorrectly()
    {
        var result = ParseSource("isValid ? GetSuccessMessage() : GetErrorMessage()");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var ternary = Assert.IsType<TernaryExpression>(exprStmt.Expression);

        Assert.IsType<VariableExpression>(ternary.Condition);
        Assert.IsType<FunctionCallExpression>(ternary.TrueExpression);
        Assert.IsType<FunctionCallExpression>(ternary.FalseExpression);
    }

    [Fact]
    public void Parse_NestedTernaryRightAssociative_ParsedCorrectly()
    {
        var result = ParseSource("a ? b : c ? d : e");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var outerTernary = Assert.IsType<TernaryExpression>(exprStmt.Expression);

        // Should parse as: a ? b : (c ? d : e)
        Assert.IsType<VariableExpression>(outerTernary.Condition); // a
        Assert.IsType<VariableExpression>(outerTernary.TrueExpression); // b

        var innerTernary = Assert.IsType<TernaryExpression>(outerTernary.FalseExpression);
        Assert.IsType<VariableExpression>(innerTernary.Condition); // c
        Assert.IsType<VariableExpression>(innerTernary.TrueExpression); // d
        Assert.IsType<VariableExpression>(innerTernary.FalseExpression); // e
    }

    [Fact]
    public void Parse_TernaryWithComplexCondition_ParsedCorrectly()
    {
        var result = ParseSource("user.isActive and user.hasPermission ? \"granted\" : \"denied\"");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var ternary = Assert.IsType<TernaryExpression>(exprStmt.Expression);

        var condition = Assert.IsType<BinaryExpression>(ternary.Condition);
        Assert.Equal(JyroTokenType.And, condition.Operator);
        Assert.IsType<LiteralExpression>(ternary.TrueExpression);
        Assert.IsType<LiteralExpression>(ternary.FalseExpression);
    }

    [Fact]
    public void Parse_TernaryInAssignment_ParsedCorrectly()
    {
        var result = ParseSource("status = isActive ? \"active\" : \"inactive\"");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var assignment = Assert.IsType<AssignmentStatement>(result.Statements[0]);
        Assert.IsType<VariableExpression>(assignment.Target);

        var ternary = Assert.IsType<TernaryExpression>(assignment.Value);
        Assert.IsType<VariableExpression>(ternary.Condition);
        Assert.IsType<LiteralExpression>(ternary.TrueExpression);
        Assert.IsType<LiteralExpression>(ternary.FalseExpression);
    }

    [Fact]
    public void Parse_TernaryPrecedenceLowerThanLogicalOr_ParsedCorrectly()
    {
        var result = ParseSource("a or b ? c : d");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var ternary = Assert.IsType<TernaryExpression>(exprStmt.Expression);

        // Should parse as: (a or b) ? c : d
        var condition = Assert.IsType<BinaryExpression>(ternary.Condition);
        Assert.Equal(JyroTokenType.Or, condition.Operator);
        Assert.IsType<VariableExpression>(ternary.TrueExpression);
        Assert.IsType<VariableExpression>(ternary.FalseExpression);
    }

    [Fact]
    public void Parse_TernaryWithParentheses_ParsedCorrectly()
    {
        var result = ParseSource("(a ? b : c) + d");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var binary = Assert.IsType<BinaryExpression>(exprStmt.Expression);

        // Should parse as: (a ? b : c) + d
        Assert.Equal(JyroTokenType.Plus, binary.Operator);
        Assert.IsType<TernaryExpression>(binary.Left);
        Assert.IsType<VariableExpression>(binary.Right);
    }

    [Fact]
    public void Parse_TernaryWithPropertyAccess_ParsedCorrectly()
    {
        var result = ParseSource("Data.isValid ? Data.successValue : Data.errorValue");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var ternary = Assert.IsType<TernaryExpression>(exprStmt.Expression);

        Assert.IsType<PropertyAccessExpression>(ternary.Condition);
        Assert.IsType<PropertyAccessExpression>(ternary.TrueExpression);
        Assert.IsType<PropertyAccessExpression>(ternary.FalseExpression);
    }

    [Fact]
    public void Parse_TernaryMissingTrueExpression_ReturnsError()
    {
        var result = ParseSource("condition ? : false");

        Assert.False(result.IsSuccessful);
        Assert.True(result.ErrorCount > 0);
        Assert.Contains(result.Messages, msg => msg.Code == MessageCode.UnexpectedToken);
    }

    [Fact]
    public void Parse_TernaryMissingColon_ReturnsError()
    {
        var result = ParseSource("condition ? true false");

        Assert.False(result.IsSuccessful);
        Assert.True(result.ErrorCount > 0);
        Assert.Contains(result.Messages, msg => msg.Code == MessageCode.MissingToken);
    }

    [Fact]
    public void Parse_TernaryMissingFalseExpression_ReturnsError()
    {
        var result = ParseSource("condition ? true :");

        Assert.False(result.IsSuccessful);
        Assert.True(result.ErrorCount > 0);
        Assert.Contains(result.Messages, msg => msg.Code == MessageCode.UnexpectedToken);
    }

    [Fact]
    public void Parse_TernaryMissingQuestionMarkWithColon_ReturnsError()
    {
        var result = ParseSource("condition true : false");

        Assert.False(result.IsSuccessful);
        Assert.True(result.ErrorCount > 0);
        Assert.Contains(result.Messages, msg => msg.Code == MessageCode.UnexpectedToken);
    }

    [Fact]
    public void Parse_TernaryMissingQuestionMarkWithoutColon_ParsesAsVariable()
    {
        var result = ParseSource("condition true false");

        // This should parse as separate expression statements, not a ternary
        Assert.True(result.IsSuccessful);
        Assert.Equal(3, result.Statements.Count);

        Assert.IsType<ExpressionStatement>(result.Statements[0]);
        Assert.IsType<ExpressionStatement>(result.Statements[1]);
        Assert.IsType<ExpressionStatement>(result.Statements[2]);
    }

    [Theory]
    [InlineData("score >= 90 ? \"A\" : score >= 80 ? \"B\" : \"F\"")]
    [InlineData("x ? y ? z : w : v")]
    [InlineData("a ? b : c ? d : e ? f : g")]
    public void Parse_ChainedTernaryExpressions_ParsedCorrectly(string input)
    {
        var result = ParseSource(input);

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        Assert.IsType<TernaryExpression>(exprStmt.Expression);
    }

    [Fact]
    public void Parse_TernaryInVariableDeclaration_ParsedCorrectly()
    {
        var result = ParseSource("var result = condition ? \"yes\" : \"no\"");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var varDecl = Assert.IsType<VariableDeclarationStatement>(result.Statements[0]);
        Assert.Equal("result", varDecl.Name);

        var ternary = Assert.IsType<TernaryExpression>(varDecl.Initializer);
        Assert.IsType<VariableExpression>(ternary.Condition);
        Assert.IsType<LiteralExpression>(ternary.TrueExpression);
        Assert.IsType<LiteralExpression>(ternary.FalseExpression);
    }

    #endregion

    #region Property and Index Access

    [Fact]
    public void Parse_PropertyAccess_ParsedCorrectly()
    {
        var result = ParseSource("Data.name");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var propAccess = Assert.IsType<PropertyAccessExpression>(exprStmt.Expression);

        var variable = Assert.IsType<VariableExpression>(propAccess.Target);
        Assert.Equal("Data", variable.Name);
        Assert.Equal("name", propAccess.PropertyName);
    }

    [Fact]
    public void Parse_ChainedPropertyAccess_ParsedCorrectly()
    {
        var result = ParseSource("Data.user.name");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var outerProp = Assert.IsType<PropertyAccessExpression>(exprStmt.Expression);
        Assert.Equal("name", outerProp.PropertyName);

        var innerProp = Assert.IsType<PropertyAccessExpression>(outerProp.Target);
        Assert.Equal("user", innerProp.PropertyName);

        var variable = Assert.IsType<VariableExpression>(innerProp.Target);
        Assert.Equal("Data", variable.Name);
    }

    [Fact]
    public void Parse_IndexAccess_ParsedCorrectly()
    {
        var result = ParseSource("Data[0]");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var indexAccess = Assert.IsType<IndexAccessExpression>(exprStmt.Expression);

        var variable = Assert.IsType<VariableExpression>(indexAccess.Target);
        Assert.Equal("Data", variable.Name);
        Assert.IsType<LiteralExpression>(indexAccess.Index);
    }

    [Fact]
    public void Parse_ChainedIndexAccess_ParsedCorrectly()
    {
        var result = ParseSource("Data[0][1]");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var outerIndex = Assert.IsType<IndexAccessExpression>(exprStmt.Expression);
        Assert.IsType<LiteralExpression>(outerIndex.Index);

        var innerIndex = Assert.IsType<IndexAccessExpression>(outerIndex.Target);
        Assert.IsType<LiteralExpression>(innerIndex.Index);

        var variable = Assert.IsType<VariableExpression>(innerIndex.Target);
        Assert.Equal("Data", variable.Name);
    }

    #endregion

    #region Function Calls

    [Fact]
    public void Parse_SimpleFunctionCall_ParsedCorrectly()
    {
        var result = ParseSource("Upper(\"hello\")");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var funcCall = Assert.IsType<FunctionCallExpression>(exprStmt.Expression);

        var target = Assert.IsType<VariableExpression>(funcCall.Target);
        Assert.Equal("Upper", target.Name);
        Assert.Single(funcCall.Arguments);
        Assert.IsType<LiteralExpression>(funcCall.Arguments[0]);
    }

    [Fact]
    public void Parse_FunctionCallWithNoArguments_ParsedCorrectly()
    {
        var result = ParseSource("Now()");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var funcCall = Assert.IsType<FunctionCallExpression>(exprStmt.Expression);

        var target = Assert.IsType<VariableExpression>(funcCall.Target);
        Assert.Equal("Now", target.Name);
        Assert.Empty(funcCall.Arguments);
    }

    [Fact]
    public void Parse_FunctionCallWithMultipleArguments_ParsedCorrectly()
    {
        var result = ParseSource("Replace(\"hello\", \"l\", \"x\")");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var funcCall = Assert.IsType<FunctionCallExpression>(exprStmt.Expression);

        var target = Assert.IsType<VariableExpression>(funcCall.Target);
        Assert.Equal("Replace", target.Name);
        Assert.Equal(3, funcCall.Arguments.Count);
        Assert.All(funcCall.Arguments, arg => Assert.IsType<LiteralExpression>(arg));
    }

    #endregion

    #region Array and Object Literals

    [Fact]
    public void Parse_EmptyArrayLiteral_ParsedCorrectly()
    {
        var result = ParseSource("[]");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var arrayLiteral = Assert.IsType<ArrayLiteralExpression>(exprStmt.Expression);
        Assert.Empty(arrayLiteral.Elements);
    }

    [Fact]
    public void Parse_ArrayLiteralWithElements_ParsedCorrectly()
    {
        var result = ParseSource("[1, 2, 3]");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var arrayLiteral = Assert.IsType<ArrayLiteralExpression>(exprStmt.Expression);
        Assert.Equal(3, arrayLiteral.Elements.Count);
        Assert.All(arrayLiteral.Elements, elem => Assert.IsType<LiteralExpression>(elem));
    }

    [Fact]
    public void Parse_EmptyObjectLiteral_ParsedCorrectly()
    {
        var result = ParseSource("{}");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var objectLiteral = Assert.IsType<ObjectLiteralExpression>(exprStmt.Expression);
        Assert.Empty(objectLiteral.Properties);
    }

    [Fact]
    public void Parse_ObjectLiteralWithProperties_ParsedCorrectly()
    {
        var result = ParseSource("{\"name\": \"John\", \"age\": 30}");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var objectLiteral = Assert.IsType<ObjectLiteralExpression>(exprStmt.Expression);
        Assert.Equal(2, objectLiteral.Properties.Count);

        Assert.Equal("name", objectLiteral.Properties[0].Key);
        Assert.Equal("age", objectLiteral.Properties[1].Key);
        Assert.All(objectLiteral.Properties, prop => Assert.IsType<LiteralExpression>(prop.Value));
    }

    #endregion

    #region Control Flow Statements

    [Fact]
    public void Parse_SimpleIfStatement_ParsedCorrectly()
    {
        var result = ParseSource("if true then x = 1 end");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var ifStmt = Assert.IsType<IfStatement>(result.Statements[0]);
        Assert.IsType<LiteralExpression>(ifStmt.Condition);
        Assert.Single(ifStmt.ThenBranch);
        Assert.Empty(ifStmt.ElseBranch);
        Assert.Empty(ifStmt.ElseIfClauses);
    }

    [Fact]
    public void Parse_IfElseStatement_ParsedCorrectly()
    {
        var result = ParseSource("if true then x = 1 else x = 2 end");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var ifStmt = Assert.IsType<IfStatement>(result.Statements[0]);
        Assert.IsType<LiteralExpression>(ifStmt.Condition);
        Assert.Single(ifStmt.ThenBranch);
        Assert.Single(ifStmt.ElseBranch);
        Assert.Empty(ifStmt.ElseIfClauses);
    }

    [Fact]
    public void Parse_IfElseIfElseStatement_ParsedCorrectly()
    {
        var result = ParseSource("if x == 1 then y = 1 else if x == 2 then y = 2 else y = 3 end");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var ifStmt = Assert.IsType<IfStatement>(result.Statements[0]);
        Assert.IsType<BinaryExpression>(ifStmt.Condition);
        Assert.Single(ifStmt.ThenBranch);
        Assert.Single(ifStmt.ElseBranch);
        Assert.Single(ifStmt.ElseIfClauses);

        var elseIfClause = ifStmt.ElseIfClauses[0];
        Assert.IsType<BinaryExpression>(elseIfClause.Condition);
        Assert.Single(elseIfClause.Statements);
    }

    [Fact]
    public void Parse_WhileStatement_ParsedCorrectly()
    {
        var result = ParseSource("while x < 10 do x = x + 1 end");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var whileStmt = Assert.IsType<WhileStatement>(result.Statements[0]);
        Assert.IsType<BinaryExpression>(whileStmt.Condition);
        Assert.Single(whileStmt.Body);
        Assert.IsType<AssignmentStatement>(whileStmt.Body[0]);
    }

    [Fact]
    public void Parse_ForEachStatement_ParsedCorrectly()
    {
        var result = ParseSource("foreach item in Data.items do item.processed = true end");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var forEachStmt = Assert.IsType<ForEachStatement>(result.Statements[0]);
        Assert.Equal("item", forEachStmt.IteratorName);
        Assert.IsType<PropertyAccessExpression>(forEachStmt.Source);
        Assert.Single(forEachStmt.Body);
        Assert.IsType<AssignmentStatement>(forEachStmt.Body[0]);
    }

    [Fact]
    public void Parse_SwitchStatement_ParsedCorrectly()
    {
        var result = ParseSource("switch x case 1 then y = 1 case 2 then y = 2 default y = 0 end");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var switchStmt = Assert.IsType<SwitchStatement>(result.Statements[0]);
        Assert.IsType<VariableExpression>(switchStmt.Expression);
        Assert.Equal(2, switchStmt.Cases.Count);
        Assert.Single(switchStmt.DefaultBranch);

        Assert.IsType<LiteralExpression>(switchStmt.Cases[0].MatchExpression);
        Assert.Single(switchStmt.Cases[0].Body);
    }

    [Fact]
    public void Parse_BreakStatement_ParsedCorrectly()
    {
        var result = ParseSource("break");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);
        Assert.IsType<BreakStatement>(result.Statements[0]);
    }

    [Fact]
    public void Parse_ContinueStatement_ParsedCorrectly()
    {
        var result = ParseSource("continue");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);
        Assert.IsType<ContinueStatement>(result.Statements[0]);
    }

    [Fact]
    public void Parse_ReturnStatement_ParsedCorrectly()
    {
        var result = ParseSource("return");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);
        var returnStmt = Assert.IsType<ReturnStatement>(result.Statements[0]);
        Assert.Null(returnStmt.Value);
    }

    #endregion

    #region Type Checking Expressions

    [Theory]
    [InlineData("x is number")]
    [InlineData("x is string")]
    [InlineData("x is boolean")]
    [InlineData("x is object")]
    [InlineData("x is array")]
    public void Parse_TypeCheckExpressions_ParsedCorrectly(string input)
    {
        var result = ParseSource(input);

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var typeCheck = Assert.IsType<TypeCheckExpression>(exprStmt.Expression);
        Assert.IsType<VariableExpression>(typeCheck.Target);
    }

    #endregion

    #region Parenthesized Expressions

    [Fact]
    public void Parse_ParenthesizedExpression_ParsedCorrectly()
    {
        var result = ParseSource("(1 + 2) * 3");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var binary = Assert.IsType<BinaryExpression>(exprStmt.Expression);

        // Should be parsed as (1 + 2) * 3
        Assert.Equal(JyroTokenType.Star, binary.Operator);
        Assert.IsType<BinaryExpression>(binary.Left);
        Assert.IsType<LiteralExpression>(binary.Right);
    }

    #endregion

    #region Multiple Statements

    [Fact]
    public void Parse_MultipleStatements_ParsedCorrectly()
    {
        var result = ParseSource("var x = 1\nvar y = 2\nx = x + y");

        Assert.True(result.IsSuccessful);
        Assert.Equal(3, result.Statements.Count);

        Assert.IsType<VariableDeclarationStatement>(result.Statements[0]);
        Assert.IsType<VariableDeclarationStatement>(result.Statements[1]);
        Assert.IsType<AssignmentStatement>(result.Statements[2]);
    }

    [Fact]
    public void Parse_StatementsWithComments_ParsedCorrectly()
    {
        var result = ParseSource("# Comment\nvar x = 1 # Another comment\n# Final comment");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);
        Assert.IsType<VariableDeclarationStatement>(result.Statements[0]);
    }

    #endregion

    #region Complex Expressions

    [Fact]
    public void Parse_ComplexExpression_ParsedCorrectly()
    {
        var result = ParseSource("Data.users[0].name == \"John\" and Data.users[0].age > 18");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var andExpr = Assert.IsType<BinaryExpression>(exprStmt.Expression);
        Assert.Equal(JyroTokenType.And, andExpr.Operator);

        Assert.IsType<BinaryExpression>(andExpr.Left);
        Assert.IsType<BinaryExpression>(andExpr.Right);
    }

    [Fact]
    public void Parse_NestedFunctionCalls_ParsedCorrectly()
    {
        var result = ParseSource("Upper(Lower(\"Hello\"))");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var outerCall = Assert.IsType<FunctionCallExpression>(exprStmt.Expression);

        var outerTarget = Assert.IsType<VariableExpression>(outerCall.Target);
        Assert.Equal("Upper", outerTarget.Name);
        Assert.Single(outerCall.Arguments);

        var innerCall = Assert.IsType<FunctionCallExpression>(outerCall.Arguments[0]);
        var innerTarget = Assert.IsType<VariableExpression>(innerCall.Target);
        Assert.Equal("Lower", innerTarget.Name);
    }

    #endregion

    #region Error Handling

    [Fact]
    public void Parse_UnexpectedToken_ReturnsError()
    {
        var result = ParseSource("var 123");

        Assert.False(result.IsSuccessful);
        Assert.Equal(2, result.ErrorCount); // Actual error count
        Assert.Contains(result.Messages, msg => msg.Code == MessageCode.MissingToken);
        Assert.Equal(MessageSeverity.Error, result.Messages[0].Severity);
    }

    [Fact]
    public void Parse_MissingEndToken_ReturnsError()
    {
        var result = ParseSource("if true then x = 1");

        Assert.False(result.IsSuccessful);
        Assert.Equal(4, result.ErrorCount); // Actual error count
        Assert.Contains(result.Messages, msg => msg.Code == MessageCode.MissingToken);
    }

    [Fact]
    public void Parse_InvalidAssignmentTarget_ReturnsError()
    {
        var result = ParseSource("42 = x");

        Assert.False(result.IsSuccessful);
        Assert.Equal(4, result.ErrorCount); // Actual error count
        Assert.Contains(result.Messages, msg => msg.Code == MessageCode.InvalidAssignmentTarget);
    }

    [Fact]
    public void Parse_ErrorRecovery_ContinuesAfterError()
    {
        var result = ParseSource("var 123\nvar validVar = 42");

        Assert.False(result.IsSuccessful);
        Assert.True(result.ErrorCount > 0);

        // Should still have parsed some valid statements
        Assert.True(result.Statements.Count > 0);
    }

    [Fact]
    public void Parse_UnterminatedExpression_ReturnsError()
    {
        var result = ParseSource("1 +");

        Assert.False(result.IsSuccessful);
        Assert.Equal(4, result.ErrorCount); // Actual error count
    }

    [Fact]
    public void Parse_MismatchedParentheses_ReturnsError()
    {
        var result = ParseSource("(1 + 2");

        Assert.False(result.IsSuccessful);
        Assert.Equal(2, result.ErrorCount); // Actual error count
        Assert.Contains(result.Messages, msg => msg.Code == MessageCode.MissingToken);
    }

    [Fact]
    public void Parse_MismatchedBrackets_ReturnsError()
    {
        var result = ParseSource("[1, 2, 3");

        Assert.False(result.IsSuccessful);
        Assert.Equal(6, result.ErrorCount); // Actual error count
        Assert.Contains(result.Messages, msg => msg.Code == MessageCode.MissingToken);
    }

    [Fact]
    public void Parse_MismatchedBraces_ReturnsError()
    {
        var result = ParseSource("{\"key\": \"value\"");

        Assert.False(result.IsSuccessful);
        Assert.Equal(4, result.ErrorCount); // Actual error count
        Assert.Contains(result.Messages, msg => msg.Code == MessageCode.MissingToken);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Parse_VeryDeeplyNestedExpression_ParsedCorrectly()
    {
        var result = ParseSource("((((1 + 2) * 3) - 4) / 5)");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        Assert.IsType<BinaryExpression>(exprStmt.Expression);
    }

    [Fact]
    public void Parse_LongChainedPropertyAccess_ParsedCorrectly()
    {
        var result = ParseSource("Data.level1.level2.level3.level4.value");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var propAccess = Assert.IsType<PropertyAccessExpression>(exprStmt.Expression);
        Assert.Equal("value", propAccess.PropertyName);
    }

    [Fact]
    public void Parse_MixedAccessChain_ParsedCorrectly()
    {
        var result = ParseSource("Data.users[0].contacts[\"email\"].verified");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Statements);

        var exprStmt = Assert.IsType<ExpressionStatement>(result.Statements[0]);
        var finalProp = Assert.IsType<PropertyAccessExpression>(exprStmt.Expression);
        Assert.Equal("verified", finalProp.PropertyName);

        var emailIndex = Assert.IsType<IndexAccessExpression>(finalProp.Target);
        var contacts = Assert.IsType<PropertyAccessExpression>(emailIndex.Target);
        Assert.Equal("contacts", contacts.PropertyName);
    }

    #endregion

    #region Metadata Validation

    [Fact]
    public void Parse_ValidInput_ReturnsCorrectMetadata()
    {
        var result = ParseSource("var x = 42\nif x > 0 then x = x + 1 end");

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Metadata);
        Assert.Equal(2, result.Metadata.StatementCount);
        Assert.True(result.Metadata.ProcessingTime >= TimeSpan.Zero);
        Assert.True(result.Metadata.StartedAt <= DateTimeOffset.UtcNow);
        Assert.True(result.Metadata.MaxNestingDepth >= 1);
    }

    [Fact]
    public void Parse_InputWithErrors_StillReturnsMetadata()
    {
        var result = ParseSource("var 123 invalid");

        Assert.False(result.IsSuccessful);
        Assert.NotNull(result.Metadata);
        Assert.True(result.Metadata.ProcessingTime >= TimeSpan.Zero);
    }

    #endregion

    #region Debug Tests

    [Fact]
    public void Debug_ParserBehavior()
    {
        _output.WriteLine("=== Simple assignment ===");
        var result1 = ParseSource("x = 42");
        LogParsingResult(result1);

        _output.WriteLine("=== If statement ===");
        var result2 = ParseSource("if true then x = 1 end");
        LogParsingResult(result2);

        _output.WriteLine("=== Complex expression ===");
        var result3 = ParseSource("Data.users[0].name");
        LogParsingResult(result3);
    }

    #endregion
}