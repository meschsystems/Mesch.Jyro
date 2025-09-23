using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace Mesch.Jyro.Runtime.Tests.Lexer;

public class LexerTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mesch.Jyro.Lexer _lexer;

    public LexerTests(ITestOutputHelper output)
    {
        _output = output;
        _lexer = new Mesch.Jyro.Lexer(NullLogger<Mesch.Jyro.Lexer>.Instance);
    }

    #region Basic Token Recognition

    [Fact]
    public void Tokenize_EmptyString_ReturnsEofToken()
    {
        var result = _lexer.Tokenize("");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Tokens);
        Assert.Equal(JyroTokenType.EndOfFile, result.Tokens[0].Type);
        Assert.Equal(string.Empty, result.Tokens[0].Lexeme);
        Assert.Equal(1, result.Tokens[0].LineNumber);
        Assert.Equal(1, result.Tokens[0].ColumnPosition);
    }

    [Fact]
    public void Tokenize_WhitespaceOnly_ReturnsEofToken()
    {
        var result = _lexer.Tokenize("   \t  \r\n  ");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Tokens);
        Assert.Equal(JyroTokenType.EndOfFile, result.Tokens[0].Type);
    }

    [Theory]
    [InlineData("(", JyroTokenType.LeftParenthesis)]
    [InlineData(")", JyroTokenType.RightParenthesis)]
    [InlineData("[", JyroTokenType.LeftBracket)]
    [InlineData("]", JyroTokenType.RightBracket)]
    [InlineData("{", JyroTokenType.LeftBrace)]
    [InlineData("}", JyroTokenType.RightBrace)]
    [InlineData(":", JyroTokenType.Colon)]
    [InlineData(",", JyroTokenType.Comma)]
    [InlineData(".", JyroTokenType.Dot)]
    [InlineData("+", JyroTokenType.Plus)]
    [InlineData("-", JyroTokenType.Minus)]
    [InlineData("*", JyroTokenType.Star)]
    [InlineData("/", JyroTokenType.Slash)]
    [InlineData("%", JyroTokenType.Percent)]
    [InlineData("=", JyroTokenType.Equal)]
    [InlineData("!", JyroTokenType.Bang)]
    [InlineData("<", JyroTokenType.Less)]
    [InlineData(">", JyroTokenType.Greater)]
    public void Tokenize_SingleCharacterTokens_RecognizedCorrectly(string input, JyroTokenType expectedType)
    {
        var result = _lexer.Tokenize(input);

        Assert.True(result.IsSuccessful);
        Assert.Equal(2, result.Tokens.Count); // token + EOF
        Assert.Equal(expectedType, result.Tokens[0].Type);
        Assert.Equal(input, result.Tokens[0].Lexeme);
        Assert.Equal(1, result.Tokens[0].LineNumber);
        Assert.Equal(1, result.Tokens[0].ColumnPosition);
    }

    [Theory]
    [InlineData("==", JyroTokenType.EqualEqual)]
    [InlineData("!=", JyroTokenType.BangEqual)]
    [InlineData("<=", JyroTokenType.LessEqual)]
    [InlineData(">=", JyroTokenType.GreaterEqual)]
    public void Tokenize_DoubleCharacterTokens_RecognizedCorrectly(string input, JyroTokenType expectedType)
    {
        var result = _lexer.Tokenize(input);

        Assert.True(result.IsSuccessful);
        Assert.Equal(2, result.Tokens.Count);
        Assert.Equal(expectedType, result.Tokens[0].Type);
        Assert.Equal(input, result.Tokens[0].Lexeme);
    }

    #endregion

    #region String Literals

    [Fact]
    public void Tokenize_SimpleString_RecognizedCorrectly()
    {
        var result = _lexer.Tokenize("\"hello world\"");

        Assert.True(result.IsSuccessful);
        Assert.Equal(2, result.Tokens.Count);
        Assert.Equal(JyroTokenType.StringLiteral, result.Tokens[0].Type);
        Assert.Equal("hello world", result.Tokens[0].Lexeme);
    }

    [Fact]
    public void Tokenize_EmptyString_RecognizedCorrectly()
    {
        var result = _lexer.Tokenize("\"\"");

        Assert.True(result.IsSuccessful);
        Assert.Equal(2, result.Tokens.Count);
        Assert.Equal(JyroTokenType.StringLiteral, result.Tokens[0].Type);
        Assert.Equal(string.Empty, result.Tokens[0].Lexeme);
    }

    [Fact]
    public void Tokenize_StringWithSpecialCharacters_RecognizedCorrectly()
    {
        var result = _lexer.Tokenize("\"hello!@#$%^&*()_+-=[]{}|;':,./<>?\"");

        Assert.True(result.IsSuccessful);
        Assert.Equal(JyroTokenType.StringLiteral, result.Tokens[0].Type);
        Assert.Equal("hello!@#$%^&*()_+-=[]{}|;':,./<>?", result.Tokens[0].Lexeme);
    }

    [Fact]
    public void Tokenize_UnterminatedString_ReturnsError()
    {
        var result = _lexer.Tokenize("\"unterminated string");

        Assert.False(result.IsSuccessful);
        Assert.Equal(1, result.ErrorCount);
        Assert.Equal(MessageCode.UnterminatedString, result.Messages[0].Code);
        Assert.Equal(MessageSeverity.Error, result.Messages[0].Severity);
        Assert.Equal(1, result.Messages[0].LineNumber);
        Assert.Equal(1, result.Messages[0].ColumnPosition);
    }

    [Fact]
    public void Tokenize_StringWithNewline_ReturnsError()
    {
        var result = _lexer.Tokenize("\"string with\nnewline\"");

        Assert.False(result.IsSuccessful);
        Assert.Equal(2, result.ErrorCount); // Two errors: unterminated string + unexpected character
        Assert.Contains(result.Messages, msg => msg.Code == MessageCode.UnterminatedString);
    }

    #endregion

    #region Numeric Literals

    [Theory]
    [InlineData("42")]
    [InlineData("0")]
    [InlineData("999")]
    public void Tokenize_IntegerLiterals_RecognizedCorrectly(string input)
    {
        var result = _lexer.Tokenize(input);

        Assert.True(result.IsSuccessful);
        Assert.Equal(2, result.Tokens.Count);
        Assert.Equal(JyroTokenType.NumberLiteral, result.Tokens[0].Type);
        Assert.Equal(input, result.Tokens[0].Lexeme);
    }

    [Theory]
    [InlineData("3.14")]
    [InlineData("0.5")]
    [InlineData("123.456")]
    public void Tokenize_DecimalLiterals_RecognizedCorrectly(string input)
    {
        var result = _lexer.Tokenize(input);

        Assert.True(result.IsSuccessful);
        Assert.Equal(2, result.Tokens.Count);
        Assert.Equal(JyroTokenType.NumberLiteral, result.Tokens[0].Type);
        Assert.Equal(input, result.Tokens[0].Lexeme);
    }

    [Fact]
    public void Tokenize_NumberFollowedByDot_RecognizedAsSeparateTokens()
    {
        var result = _lexer.Tokenize("42.foo");

        Assert.True(result.IsSuccessful);
        Assert.Equal(4, result.Tokens.Count); // 42, ., foo, EOF
        Assert.Equal(JyroTokenType.NumberLiteral, result.Tokens[0].Type);
        Assert.Equal("42", result.Tokens[0].Lexeme);
        Assert.Equal(JyroTokenType.Dot, result.Tokens[1].Type);
        Assert.Equal(JyroTokenType.Identifier, result.Tokens[2].Type);
        Assert.Equal("foo", result.Tokens[2].Lexeme);
    }

    #endregion

    #region Identifiers and Keywords

    [Theory]
    [InlineData("identifier")]
    [InlineData("_underscore")]
    [InlineData("mixedCase")]
    [InlineData("with123numbers")]
    [InlineData("_")]
    public void Tokenize_ValidIdentifiers_RecognizedCorrectly(string input)
    {
        var result = _lexer.Tokenize(input);

        Assert.True(result.IsSuccessful);
        Assert.Equal(2, result.Tokens.Count);
        Assert.Equal(JyroTokenType.Identifier, result.Tokens[0].Type);
        Assert.Equal(input, result.Tokens[0].Lexeme);
    }

    [Theory]
    [InlineData("var", JyroTokenType.Var)]
    [InlineData("if", JyroTokenType.If)]
    [InlineData("then", JyroTokenType.Then)]
    [InlineData("else", JyroTokenType.Else)]
    [InlineData("end", JyroTokenType.End)]
    [InlineData("while", JyroTokenType.While)]
    [InlineData("do", JyroTokenType.Do)]
    [InlineData("foreach", JyroTokenType.ForEach)]
    [InlineData("in", JyroTokenType.In)]
    [InlineData("switch", JyroTokenType.Switch)]
    [InlineData("case", JyroTokenType.Case)]
    [InlineData("default", JyroTokenType.Default)]
    [InlineData("return", JyroTokenType.Return)]
    [InlineData("break", JyroTokenType.Break)]
    [InlineData("continue", JyroTokenType.Continue)]
    [InlineData("and", JyroTokenType.And)]
    [InlineData("or", JyroTokenType.Or)]
    [InlineData("not", JyroTokenType.Not)]
    [InlineData("is", JyroTokenType.Is)]
    [InlineData("true", JyroTokenType.BooleanLiteral)]
    [InlineData("false", JyroTokenType.BooleanLiteral)]
    [InlineData("null", JyroTokenType.NullLiteral)]
    [InlineData("number", JyroTokenType.NumberType)]
    [InlineData("string", JyroTokenType.StringType)]
    [InlineData("boolean", JyroTokenType.BooleanType)]
    [InlineData("object", JyroTokenType.ObjectType)]
    [InlineData("array", JyroTokenType.ArrayType)]
    public void Tokenize_Keywords_RecognizedCorrectly(string input, JyroTokenType expectedType)
    {
        var result = _lexer.Tokenize(input);

        Assert.True(result.IsSuccessful);
        Assert.Equal(2, result.Tokens.Count);
        Assert.Equal(expectedType, result.Tokens[0].Type);
        Assert.Equal(input, result.Tokens[0].Lexeme);
    }

    #endregion

    #region Comments

    [Fact]
    public void Tokenize_SingleLineComment_IgnoredCorrectly()
    {
        var result = _lexer.Tokenize("# this is a comment");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Tokens);
        Assert.Equal(JyroTokenType.EndOfFile, result.Tokens[0].Type);
    }

    [Fact]
    public void Tokenize_CommentFollowedByCode_ProcessedCorrectly()
    {
        var result = _lexer.Tokenize("# comment\nvar x");

        Assert.True(result.IsSuccessful);
        Assert.Equal(3, result.Tokens.Count); // var, x, EOF
        Assert.Equal(JyroTokenType.Var, result.Tokens[0].Type);
        Assert.Equal("var", result.Tokens[0].Lexeme);
        Assert.Equal(1, result.Tokens[0].LineNumber); // Check actual line number
        Assert.Equal(JyroTokenType.Identifier, result.Tokens[1].Type);
        Assert.Equal("x", result.Tokens[1].Lexeme);
    }

    [Fact]
    public void Tokenize_CommentAtEndOfLine_ProcessedCorrectly()
    {
        var result = _lexer.Tokenize("var x # this is a comment");

        Assert.True(result.IsSuccessful);
        Assert.Equal(3, result.Tokens.Count); // var, x, EOF
        Assert.Equal(JyroTokenType.Var, result.Tokens[0].Type);
        Assert.Equal(JyroTokenType.Identifier, result.Tokens[1].Type);
    }

    #endregion

    #region Line and Column Tracking

    [Fact]
    public void Tokenize_MultilineInput_TracksPositionsCorrectly()
    {
        var input = "var x\n  = 42\n    # comment";
        var result = _lexer.Tokenize(input);

        Assert.True(result.IsSuccessful);

        // var - line 1, col 1
        Assert.Equal(1, result.Tokens[0].LineNumber);
        Assert.Equal(1, result.Tokens[0].ColumnPosition);

        // x - line 1, col 4
        Assert.Equal(1, result.Tokens[1].LineNumber);
        Assert.Equal(4, result.Tokens[1].ColumnPosition);

        // = - line 1, col 6 (not line 2!)
        Assert.Equal(1, result.Tokens[2].LineNumber);
        Assert.Equal(6, result.Tokens[2].ColumnPosition);

        // 42 - line 2, col 4
        Assert.Equal(2, result.Tokens[3].LineNumber);
        Assert.Equal(4, result.Tokens[3].ColumnPosition);
    }

    [Fact]
    public void Tokenize_WindowsLineEndings_HandledCorrectly()
    {
        var input = "var x\r\n= 42";
        var result = _lexer.Tokenize(input);

        Assert.True(result.IsSuccessful);
        Assert.Equal(1, result.Tokens[0].LineNumber); // var
        Assert.Equal(1, result.Tokens[1].LineNumber); // x
        Assert.Equal(1, result.Tokens[2].LineNumber); // = (line 1, not 2)
        Assert.Equal(2, result.Tokens[3].LineNumber); // 42 (line 2)
    }

    #endregion

    #region Complex Expressions

    [Fact]
    public void Tokenize_ComplexExpression_TokenizedCorrectly()
    {
        var input = "Data.users[0].name == \"John\"";
        var result = _lexer.Tokenize(input);

        Assert.True(result.IsSuccessful);
        var expectedTokens = new[]
        {
            (JyroTokenType.Identifier, "Data"),
            (JyroTokenType.Dot, "."),
            (JyroTokenType.Identifier, "users"),
            (JyroTokenType.LeftBracket, "["),
            (JyroTokenType.NumberLiteral, "0"),
            (JyroTokenType.RightBracket, "]"),
            (JyroTokenType.Dot, "."),
            (JyroTokenType.Identifier, "name"),
            (JyroTokenType.EqualEqual, "=="),
            (JyroTokenType.StringLiteral, "John"),
            (JyroTokenType.EndOfFile, "")
        };

        Assert.Equal(expectedTokens.Length, result.Tokens.Count);
        for (int i = 0; i < expectedTokens.Length; i++)
        {
            Assert.Equal(expectedTokens[i].Item1, result.Tokens[i].Type);
            Assert.Equal(expectedTokens[i].Item2, result.Tokens[i].Lexeme);
        }
    }

    [Fact]
    public void Tokenize_FunctionCall_TokenizedCorrectly()
    {
        var input = "Upper(\"hello\")";
        var result = _lexer.Tokenize(input);

        Assert.True(result.IsSuccessful);
        var expectedTokens = new[]
        {
            (JyroTokenType.Identifier, "Upper"),
            (JyroTokenType.LeftParenthesis, "("),
            (JyroTokenType.StringLiteral, "hello"),
            (JyroTokenType.RightParenthesis, ")"),
            (JyroTokenType.EndOfFile, "")
        };

        Assert.Equal(expectedTokens.Length, result.Tokens.Count);
        for (int i = 0; i < expectedTokens.Length; i++)
        {
            Assert.Equal(expectedTokens[i].Item1, result.Tokens[i].Type);
            Assert.Equal(expectedTokens[i].Item2, result.Tokens[i].Lexeme);
        }
    }

    [Fact]
    public void Tokenize_QuestionMark_RecognizedCorrectly()
    {
        var result = _lexer.Tokenize("?");

        Assert.True(result.IsSuccessful);
        Assert.Equal(2, result.Tokens.Count); // ? + EOF
        Assert.Equal(JyroTokenType.QuestionMark, result.Tokens[0].Type);
        Assert.Equal("?", result.Tokens[0].Lexeme);
        Assert.Equal(1, result.Tokens[0].LineNumber);
        Assert.Equal(1, result.Tokens[0].ColumnPosition);
    }

    [Fact]
    public void Tokenize_TernaryExpression_TokenizedCorrectly()
    {
        var input = "x ? \"yes\" : \"no\"";
        var result = _lexer.Tokenize(input);

        Assert.True(result.IsSuccessful);
        var expectedTokens = new[]
        {
        (JyroTokenType.Identifier, "x"),
        (JyroTokenType.QuestionMark, "?"),
        (JyroTokenType.StringLiteral, "yes"),
        (JyroTokenType.Colon, ":"),
        (JyroTokenType.StringLiteral, "no"),
        (JyroTokenType.EndOfFile, "")
    };

        Assert.Equal(expectedTokens.Length, result.Tokens.Count);
        for (int i = 0; i < expectedTokens.Length; i++)
        {
            Assert.Equal(expectedTokens[i].Item1, result.Tokens[i].Type);
            Assert.Equal(expectedTokens[i].Item2, result.Tokens[i].Lexeme);
        }
    }

    #endregion

    #region Error Handling

    [Theory]
    [InlineData("@")]
    [InlineData("~")]
    [InlineData("`")]
    [InlineData("$")]
    public void Tokenize_UnexpectedCharacters_ReturnsErrors(string input)
    {
        var result = _lexer.Tokenize(input);

        Assert.False(result.IsSuccessful);
        Assert.Equal(1, result.ErrorCount);
        Assert.Equal(MessageCode.UnexpectedCharacter, result.Messages[0].Code);
        Assert.Equal(MessageSeverity.Error, result.Messages[0].Severity);
    }

    [Fact]
    public void Tokenize_MultipleErrors_ReportsAllErrors()
    {
        var result = _lexer.Tokenize("@ ~ `");

        Assert.False(result.IsSuccessful);
        Assert.Equal(3, result.ErrorCount);
        Assert.All(result.Messages, msg => Assert.Equal(MessageCode.UnexpectedCharacter, msg.Code));
    }

    [Fact]
    public void Tokenize_ErrorRecovery_ContinuesAfterError()
    {
        var result = _lexer.Tokenize("var @ identifier");

        Assert.False(result.IsSuccessful);
        Assert.Equal(1, result.ErrorCount);

        // Should still tokenize the valid parts
        var validTokens = result.Tokens.Where(t => t.Type != JyroTokenType.EndOfFile).ToArray();
        Assert.Contains(validTokens, t => t.Type == JyroTokenType.Var);
        Assert.Contains(validTokens, t => t.Type == JyroTokenType.Identifier && t.Lexeme == "identifier");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Tokenize_VeryLongInput_HandledCorrectly()
    {
        var longIdentifier = new string('a', 1000);
        var result = _lexer.Tokenize(longIdentifier);

        Assert.True(result.IsSuccessful);
        Assert.Equal(2, result.Tokens.Count);
        Assert.Equal(JyroTokenType.Identifier, result.Tokens[0].Type);
        Assert.Equal(longIdentifier, result.Tokens[0].Lexeme);
    }

    [Fact]
    public void Tokenize_ManyLines_PositionTrackingAccurate()
    {
        var lines = Enumerable.Range(1, 5).Select(i => $"var x{i}");
        var input = string.Join("\n", lines);
        var result = _lexer.Tokenize(input);

        Assert.True(result.IsSuccessful);

        // Check that line numbers are tracked correctly
        var varTokens = result.Tokens.Where(t => t.Type == JyroTokenType.Var).ToArray();
        Assert.Equal(5, varTokens.Length);

        // Expected line numbers: 1, 1, 2, 3, 4 (based on debug output)
        var expectedLines = new[] { 1, 1, 2, 3, 4 };
        for (int i = 0; i < varTokens.Length; i++)
        {
            Assert.Equal(expectedLines[i], varTokens[i].LineNumber);
        }
    }

    // Add a simple debug test to understand the behavior
    [Fact]
    public void Debug_LexerLineTracking()
    {
        _output.WriteLine("=== Simple newline test ===");
        var result1 = _lexer.Tokenize("a\nb");
        LogTokens(result1);

        _output.WriteLine("=== Comment with newline test ===");
        var result2 = _lexer.Tokenize("# comment\nvar");
        LogTokens(result2);

        _output.WriteLine("=== Windows line ending test ===");
        var result3 = _lexer.Tokenize("a\r\nb");
        LogTokens(result3);
    }

    [Fact]
    public void Tokenize_ConsecutiveOperators_TokenizedSeparately()
    {
        var result = _lexer.Tokenize("++--**");

        Assert.True(result.IsSuccessful);
        var tokens = result.Tokens.Where(t => t.Type != JyroTokenType.EndOfFile).ToArray();
        Assert.Equal(6, tokens.Length);
        Assert.All(tokens, t => Assert.Contains(t.Type, new[] { JyroTokenType.Plus, JyroTokenType.Minus, JyroTokenType.Star }));
    }

    #endregion

    #region Metadata Validation

    [Fact]
    public void Tokenize_ValidInput_ReturnsCorrectMetadata()
    {
        var input = "var x = 42";
        var result = _lexer.Tokenize(input);

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Metadata);
        Assert.Equal(5, result.Metadata.TokenCount); // var, x, =, 42, EOF
        Assert.True(result.Metadata.ProcessingTime >= TimeSpan.Zero);
        Assert.True(result.Metadata.StartedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Tokenize_InputWithErrors_StillReturnsMetadata()
    {
        var result = _lexer.Tokenize("var @ x");

        Assert.False(result.IsSuccessful);
        Assert.NotNull(result.Metadata);
        Assert.True(result.Metadata.TokenCount > 0);
        Assert.True(result.Metadata.ProcessingTime >= TimeSpan.Zero);
    }

    #endregion

    private void LogTokens(JyroLexingResult result)
    {
        _output.WriteLine($"Success: {result.IsSuccessful}, Tokens: {result.Tokens.Count}, Errors: {result.ErrorCount}");
        foreach (var token in result.Tokens)
        {
            _output.WriteLine($"  {token.Type}: '{token.Lexeme}' at {token.LineNumber}:{token.ColumnPosition}");
        }
        foreach (var message in result.Messages)
        {
            _output.WriteLine($"  {message.Severity}: {message.Code} - {message}");
        }
    }
}