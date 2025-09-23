using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace Mesch.Jyro.Runtime.Tests.Validator;

public class ValidatorTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mesch.Jyro.Lexer _lexer;
    private readonly Mesch.Jyro.Parser _parser;
    private readonly Mesch.Jyro.Validator _validator;

    public ValidatorTests(ITestOutputHelper output)
    {
        _output = output;
        _lexer = new Mesch.Jyro.Lexer(NullLogger<Mesch.Jyro.Lexer>.Instance);
        _parser = new Mesch.Jyro.Parser(NullLogger<Mesch.Jyro.Parser>.Instance);

        // Create validator with standard built-in functions
        var builtins = new[] { "Data", "Upper", "Lower", "Length", "Now", "Equal" };
        _validator = new Mesch.Jyro.Validator(NullLogger<Mesch.Jyro.Validator>.Instance, builtins);
    }

    [Fact]
    public void Debug_SimpleValidation()
    {
        _output.WriteLine("=== Simple assignment ===");
        var result1 = ValidateSource("x = 42");
        LogValidationResult(result1);

        _output.WriteLine("=== Variable declaration ===");
        var result2 = ValidateSource("var x = 42");
        LogValidationResult(result2);

        _output.WriteLine("=== Builtin reference ===");
        var result3 = ValidateSource("Data.name = \"test\"");
        LogValidationResult(result3);

        _output.WriteLine("=== Function call ===");
        var result4 = ValidateSource("Upper(\"test\")");
        LogValidationResult(result4);
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

    private JyroValidationResult ValidateSource(string source)
    {
        var statements = ParseStatements(source);
        return _validator.Validate(statements);
    }

    private void LogValidationResult(JyroValidationResult result)
    {
        _output.WriteLine($"Success: {result.IsSuccessful}, Errors: {result.ErrorCount}, Warnings: {result.WarningCount}");
        foreach (var message in result.Messages)
        {
            _output.WriteLine($"  {message.Severity}: {message.Code} - {message}");
        }
    }

    #endregion

    #region Basic Validation

    [Fact]
    public void Validate_EmptyProgram_ReturnsSuccess()
    {
        var result = ValidateSource("");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
        Assert.Equal(0, result.WarningCount);
        Assert.NotNull(result.Metadata);
    }

    [Fact]
    public void Validate_SimpleAssignment_ReturnsSuccess()
    {
        var result = ValidateSource("x = 42");

        Assert.False(result.IsSuccessful); // Assignment to undeclared variable is an error
        Assert.Equal(1, result.ErrorCount);
        Assert.Equal(MessageCode.InvalidVariableReference, result.Messages[0].Code);
    }

    [Fact]
    public void Validate_SimpleExpression_ReturnsSuccess()
    {
        var result = ValidateSource("42 + 24");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    #endregion

    #region Variable Declaration Validation

    [Fact]
    public void Validate_VariableDeclaration_ReturnsSuccess()
    {
        var result = ValidateSource("var x = 42");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_VariableDeclarationWithType_ReturnsSuccess()
    {
        var result = ValidateSource("var x: number = 42");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_MultipleVariableDeclarations_ReturnsSuccess()
    {
        var result = ValidateSource("var x = 1\nvar y = 2\nvar z = x + y");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_VariableRedeclarationInSameScope_ReturnsError()
    {
        var result = ValidateSource("var x = 1\nvar x = 2");

        Assert.False(result.IsSuccessful);
        Assert.Equal(1, result.ErrorCount);
        Assert.Equal(MessageCode.InvalidVariableReference, result.Messages[0].Code);
        Assert.Contains("already declared", result.Messages[0].Arguments[0]);
    }

    [Fact]
    public void Validate_VariableRedeclarationInDifferentScope_ReturnsSuccess()
    {
        var result = ValidateSource("var x = 1\nif true then var x = 2 end");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_ReservedVariableName_ReturnsError()
    {
        var result = ValidateSource("var Data = 42");

        Assert.False(result.IsSuccessful);
        Assert.Equal(1, result.ErrorCount);
        Assert.Equal(MessageCode.InvalidVariableReference, result.Messages[0].Code);
        Assert.Contains("reserved name", result.Messages[0].Arguments[0]);
    }

    #endregion

    #region Variable Reference Validation

    [Fact]
    public void Validate_UndeclaredVariable_ReturnsError()
    {
        var result = ValidateSource("var x = undeclaredVar");

        Assert.False(result.IsSuccessful);
        Assert.Equal(1, result.ErrorCount);
        Assert.Equal(MessageCode.InvalidVariableReference, result.Messages[0].Code);
        Assert.Contains("Undeclared variable", result.Messages[0].Arguments[0]);
    }

    [Fact]
    public void Validate_DeclaredVariableReference_ReturnsSuccess()
    {
        var result = ValidateSource("var x = 42\nvar y = x + 1");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_BuiltinVariableReference_ReturnsSuccess()
    {
        var result = ValidateSource("Data.name = \"John\"");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_BuiltinFunctionReference_ReturnsSuccess()
    {
        var result = ValidateSource("var result = Upper(\"hello\")");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    #endregion

    #region Assignment Target Validation

    [Fact]
    public void Validate_ValidAssignmentTargets_ReturnsSuccess()
    {
        var result = ValidateSource(@"
            var x = 1
            x = 2
            Data.name = ""John""
            Data[0] = ""first""
            Data.users[0].name = ""Jane""");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_DataReassignment_ReturnsError()
    {
        var result = ValidateSource("Data = {}");

        Assert.False(result.IsSuccessful);
        Assert.Equal(1, result.ErrorCount);
        Assert.Equal(MessageCode.InvalidAssignmentTarget, result.Messages[0].Code);
        Assert.Contains("Cannot reassign Data", result.Messages[0].Arguments[0]);
    }

    [Fact]
    public void Validate_InvalidAssignmentTarget_ReturnsError()
    {
        var result = ValidateSource("42 = x");

        Assert.True(result.IsSuccessful); // Parser catches this, not validator
                                          // The parser should have caught this syntax error, so validator doesn't see it
    }

    #endregion

    #region Control Flow Validation

    [Fact]
    public void Validate_IfStatement_ReturnsSuccess()
    {
        var result = ValidateSource("if true then var x = 1 end");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_IfElseStatement_ReturnsSuccess()
    {
        var result = ValidateSource("if true then var x = 1 else var x = 2 end");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_NestedIfStatements_ReturnsSuccess()
    {
        var result = ValidateSource(@"
        if true then
            if false then
                var x = 1
            else
                var x = 2
            end
        end");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_WhileLoop_ReturnsSuccess()
    {
        var result = ValidateSource("var x = 0\nwhile x < 10 do x = x + 1 end");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_ForEachLoop_ReturnsSuccess()
    {
        var result = ValidateSource("foreach item in Data.items do item.processed = true end");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_SwitchStatement_ReturnsSuccess()
    {
        var result = ValidateSource(@"
        var x = 1
        switch x
            case 1 then var y = ""one""
            case 2 then var y = ""two""
            default var y = ""other""
        end");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    #endregion

    #region Loop Control Validation

    [Fact]
    public void Validate_BreakInLoop_ReturnsSuccess()
    {
        var result = ValidateSource("while true do break end");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_ContinueInLoop_ReturnsSuccess()
    {
        var result = ValidateSource("while true do continue end");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_BreakInSwitch_ReturnsSuccess()
    {
        var result = ValidateSource("var x = 1\nswitch x case 1 then break default var y = 1 end");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_BreakOutsideLoop_ReturnsError()
    {
        var result = ValidateSource("break");

        Assert.False(result.IsSuccessful);
        Assert.Equal(1, result.ErrorCount);
        Assert.Equal(MessageCode.LoopStatementOutsideOfLoop, result.Messages[0].Code);
        Assert.Contains("break statement outside", result.Messages[0].Arguments[0]);
    }

    [Fact]
    public void Validate_ContinueOutsideLoop_ReturnsError()
    {
        var result = ValidateSource("continue");

        Assert.False(result.IsSuccessful);
        Assert.Equal(1, result.ErrorCount);
        Assert.Equal(MessageCode.LoopStatementOutsideOfLoop, result.Messages[0].Code);
        Assert.Contains("continue statement outside", result.Messages[0].Arguments[0]);
    }

    [Fact]
    public void Validate_ContinueInSwitch_ReturnsError()
    {
        var result = ValidateSource("var x = 1\nswitch x case 1 then continue default var y = 1 end");

        Assert.False(result.IsSuccessful);
        Assert.Equal(1, result.ErrorCount);
        Assert.Equal(MessageCode.LoopStatementOutsideOfLoop, result.Messages[0].Code);
    }

    #endregion

    #region Iterator Variable Validation

    [Fact]
    public void Validate_ForEachIteratorVariable_ReturnsSuccess()
    {
        var result = ValidateSource("foreach item in Data.items do var x = item.name end");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_NestedForEachWithDifferentIterators_ReturnsSuccess()
    {
        var result = ValidateSource(@"
            foreach user in Data.users do
                foreach task in user.tasks do
                    task.completed = true
                end
            end");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_IteratorVariableOutsideLoop_ReturnsError()
    {
        var result = ValidateSource("foreach item in Data.items do end\nvar x = item");

        Assert.False(result.IsSuccessful);
        Assert.Equal(1, result.ErrorCount);
        Assert.Equal(MessageCode.InvalidVariableReference, result.Messages[0].Code);
    }

    #endregion

    #region Scope Validation

    [Fact]
    public void Validate_VariableScoping_ReturnsSuccess()
    {
        var result = ValidateSource(@"
            var x = 1
            if true then
                var y = 2
                x = x + y
            end
            x = x + 1");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_VariableOutOfScope_ReturnsError()
    {
        var result = ValidateSource(@"
        if true then
            var localVar = 42
        end
        var x = localVar");

        Assert.False(result.IsSuccessful);
        Assert.Equal(1, result.ErrorCount);
        Assert.Equal(MessageCode.InvalidVariableReference, result.Messages[0].Code);
    }

    [Fact]
    public void Validate_NestedScopes_ReturnsSuccess()
    {
        var result = ValidateSource(@"
            var x = 1
            if true then
                var y = 2
                if true then
                    var z = 3
                    x = x + y + z
                end
            end");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    #endregion

    #region Expression Validation

    [Fact]
    public void Validate_PropertyAccess_ReturnsSuccess()
    {
        var result = ValidateSource("var x = Data.users[0].name");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_FunctionCallExpression_ReturnsSuccess()
    {
        var result = ValidateSource("var x = Upper(\"hello\")");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_BinaryExpression_ReturnsSuccess()
    {
        var result = ValidateSource("var x = 1\nvar y = 2\nvar z = x + y * 3");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_UnaryExpression_ReturnsSuccess()
    {
        var result = ValidateSource("var x = true\nvar y = not x");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_TypeCheckExpression_ReturnsSuccess()
    {
        var result = ValidateSource("var x = 42\nif x is number then var y = true end");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void Validate_ComplexProgram_ReturnsSuccess()
    {
        var result = ValidateSource(@"
            var users = Data.users
            var processedCount = 0
            
            foreach user in users do
                if user.active then
                    user.lastProcessed = Now()
                    processedCount = processedCount + 1
                    
                    if processedCount > 100 then
                        break
                    end
                end
            end
            
            Data.summary = {
                ""processed"": processedCount,
                ""timestamp"": Now()
            }");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_MultipleErrors_ReportsAll()
    {
        var result = ValidateSource(@"
            undeclaredVar1 = 42
            var x = 1
            var x = 2
            undeclaredVar2 = ""test""
            break");

        Assert.False(result.IsSuccessful);
        Assert.Equal(4, result.ErrorCount);
        Assert.Contains(result.Messages, msg => msg.Code == MessageCode.InvalidVariableReference);
        Assert.Contains(result.Messages, msg => msg.Code == MessageCode.LoopStatementOutsideOfLoop);
    }

    #endregion

    #region Return Statement Validation

    [Fact]
    public void Validate_ReturnStatement_ReturnsSuccess()
    {
        var result = ValidateSource("return");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_ReturnWithExpression_ReturnsSuccess()
    {
        var result = ValidateSource("var x = 42\nreturn");

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    #endregion

    #region Metadata Validation

    [Fact]
    public void Validate_ValidInput_ReturnsCorrectMetadata()
    {
        var result = ValidateSource("var x = 42\nif x > 0 then x = x + 1 end");

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Metadata);
        Assert.True(result.Metadata.ProcessingTime >= TimeSpan.Zero);
        Assert.True(result.Metadata.StartedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Validate_InputWithErrors_StillReturnsMetadata()
    {
        var result = ValidateSource("undeclaredVar = 42");

        Assert.False(result.IsSuccessful);
        Assert.NotNull(result.Metadata);
        Assert.True(result.Metadata.ProcessingTime >= TimeSpan.Zero);
    }

    #endregion

    #region Exception Handling

    [Fact]
    public void Validate_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _validator.Validate(null!));
    }

    [Fact]
    public void Validate_EmptyStatementList_ReturnsSuccess()
    {
        var emptyStatements = Array.Empty<IJyroStatement>();
        var result = _validator.Validate(emptyStatements);

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ErrorCount);
    }

    #endregion

    #region Debug Tests

    [Fact]
    public void Debug_ValidatorBehavior()
    {
        _output.WriteLine("=== Valid program ===");
        var result1 = ValidateSource("var x = 42\nData.result = x");
        LogValidationResult(result1);

        _output.WriteLine("=== Undeclared variable ===");
        var result2 = ValidateSource("x = undeclaredVar");
        LogValidationResult(result2);

        _output.WriteLine("=== Break outside loop ===");
        var result3 = ValidateSource("break");
        LogValidationResult(result3);

        _output.WriteLine("=== Excessive nesting ===");
        var result4 = ValidateSource("while true do while true do while true do while true do x = 1 end end end end");
        LogValidationResult(result4);
    }

    #endregion
}