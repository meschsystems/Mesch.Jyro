# Mesch.Jyro

Jyro is a secure, sandboxed imperative scripting language designed for data transformation and ETL operations on JSON-like data structures. Built for .NET 8+, it provides a safe runtime environment for executing user-provided scripts with comprehensive resource limits and fail-fast error handling.

## Features

- **Imperative Programming Model**: Familiar syntax with variables, loops, conditionals, and functions
- **Secure Sandboxing**: Built-in resource limits (execution time, statement count, stack depth)
- **Fail-Fast Error Handling**: Runtime errors are caught immediately to prevent data corruption
- **Rich Standard Library**: String manipulation, array operations, math functions, date/time utilities
- **Extensible**: Add custom host functions to expose your application's functionality to scripts
- **ANTLR-Powered**: Fast parsing with clear error messages
- **Strongly-Typed Runtime**: Type-safe execution with clear error messages

## Installation

```bash
dotnet add package Mesch.Jyro
```

## Quick Start

The following sections will assist in getting Jyro up and running in a host environment. The full language reference is available at https://docs.mesch.cloud/jyro/

### Basic Usage

```csharp
using Mesch.Jyro;
using Microsoft.Extensions.Logging.Abstractions;

// Create input data
var data = new JyroObject();
data.SetProperty("name", new JyroString("Alice"));
data.SetProperty("age", new JyroNumber(25));

// Define a script
var script = @"
    Data.greeting = 'Hello, ' + Data.name + '!'
    Data.canVote = Data.age >= 18
";

// Execute the script
var result = JyroBuilder
    .Create(NullLoggerFactory.Instance)
    .WithScript(script)
    .WithData(data)
    .WithStandardLibrary()
    .Run();

// Access results
if (result.IsSuccessful)
{
    var outputData = (JyroObject)result.Data;
    var greeting = ((JyroString)outputData.GetProperty("greeting")).Value;
    Console.WriteLine(greeting); // "Hello, Alice!"
}
else
{
    foreach (var message in result.Messages)
    {
        Console.WriteLine($"[{message.Severity}] {message.Code}");
    }
}
```

### Working with JSON

Jyro provides seamless JSON integration for real-world scenarios:

```csharp
using System.Text.Json;

// Parse JSON input directly
var inputJson = @"{
    ""customer"": {
        ""name"": ""Alice"",
        ""age"": 25,
        ""orders"": [
            { ""id"": 1, ""total"": 150.00 },
            { ""id"": 2, ""total"": 75.50 }
        ]
    }
}";

var data = JyroValue.FromJson(inputJson);

var script = @"
    var totalSpent = 0
    foreach order in Data.customer.orders do
        totalSpent = totalSpent + order.total
    end
    Data.customer.totalSpent = totalSpent
";

var result = JyroBuilder
    .Create(NullLoggerFactory.Instance)
    .WithScript(script)
    .WithData(data)
    .WithStandardLibrary()
    .Run();

// Serialize result back to JSON
var outputJson = result.Data.ToJson(new JsonSerializerOptions
{
    WriteIndented = true
});
Console.WriteLine(outputJson);
// {
//   "customer": {
//     "name": "Alice",
//     "age": 25,
//     "orders": [...],
//     "totalSpent": 225.5
//   }
// }
```

### Working with .NET Objects

Convert .NET objects to JyroValue and back:

```csharp
// From .NET object to JyroValue
var customer = new
{
    Name = "Bob",
    Age = 30,
    IsActive = true,
    Tags = new[] { "premium", "verified" }
};

var data = JyroValue.FromObject(customer);

// Execute script
var result = JyroBuilder
    .Create(NullLoggerFactory.Instance) 
    .WithScript("Data.displayName = Upper(Data.Name)")
    .WithData(data)
    .WithStandardLibrary()
    .Run();

// Convert back to .NET object
var outputObject = result.Data.ToObjectValue();
// outputObject is a Dictionary<string, object?> with all properties
```

## Supported Scenarios

### 1. Data Transformation & ETL

Transform raw data into structured output with business logic:

```csharp
var script = @"
    Data.orders = []

    foreach order in Data.rawOrders do
        var discount = 0
        var shippingCost = 10

        # Apply volume discount
        if order.quantity >= 10 then
            discount = 0.15
        else if order.quantity >= 5 then
            discount = 0.10
        end

        # Free shipping for large orders
        if order.total >= 100 then
            shippingCost = 0
        end

        # Calculate final price
        var subtotal = order.total * (1 - discount)
        var finalPrice = subtotal + shippingCost

        Append(Data.orders, {
            'orderId': order.id,
            'originalPrice': order.total,
            'discount': discount * 100,
            'shippingCost': shippingCost,
            'finalPrice': finalPrice
        })
    end
";

var inputData = new JyroObject();
var rawOrders = new JyroArray();
// ... populate rawOrders with order data
inputData.SetProperty("rawOrders", rawOrders);

var result = JyroBuilder
    .Create(NullLoggerFactory.Instance)  
    .WithScript(script)
    .WithData(inputData)
    .WithStandardLibrary()
    .Run();
```

### 2. Business Rule Evaluation

Execute user-defined business rules safely:

```csharp
var ruleScript = @"
    # Calculate risk score based on multiple factors
    var riskScore = 0

    if Data.customer.paymentHistory == 'poor' then
        riskScore = riskScore + 30
    end

    if Data.order.amount > 10000 then
        riskScore = riskScore + 20
    end

    if Data.customer.accountAge < 30 then
        riskScore = riskScore + 15
    end

    Data.riskScore = riskScore
    Data.requiresApproval = riskScore > 50
";

var result = JyroBuilder
    .Create(loggerFactory)
    .WithScript(ruleScript)
    .WithData(customerData)
    .WithStandardLibrary()
    .WithOptions(new JyroExecutionOptions
    {
        MaxExecutionTime = TimeSpan.FromSeconds(2),
        MaxStatements = 5_000
    })
    .Run();
```

### 3. Custom Host Functions

Extend Jyro with your application's functionality:

```csharp
// Define a custom function
public class SendEmailFunction : JyroFunctionBase
{
    private readonly IEmailService _emailService;

    public SendEmailFunction(IEmailService emailService)
        : base(new JyroFunctionSignature(
            "SendEmail",
            [
                new Parameter("recipient", ParameterType.String),
                new Parameter("subject", ParameterType.String),
                new Parameter("body", ParameterType.String)
            ],
            ParameterType.Boolean))
    {
        _emailService = emailService;
    }

    public override JyroValue Execute(
        IReadOnlyList<JyroValue> arguments,
        ExecutionContext executionContext)
    {
        var recipient = GetArgument<JyroString>(arguments, 0).Value;
        var subject = GetArgument<JyroString>(arguments, 1).Value;
        var body = GetArgument<JyroString>(arguments, 2).Value;

        var success = _emailService.SendEmail(recipient, subject, body);
        return JyroBoolean.FromBoolean(success);
    }
}

// Use the custom function
var script = @"
    if Data.sendNotification == true then
        var sent = SendEmail(Data.email, 'Order Confirmation', Data.message)
        Data.emailSent = sent
    end
";

var result = JyroBuilder
    .Create(loggerFactory)
    .WithScript(script)
    .WithData(data)
    .WithStandardLibrary()
    .WithFunction(new SendEmailFunction(emailService))
    .Run();
```

### 4. Dynamic Configuration Processing

Load and process configuration with scripts:

```csharp
var configScript = @"
    # Transform configuration values
    Data.processedConfig = {
        'apiUrl': Data.rawConfig.baseUrl + '/api/v1',
        'timeout': Data.rawConfig.timeoutSeconds * 1000,
        'retries': Data.rawConfig.maxRetries,
        'enableCache': Data.rawConfig.cacheEnabled == true
    }

    # Validate configuration
    if Length(Data.processedConfig.apiUrl) == 0 then
        Data.errors = ['API URL is required']
    end
";
```

### 5. REST API Integration

Use Jyro in ASP.NET Core API endpoints:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ScriptController : ControllerBase
{
    private readonly IJyroScriptService _scriptService;

    public ScriptController(IJyroScriptService scriptService)
    {
        _scriptService = scriptService;
    }

    [HttpPost("execute")]
    public IActionResult ExecuteScript([FromBody] ScriptRequest request)
    {
        try
        {
            // Parse JSON input
            var inputData = JyroValue.FromJson(request.Data);

            // Execute script
            var result = _scriptService.ExecuteScript(request.Script, inputData);

            if (!result.IsSuccessful)
            {
                return BadRequest(new
                {
                    success = false,
                    errors = result.Messages
                        .Where(m => m.Severity == MessageSeverity.Error)
                        .Select(m => new { m.Code, Line = m.LineNumber, Column = m.ColumnPosition })
                });
            }

            // Return JSON output
            var outputJson = result.Data.ToJson();
            return Ok(new
            {
                success = true,
                data = JsonDocument.Parse(outputJson),
                executionTime = result.Metadata.ProcessingTime  // Note: ProcessingTime, not ExecutionTime
            });
        }
        catch (JsonException ex)
        {
            return BadRequest(new { success = false, error = "Invalid JSON: " + ex.Message });
        }
    }
}

public record ScriptRequest(string Script, string Data);
```

## Dependency Injection Integration

### ASP.NET Core

Register Jyro services in your application:

```csharp
// Program.cs or Startup.cs
services.AddSingleton<IJyroScriptService, JyroScriptService>();

// JyroScriptService.cs
public interface IJyroScriptService
{
    JyroExecutionResult ExecuteScript(string script, JyroValue data);
}

public class JyroScriptService : IJyroScriptService
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly JyroExecutionOptions _defaultOptions;

    public JyroScriptService(ILoggerFactory loggerFactory, IOptions<JyroExecutionOptions> options)
    {
        _loggerFactory = loggerFactory;
        _defaultOptions = options.Value;
    }

    public JyroExecutionResult ExecuteScript(string script, JyroValue data)
    {
        return JyroBuilder
            .Create(_loggerFactory)
            .WithScript(script)
            .WithData(data)
            .WithStandardLibrary()
            .WithOptions(_defaultOptions)
            .Run();
    }
}

// Configure options
services.Configure<JyroExecutionOptions>(options =>
{
    options.MaxExecutionTime = TimeSpan.FromSeconds(10);
    options.MaxStatements = 50_000;
    options.MaxLoops = 5_000;
});
```

### Scoped Execution with Cancellation

```csharp
public async Task<JyroExecutionResult> ExecuteWithCancellationAsync(
    string script,
    JyroValue data,
    CancellationToken cancellationToken)
{
    return await Task.Run(() =>
    {
        return JyroBuilder
            .Create(_loggerFactory)
            .WithScript(script)
            .WithData(data)
            .WithStandardLibrary()
            .Run(cancellationToken);
    }, cancellationToken);
}
```

## Configuration Options

### JyroExecutionOptions

Control resource limits and runtime behavior:

```csharp
var options = new JyroExecutionOptions
{
    // Maximum wall-clock execution time (default: 5 seconds)
    MaxExecutionTime = TimeSpan.FromSeconds(10),

    // Maximum statements executed (default: 10,000)
    MaxStatements = 50_000,

    // Maximum loop iterations (default: 1,000)
    MaxLoops = 5_000,

    // Maximum expression stack depth (default: 256)
    MaxStackDepth = 512,

    // Maximum function call depth (default: 64)
    MaxCallDepth = 128,

    // Maximum script-to-script call depth (default: 5)
    MaxScriptCallDepth = 10
};
```

## API Reference

### Core Types

#### JyroExecutionResult

The result of executing a Jyro script.

**Properties:**
- `bool IsSuccessful` - Whether execution completed successfully without fatal errors
- `JyroValue Data` - Final state of the root data object after execution
- `IReadOnlyList<IMessage> Messages` - Diagnostic messages (errors, warnings, info)
- `ExecutionMetadata Metadata` - Execution statistics and timing information
- `int ErrorCount` - Number of error-level messages (computed property)

**Constructor:**
```csharp
public JyroExecutionResult(
    bool executionSucceeded,
    JyroValue finalDataValue,
    IReadOnlyList<IMessage> diagnosticMessages,
    ExecutionMetadata executionMetadata)
```

#### ExecutionMetadata

Contains performance and statistical information about script execution.

**Properties:**
- `TimeSpan ProcessingTime` - Total execution time ⚠️ **Note:** Named `ProcessingTime`, not `ExecutionTime`
- `int StatementCount` - Number of statements executed
- `int LoopCount` - Total loop iterations performed
- `int FunctionCallCount` - Number of function calls made
- `int MaxCallDepth` - Maximum call stack depth reached
- `DateTimeOffset StartedAt` - When execution began

**Constructor:**
```csharp
public ExecutionMetadata(
    TimeSpan executionProcessingTime,
    int executedStatementCount,
    int executedLoopCount,
    int performedFunctionCallCount,
    int maximumCallDepth,
    DateTimeOffset executionStartedAt)
```

#### IMessage

Diagnostic message interface for compilation and runtime errors.

**Properties:**
- `MessageCode Code` - Error code (enum, not string!)
- `MessageSeverity Severity` - Error, Warning, or Information
- `ProcessingStage Stage` - Which pipeline stage produced the message
- `int LineNumber` - Source line number (1-based)
- `int ColumnPosition` - Source column position (1-based)
- `IReadOnlyList<string> Arguments` - Message arguments for formatting

⚠️ **Important:** `IMessage` does **not** have a `Text` or `Message` property. Error details are conveyed through the `Code` enum and `Arguments` array.

**Implementation (Message class):**
```csharp
public Message(
    MessageCode code,
    int lineNumber,
    int columnPosition,
    MessageSeverity severity,
    ProcessingStage stage,
    params string[] arguments)
```

#### MessageCode Enum

Standardized diagnostic codes organized by processing stage:

**Lexical Analysis (1000-1999):**
- `UnknownLexerError` (1000)
- `UnexpectedCharacter` (1001)
- `UnterminatedString` (1002)

**Parsing (2000-2999):**
- `UnknownParserError` (2000)
- `UnexpectedToken` (2001)
- `MissingToken` (2002)
- `InvalidNumberFormat` (2003)

**Validation (3000-3999):**
- `UnknownValidatorError` (3000)
- `InvalidVariableReference` (3001)
- `InvalidAssignmentTarget` (3002)
- `TypeMismatch` (3003)
- `LoopStatementOutsideOfLoop` (3004)
- `ExcessiveLoopNesting` (3005)
- `UnreachableCode` (3006)

**Linking (4000-4999):**
- `UnknownLinkerError` (4000)
- `UndefinedFunction` (4001)
- `DuplicateFunction` (4002)
- `FunctionOverride` (4003)
- `InvalidNumberArguments` (4004)

**Execution (5000-5999):**
- `UnknownExecutorError` (5000)
- `RuntimeError` (5001)
- `CancelledByHost` (5002)
- `InvalidType` (5003)
- `InvalidArgumentType` (5004)

#### MessageSeverity Enum

- `Error` - Fatal errors that prevent execution
- `Warning` - Non-fatal issues
- `Information` - Informational messages

#### ProcessingStage Enum

- `Lexing` - Tokenization stage
- `Parsing` - Syntax analysis stage
- `Validation` - Semantic analysis stage
- `Linking` - Reference resolution stage
- `Execution` - Runtime stage

### JyroValue Types

#### JyroNull

Represents null values. Uses the **singleton pattern**.

```csharp
// ✅ Correct - use the singleton instance
var nullValue = JyroNull.Instance;

// ❌ Wrong - constructor is private
var nullValue = new JyroNull();
```

**Properties:**
- `JyroValueType Type` - Always returns `JyroValueType.Null`
- `bool IsNull` - Always returns `true`

#### JyroString

Represents string values.

```csharp
var str = new JyroString("Hello");
string value = str.Value;
```

#### JyroNumber

Represents numeric values (stored as `decimal`).

```csharp
var num = new JyroNumber(42.5m);
decimal value = num.Value;
```

#### JyroBoolean

Represents boolean values.

```csharp
var boolean = JyroBoolean.True;   // Singleton for true
var boolean = JyroBoolean.False;  // Singleton for false

// Or construct from bool
var boolean = JyroBoolean.FromBoolean(true);
```

#### JyroObject

Represents objects (key-value pairs).

```csharp
var obj = new JyroObject();
obj.SetProperty("name", new JyroString("Alice"));
obj.SetProperty("age", new JyroNumber(25));

// Get property
JyroValue name = obj.GetProperty("name");  // Returns JyroNull.Instance if not found

// Check if object has properties
int propertyCount = obj.Count;  // ⚠️ Use Count, not GetProperties()

// Indexer access
obj["name"] = new JyroString("Bob");
JyroValue name = obj["name"];
```

#### JyroArray

Represents arrays.

```csharp
var arr = new JyroArray();
arr.Add(new JyroNumber(1));
arr.Add(new JyroNumber(2));

// Access by index
JyroValue first = arr[0];

// Array properties
int length = arr.Count;
```

### JyroBuilder

Fluent API for building and executing Jyro scripts.

```csharp
var result = JyroBuilder
    .Create(loggerFactory)           // Required: ILoggerFactory
    .WithScript(scriptSource)        // Required: Script source code
    .WithData(dataObject)            // Required: Input data
    .WithStandardLibrary()           // Optional: Include standard functions
    .WithFunction(customFunction)    // Optional: Add custom functions
    .WithOptions(executionOptions)   // Optional: Configure resource limits
    .Run(cancellationToken);         // Optional: CancellationToken
```

**Important:** `JyroBuilder.Create()` requires an `ILoggerFactory` parameter. Use `NullLoggerFactory.Instance` for no logging.

### Creating Error Results

When manually creating error results (e.g., for script-not-found scenarios):

```csharp
private JyroExecutionResult CreateErrorResult(string errorMessage)
{
    return new JyroExecutionResult(
        false,                          // executionSucceeded
        JyroNull.Instance,              // finalDataValue (use singleton!)
        new List<IMessage>
        {
            new Message(
                MessageCode.RuntimeError,   // code (enum!)
                0,                          // lineNumber
                0,                          // columnPosition
                MessageSeverity.Error,      // severity
                ProcessingStage.Execution,  // stage
                errorMessage)               // arguments (params string[])
        },
        new ExecutionMetadata(
            TimeSpan.Zero,              // processingTime
            0,                          // statementCount
            0,                          // loopCount
            0,                          // functionCallCount
            0,                          // maxCallDepth
            DateTimeOffset.UtcNow));    // startedAt
}
```

## Standard Library Functions

### String Functions
- `Upper(str)` - Convert to uppercase
- `Lower(str)` - Convert to lowercase
- `Trim(str)` - Remove leading/trailing whitespace
- `Replace(str, search, replace)` - Replace text
- `Contains(str, search)` - Check if string contains substring
- `StartsWith(str, prefix)` - Check if string starts with prefix
- `EndsWith(str, suffix)` - Check if string ends with suffix
- `Split(str, delimiter)` - Split string into array
- `Join(arr, separator)` - Join array elements into string

### Array Functions
- `Length(arr)` - Get array length
- `Append(arr, value)` - Add element to end
- `IndexOf(arr, value)` - Find index of element using deep equality
- `Insert(arr, index, value)` - Insert element at index
- `RemoveAt(arr, index)` - Remove element at index
- `RemoveLast(arr)` - Remove last element
- `Clear(arr)` - Remove all elements
- `Sort(arr)` - Sort array in-place (ascending)
- `SortByField(arr, field, direction)` - Sort array of objects by field
- `Reverse(arr)` - Reverse array in-place
- `MergeArrays(arr1, arr2)` - Combine two arrays

### Math Functions
- `Min(a, b)` - Return minimum value
- `Max(a, b)` - Return maximum value
- `Sum(a, b, c, ...)` - Sum multiple values
- `Abs(num)` - Absolute value
- `Round(num, decimals)` - Round to decimal places

### Utility Functions
- `TypeOf(value)` - Get type name as string
- `IsNull(value)` - Check if value is null
- `Exists(obj, property)` - Check if property exists
- `Equal(a, b)` - Deep equality comparison
- `NotEqual(a, b)` - Deep inequality comparison
- `NewGuid()` - Generate new GUID string
- `InvokeRestMethod(url, method, options)` - Execute HTTP REST API requests (experimental)

### Date/Time Functions
- `Now()` - Current date and time
- `Today()` - Current date (midnight)
- `ParseDate(str)` - Parse ISO 8601 date string
- `FormatDate(date, format)` - Format date to string
- `DateAdd(date, days)` - Add days to date
- `DateDiff(date1, date2)` - Get difference in days
- `DatePart(date, part)` - Extract part ('year', 'month', 'day', 'hour', 'minute', 'second')

## Language Syntax

### Variables
```jyro
var name = "Alice"
var age = 25
var isActive = true
```

### Conditionals
```jyro
if age >= 18 then
    Data.status = "adult"
else if age >= 13 then
    Data.status = "teen"
else
    Data.status = "child"
end
```

### Loops
```jyro
# For loop
for i = 1 to 10 do
    Data.sum = Data.sum + i
end

# While loop
while count < 100 do
    count = count + 1
end

# Foreach loop
foreach item in Data.items do
    Data.total = Data.total + item.price
end
```

### Objects and Arrays
```jyro
# Create object
var person = {
    "name": "Bob",
    "age": 30,
    "active": true
}

# Create array
var numbers = [1, 2, 3, 4, 5]

# Access properties
var personName = person.name
var firstNumber = numbers[0]
```

### Logical Operators
```jyro
# Use word-based operators
if isActive and age > 18 then
    # ...
end

if not isDeleted or isArchived then
    # ...
end
```

## Error Handling

Jyro uses fail-fast error handling to prevent data corruption:

```csharp
var result = JyroBuilder
    .Create(loggerFactory)  
    .WithScript(script)
    .WithData(data)
    .WithStandardLibrary()
    .Run();

if (!result.IsSuccessful)
{
    foreach (var message in result.Messages)
    {
        if (message.Severity == MessageSeverity.Error)
        {
            _logger.LogError(
                "Jyro execution failed: {Code} at {Line}:{Column}",
                message.Code,
                message.LineNumber,
                message.ColumnPosition);

            // Error details are in Code (enum) and Arguments (string array)
            if (message.Arguments.Any())
            {
                _logger.LogError("  Arguments: {Arguments}", string.Join(", ", message.Arguments));
            }
        }
    }

    // Handle failure appropriately
    return BadRequest(result.Messages);
}

// Process successful result
var outputData = result.Data;
```

### Common Runtime Errors

- **Division by zero**: Throws immediately
- **Null property access**: `obj.property` on null throws error
- **Array index out of bounds**: Both negative and out-of-range indexes throw
- **Timeout**: Script execution exceeds `MaxExecutionTime`
- **Resource limits**: Exceeds `MaxStatements`, `MaxLoops`, or `MaxStackDepth`

## Performance Considerations

- **Parsing**: Parse trees are generated on each execution. For frequently-used scripts, consider caching compiled results
- **Resource Limits**: Tune `JyroExecutionOptions` based on your use case
- **Standard Library**: Only include `WithStandardLibrary()` if needed
- **Custom Functions**: Keep host function implementations fast to avoid blocking script execution
- **Logging**: Use `NullLoggerFactory.Instance` in production for maximum performance

## Security

Jyro is designed for safe execution of untrusted scripts:

- ✅ **Sandboxed execution** - No file system, network, or system access
- ✅ **Resource limits** - Prevent infinite loops and resource exhaustion
- ✅ **Type safety** - Strong typing prevents type confusion attacks
- ✅ **No reflection** - Scripts cannot access .NET reflection APIs
- ✅ **Fail-fast** - Runtime errors stop execution immediately

**Important**: Host functions are the primary extension point. Ensure your custom functions validate inputs and don't expose sensitive functionality.

## License

Copyright © Mesch Systems

## Support

For issues, feature requests, or questions, please contact Mesch Systems or open an issue in the repository.
