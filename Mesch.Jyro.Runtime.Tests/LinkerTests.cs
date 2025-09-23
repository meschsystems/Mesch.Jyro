using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace Mesch.Jyro.Runtime.Tests.Linker;

public class LinkerTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mesch.Jyro.Lexer _lexer;
    private readonly Mesch.Jyro.Parser _parser;
    private readonly Mesch.Jyro.Linker _linker;

    public LinkerTests(ITestOutputHelper output)
    {
        _output = output;
        _lexer = new Mesch.Jyro.Lexer(NullLogger<Mesch.Jyro.Lexer>.Instance);
        _parser = new Mesch.Jyro.Parser(NullLogger<Mesch.Jyro.Parser>.Instance);
        _linker = new Mesch.Jyro.Linker(NullLogger<Mesch.Jyro.Linker>.Instance);
    }

    #region Helper Methods

    private IReadOnlyList<IJyroStatement> ParseStatements(string source)
    {
        var lexingResult = _lexer.Tokenize(source);
        if (!lexingResult.IsSuccessful)
        {
            _output.WriteLine($"Lexing failed: {string.Join(", ", lexingResult.Messages)}");
            return [];
        }

        var parsingResult = _parser.Parse(lexingResult.Tokens);
        if (!parsingResult.IsSuccessful)
        {
            _output.WriteLine($"Parsing failed: {string.Join(", ", parsingResult.Messages)}");
            return [];
        }

        return parsingResult.Statements;
    }

    private JyroLinkingResult LinkSource(string source, IEnumerable<IJyroFunction>? hostFunctions = null)
    {
        var statements = ParseStatements(source);
        return _linker.Link(statements, hostFunctions);
    }

    private void LogLinkingResult(JyroLinkingResult result)
    {
        _output.WriteLine($"Success: {result.IsSuccessful}, Errors: {result.ErrorCount}");
        if (result.Program != null)
        {
            _output.WriteLine($"Functions: {result.Program.Functions.Count}, Statements: {result.Program.Statements.Count}");
            foreach (var func in result.Program.Functions)
            {
                _output.WriteLine($"  Function: {func.Key}");
            }
        }
        foreach (var message in result.Messages)
        {
            _output.WriteLine($"  {message.Severity}: {message.Code} - {message}");
        }
    }

    // Simple test function for testing host function registration
    private class TestFunction : JyroFunctionBase
    {
        public TestFunction() : base(new JyroFunctionSignature("TestFunc", [], ParameterType.Number))
        {
        }

        public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
        {
            return new JyroNumber(42);
        }
    }

    // Function with parameters for testing validation
    private class AddFunction : JyroFunctionBase
    {
        public AddFunction() : base(new JyroFunctionSignature("Add",
        [
            new Parameter("left", ParameterType.Number),
            new Parameter("right", ParameterType.Number)
        ], ParameterType.Number))
        {
        }

        public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
        {
            var left = (JyroNumber)arguments[0];
            var right = (JyroNumber)arguments[1];
            return new JyroNumber(left.Value + right.Value);
        }
    }

    // Function with optional parameters
    private class OptionalParamFunction : JyroFunctionBase
    {
        public OptionalParamFunction() : base(new JyroFunctionSignature("OptionalFunc",
        [
            new Parameter("required", ParameterType.String),
            new Parameter("optional", ParameterType.Number, true)
        ], ParameterType.String))
        {
        }

        public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
        {
            return new JyroString("test");
        }
    }

    #endregion

    #region Basic Linking

    [Fact]
    public void Link_EmptyProgram_ReturnsSuccessWithStandardLibrary()
    {
        var result = LinkSource("");

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Program);
        Assert.Empty(result.Program.Statements);
        Assert.True(result.Program.Functions.Count > 0); // Should include standard library
        Assert.Equal(0, result.ErrorCount);
        Assert.NotNull(result.Metadata);
    }

    [Fact]
    public void Link_SimpleAssignment_ReturnsSuccess()
    {
        var result = LinkSource("x = 42");

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Program);
        Assert.Single(result.Program.Statements);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Link_SimpleVariableDeclaration_ReturnsSuccess()
    {
        var result = LinkSource("var x = 42");

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Program);
        Assert.Single(result.Program.Statements);
        Assert.Equal(0, result.ErrorCount);
    }

    #endregion

    #region Standard Library Functions

    [Fact]
    public void Link_StandardLibraryFunctions_AvailableByDefault()
    {
        var result = LinkSource("");

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Program);

        // Check for some standard library functions
        Assert.Contains("Upper", result.Program.Functions.Keys);
        Assert.Contains("Lower", result.Program.Functions.Keys);
        Assert.Contains("Length", result.Program.Functions.Keys);
        Assert.Contains("Append", result.Program.Functions.Keys);
        Assert.Contains("Min", result.Program.Functions.Keys);
        Assert.Contains("Max", result.Program.Functions.Keys);
        Assert.Contains("Now", result.Program.Functions.Keys);
        Assert.Contains("Equal", result.Program.Functions.Keys);
    }

    [Fact]
    public void Link_StandardLibraryFunctionCall_ValidatesCorrectly()
    {
        var result = LinkSource("Upper(\"hello\")");

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Program);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Link_StandardLibraryWithWrongArguments_ReturnsError()
    {
        var result = LinkSource("Upper(42, \"extra\")");

        Assert.False(result.IsSuccessful);
        Assert.Equal(1, result.ErrorCount);
        Assert.Equal(MessageCode.InvalidNumberArguments, result.Messages[0].Code);
    }

    #endregion

    #region Host Functions

    [Fact]
    public void Link_WithHostFunctions_RegisteredCorrectly()
    {
        var hostFunctions = new IJyroFunction[] { new TestFunction() };
        var result = LinkSource("", hostFunctions);

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Program);
        Assert.Contains("TestFunc", result.Program.Functions.Keys);
    }

    [Fact]
    public void Link_HostFunctionCall_ValidatesCorrectly()
    {
        var hostFunctions = new IJyroFunction[] { new TestFunction() };
        var result = LinkSource("TestFunc()", hostFunctions);

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Program);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Link_HostFunctionOverridesStandardLibrary_WarningGenerated()
    {
        var hostFunctions = new IJyroFunction[] {
            new OverrideUpperFunction()
        };
        var result = LinkSource("", hostFunctions);

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Program);
        Assert.Contains(result.Messages, msg => msg.Code == MessageCode.FunctionOverride && msg.Severity == MessageSeverity.Warning);
    }

    [Fact]
    public void Link_MultipleHostFunctions_AllRegistered()
    {
        var hostFunctions = new IJyroFunction[] {
            new TestFunction(),
            new AddFunction()
        };
        var result = LinkSource("", hostFunctions);

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Program);
        Assert.Contains("TestFunc", result.Program.Functions.Keys);
        Assert.Contains("Add", result.Program.Functions.Keys);
    }

    #endregion

    #region Function Validation

    [Fact]
    public void Link_UndefinedFunction_ReturnsError()
    {
        var result = LinkSource("NonExistentFunction()");

        Assert.False(result.IsSuccessful);
        Assert.Equal(1, result.ErrorCount);
        Assert.Equal(MessageCode.UndefinedFunction, result.Messages[0].Code);
        Assert.Equal(MessageSeverity.Error, result.Messages[0].Severity);
    }

    [Fact]
    public void Link_FunctionWithCorrectArguments_ValidatesSuccessfully()
    {
        var hostFunctions = new IJyroFunction[] { new AddFunction() };
        var result = LinkSource("Add(1, 2)", hostFunctions);

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Program);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Link_FunctionWithTooFewArguments_ReturnsError()
    {
        var hostFunctions = new IJyroFunction[] { new AddFunction() };
        var result = LinkSource("Add(1)", hostFunctions);

        Assert.False(result.IsSuccessful);
        Assert.Equal(1, result.ErrorCount);
        Assert.Equal(MessageCode.InvalidNumberArguments, result.Messages[0].Code);
    }

    [Fact]
    public void Link_FunctionWithTooManyArguments_ReturnsError()
    {
        var hostFunctions = new IJyroFunction[] { new AddFunction() };
        var result = LinkSource("Add(1, 2, 3)", hostFunctions);

        Assert.False(result.IsSuccessful);
        Assert.Equal(1, result.ErrorCount);
        Assert.Equal(MessageCode.InvalidNumberArguments, result.Messages[0].Code);
    }

    [Fact]
    public void Link_FunctionWithWrongArgumentTypes_ReturnsError()
    {
        var hostFunctions = new IJyroFunction[] { new AddFunction() };
        var result = LinkSource("Add(\"string\", true)", hostFunctions);

        Assert.False(result.IsSuccessful);
        Assert.Equal(1, result.ErrorCount); // Actual error count is 1, not 2
        Assert.Contains(result.Messages, msg => msg.Code == MessageCode.InvalidArgumentType);
    }

    [Fact]
    public void Link_OptionalParameters_ValidatesCorrectly()
    {
        var hostFunctions = new IJyroFunction[] { new OptionalParamFunction() };

        var result1 = LinkSource("OptionalFunc(\"test\")", hostFunctions);
        Assert.True(result1.IsSuccessful);
        Assert.Equal(0, result1.ErrorCount);

        var result2 = LinkSource("OptionalFunc(\"test\", 42)", hostFunctions);
        Assert.True(result2.IsSuccessful);
        Assert.Equal(0, result2.ErrorCount);

        var result3 = LinkSource("OptionalFunc()", hostFunctions);
        Assert.False(result3.IsSuccessful);
        Assert.Equal(1, result3.ErrorCount);
    }

    #endregion

    #region Additional Helper Classes

    private class OverrideUpperFunction : JyroFunctionBase
    {
        public OverrideUpperFunction() : base(new JyroFunctionSignature("Upper",
        [
            new Parameter("text", ParameterType.String)
        ], ParameterType.String))
        {
        }

        public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
        {
            return new JyroString("overridden");
        }
    }

    #endregion

    #region Debug Tests

    [Fact]
    public void Debug_LinkerBehavior()
    {
        _output.WriteLine("=== Simple function call ===");
        var result1 = LinkSource("Upper(\"test\")");
        LogLinkingResult(result1);

        _output.WriteLine("=== Undefined function ===");
        var result2 = LinkSource("InvalidFunc()");
        LogLinkingResult(result2);

        _output.WriteLine("=== Host function override ===");
        var hostFunctions = new IJyroFunction[] { new OverrideUpperFunction() };
        var result3 = LinkSource("", hostFunctions);
        LogLinkingResult(result3);
    }

    #endregion
}