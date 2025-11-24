using Xunit.Abstractions;

namespace Mesch.Jyro.Tests;

public class BasicExpressionTests
{
    private readonly ITestOutputHelper _output;

    public BasicExpressionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ArithmeticOperations_Addition()
    {
        var script = "Data.result = 5 + 3";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(8.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ArithmeticOperations_Subtraction()
    {
        var script = "Data.result = 10 - 4";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(6.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ArithmeticOperations_Multiplication()
    {
        var script = "Data.result = 6 * 7";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(42.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ArithmeticOperations_Division()
    {
        var script = "Data.result = 20 / 4";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(5.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ArithmeticOperations_Modulo()
    {
        var script = "Data.result = 17 % 5";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(2.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ArithmeticOperations_ComplexExpression()
    {
        var script = "Data.result = (5 + 3) * 2 - 4 / 2";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(14.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void StringConcatenation()
    {
        var script = "Data.result = \"Hello\" + \" \" + \"World\"";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("Hello World", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void StringConcatenation_WithNumbers()
    {
        var script = "Data.result = \"The answer is \" + 42";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("The answer is 42", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ComparisonOperators_Equal()
    {
        var script = "Data.result = 5 == 5";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ComparisonOperators_NotEqual()
    {
        var script = "Data.result = 5 != 3";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ComparisonOperators_LessThan()
    {
        var script = "Data.result = 3 < 5";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ComparisonOperators_GreaterThan()
    {
        var script = "Data.result = 7 > 5";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void LogicalOperators_And()
    {
        var script = "Data.result = true and false";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void LogicalOperators_Or()
    {
        var script = "Data.result = true or false";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void LogicalOperators_Not()
    {
        var script = "Data.result = not false";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void UnaryMinus()
    {
        var script = "Data.result = -42";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(-42.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TernaryOperator()
    {
        var script = "Data.result = 5 > 3 ? \"yes\" : \"no\"";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("yes", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ArrayLiteral()
    {
        var script = "Data.result = [1, 2, 3]";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var arr = (JyroArray)data.GetProperty("result");
        Assert.Equal(3, arr.Length);
        Assert.Equal(1.0, ((JyroNumber)arr[0]).Value);
        Assert.Equal(2.0, ((JyroNumber)arr[1]).Value);
        Assert.Equal(3.0, ((JyroNumber)arr[2]).Value);
    }

    [Fact]
    public void ObjectLiteral()
    {
        var script = "Data.result = {\"name\": \"John\", \"age\": 30}";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var obj = (JyroObject)data.GetProperty("result");
        Assert.Equal("John", ((JyroString)obj.GetProperty("name")).Value);
        Assert.Equal(30.0, ((JyroNumber)obj.GetProperty("age")).Value);
    }

    [Fact]
    public void ArrayIndexing()
    {
        var script = @"
            var arr = [10, 20, 30]
            Data.result = arr[1]
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(20.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ObjectPropertyAccess()
    {
        var script = @"
            var obj = {""name"": ""Alice""}
            Data.result = obj.name
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("Alice", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void NestedPropertyAccess()
    {
        var script = @"
            var obj = {""person"": {""name"": ""Bob""}}
            Data.result = obj.person.name
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("Bob", ((JyroString)data.GetProperty("result")).Value);
    }

    #region Type Checking Operators

    [Fact]
    public void TypeCheck_IsNumber_True()
    {
        var script = "Data.result = 42 is number";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsNumber_False()
    {
        var script = "Data.result = \"hello\" is number";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsString_True()
    {
        var script = "Data.result = \"hello\" is string";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsString_False()
    {
        var script = "Data.result = 42 is string";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsBoolean_True()
    {
        var script = "Data.result = true is boolean";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsBoolean_False()
    {
        var script = "Data.result = 1 is boolean";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsObject_True()
    {
        var script = "Data.result = {\"key\": \"value\"} is object";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsObject_False()
    {
        var script = "Data.result = [1, 2, 3] is object";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsArray_True()
    {
        var script = "Data.result = [1, 2, 3] is array";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsArray_False()
    {
        var script = "Data.result = {\"key\": \"value\"} is array";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsNull_True()
    {
        var script = "Data.result = null is null";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsNull_False()
    {
        var script = "Data.result = 0 is null";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsNotNumber_True()
    {
        var script = "Data.result = \"hello\" is not number";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsNotNumber_False()
    {
        var script = "Data.result = 42 is not number";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsNotString_True()
    {
        var script = "Data.result = 42 is not string";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsNotString_False()
    {
        var script = "Data.result = \"hello\" is not string";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsNotBoolean_True()
    {
        var script = "Data.result = 1 is not boolean";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsNotBoolean_False()
    {
        var script = "Data.result = true is not boolean";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsNotObject_True()
    {
        var script = "Data.result = [1, 2, 3] is not object";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsNotObject_False()
    {
        var script = "Data.result = {\"key\": \"value\"} is not object";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsNotArray_True()
    {
        var script = "Data.result = {\"key\": \"value\"} is not array";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsNotArray_False()
    {
        var script = "Data.result = [1, 2, 3] is not array";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsNotNull_True()
    {
        var script = "Data.result = 0 is not null";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsNotNull_False()
    {
        var script = "Data.result = null is not null";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_InIfStatement()
    {
        var script = @"
            var x = 42
            if x is number then
                Data.result = ""is_number""
            else
                Data.result = ""not_number""
            end
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("is_number", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_IsNotInIfStatement()
    {
        var script = @"
            var x = ""hello""
            if x is not number then
                Data.result = ""not_a_number""
            else
                Data.result = ""is_a_number""
            end
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("not_a_number", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeCheck_WithVariable()
    {
        var script = @"
            var obj = {""name"": ""test""}
            Data.result = obj is object
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    #endregion
}
