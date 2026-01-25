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

    #region Keywords as Property Names

    [Fact]
    public void PropertyAccess_TypeNameAsProperty_Number()
    {
        var script = "Data.number = 42";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(42.0, ((JyroNumber)data.GetProperty("number")).Value);
    }

    [Fact]
    public void PropertyAccess_TypeNameAsProperty_String()
    {
        var script = "Data.string = \"hello\"";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("hello", ((JyroString)data.GetProperty("string")).Value);
    }

    [Fact]
    public void PropertyAccess_TypeNameAsProperty_Boolean()
    {
        var script = "Data.boolean = true";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("boolean")).Value);
    }

    [Fact]
    public void PropertyAccess_TypeNameAsProperty_Object()
    {
        var script = "Data.object = {\"key\": \"value\"}";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var obj = (JyroObject)data.GetProperty("object");
        Assert.Equal("value", ((JyroString)obj.GetProperty("key")).Value);
    }

    [Fact]
    public void PropertyAccess_TypeNameAsProperty_Array()
    {
        var script = "Data.array = [1, 2, 3]";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var arr = (JyroArray)data.GetProperty("array");
        Assert.Equal(3, arr.Length);
    }

    [Fact]
    public void PropertyAccess_BooleanLiteralAsProperty_True()
    {
        var script = "Data.true = \"yes\"";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("yes", ((JyroString)data.GetProperty("true")).Value);
    }

    [Fact]
    public void PropertyAccess_BooleanLiteralAsProperty_False()
    {
        var script = "Data.false = \"no\"";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("no", ((JyroString)data.GetProperty("false")).Value);
    }

    [Fact]
    public void PropertyAccess_NullAsProperty()
    {
        var script = "Data.null = 123";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(123.0, ((JyroNumber)data.GetProperty("null")).Value);
    }

    [Fact]
    public void PropertyAccess_ControlFlowKeywordAsProperty_If()
    {
        var script = "Data.if = \"condition\"";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("condition", ((JyroString)data.GetProperty("if")).Value);
    }

    [Fact]
    public void PropertyAccess_ControlFlowKeywordAsProperty_While()
    {
        var script = "Data.while = \"loop\"";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("loop", ((JyroString)data.GetProperty("while")).Value);
    }

    [Fact]
    public void PropertyAccess_ReadTypeNameProperty()
    {
        var script = @"
            var obj = {""number"": 99}
            Data.result = obj.number
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(99.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void PropertyAccess_MultipleTypeNamesAsProperties()
    {
        var script = @"
            Data.number = 42
            Data.string = ""test""
            Data.boolean = false
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(42.0, ((JyroNumber)data.GetProperty("number")).Value);
        Assert.Equal("test", ((JyroString)data.GetProperty("string")).Value);
        Assert.False(((JyroBoolean)data.GetProperty("boolean")).Value);
    }

    [Fact]
    public void PropertyAccess_NestedTypeNameProperties()
    {
        var script = @"
            var obj = {""string"": {""number"": 7}}
            Data.result = obj.string.number
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(7.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    #endregion

    #region Bracket Notation with Dots (JSON Behavior)

    [Fact]
    public void BracketNotation_LiteralKeyWithDots_SetAndGet()
    {
        // Keys with dots should be treated literally in bracket notation (JSON behavior)
        // obj["a.b"] should NOT traverse path a -> b
        var script = @"
            var obj = {}
            obj[""address.city""] = ""New York""
            Data.result = obj[""address.city""]
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("New York", ((JyroString)data.GetPropertyLiteral("result")).Value);
    }

    [Fact]
    public void BracketNotation_EmailAsKey_SetAndGet()
    {
        // Email addresses contain dots - should work as literal keys
        var script = @"
            var map = {}
            map[""john.doe@example.com""] = ""John Doe""
            Data.result = map[""john.doe@example.com""]
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("John Doe", ((JyroString)data.GetPropertyLiteral("result")).Value);
    }

    [Fact]
    public void BracketNotation_DynamicKeyWithDots_SetAndGet()
    {
        // Dynamic key variable containing dots should be treated literally
        var script = @"
            var map = {}
            var email = ""user.name@company.org""
            map[email] = ""User Name""
            Data.result = map[email]
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("User Name", ((JyroString)data.GetPropertyLiteral("result")).Value);
    }

    [Fact]
    public void BracketNotation_ArrayValueWithDottedKey()
    {
        // Should be able to store arrays under dotted keys
        var script = @"
            var map = {}
            var key = ""category.subcategory""
            map[key] = []
            Append(map[key], ""item1"")
            Append(map[key], ""item2"")
            Data.count = Length(map[key])
            Data.first = map[key][0]
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(2.0, ((JyroNumber)data.GetPropertyLiteral("count")).Value);
        Assert.Equal("item1", ((JyroString)data.GetPropertyLiteral("first")).Value);
    }

    [Fact]
    public void BracketNotation_DistinguishFromDotNotation()
    {
        // Bracket notation with "a.b" should be different from dot notation a.b
        var script = @"
            var obj = {}
            obj.a = {}
            obj.a.b = ""nested""
            obj[""a.b""] = ""literal""
            Data.nestedValue = obj.a.b
            Data.literalValue = obj[""a.b""]
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("nested", ((JyroString)data.GetPropertyLiteral("nestedValue")).Value);
        Assert.Equal("literal", ((JyroString)data.GetPropertyLiteral("literalValue")).Value);
    }

    [Fact]
    public void BracketNotation_MultipleDotsInKey()
    {
        // Keys with multiple dots should work
        var script = @"
            var obj = {}
            obj[""a.b.c.d""] = ""deep""
            Data.result = obj[""a.b.c.d""]
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("deep", ((JyroString)data.GetPropertyLiteral("result")).Value);
    }

    #endregion
}
