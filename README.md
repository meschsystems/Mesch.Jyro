# Mesch.Jyro

Jyro is a secure, sandboxed scripting language for .NET 8+ that lets you safely execute user-provided scripts while enforcing strict isolation that keeps the host safe from buggy, malicious or runaway code. With configurable resource limits, fail-fast error handling, and linear, predictable, run-to-completion execution, Jyro delivers real-world scripting capability without compromise.

## Features

- **Imperative Programming Model**: Familiar syntax with variables, loops, conditionals, and functions
- **Secure Sandboxing**: Built-in resource limits (execution time, statement count, stack depth)
- **Fail-Fast Error Handling**: Runtime errors are caught immediately to prevent data corruption
- **Rich Standard Library**: Nearly 80 standard library functions covering string manipulation, array operations, math functions, date/time and schema validation
- **Extensible**: Add custom host functions to expose your application's functionality to scripts
- **ANTLR-Powered**: Fast parsing with clear error messages
- **Strongly-Typed Runtime**: Type-safe execution with clear error messages
- **VS Code Extension**: Full language support with syntax highlighting, error checking, and IntelliSense

## First Look

Here's a sample Jyro script that demonstrates:

- Variable declarations
- String concatenation and operations
- Property access with dot notation
- Basic conditionals
- Simple array manipulation

```jyro
# Create a personalized greeting
var greeting = "Hello, " + Data.name + "!"
Data.greeting = greeting

# Build location string
var fullLocation = Data.city + ", " + Data.country
Data.fullLocation = fullLocation

# Check if person is an adult
if Data.age >= 18 then
    Data.isAdult = true
    Data.ageCategory = "Adult"
else
    Data.isAdult = false
    Data.ageCategory = "Minor"
end

# Create a fun fact using string concatenation
var funFact = Data.name + " is " + Data.age + " years old and loves the color " + Data.favoriteColor
Data.funFact = funFact

# Process hobbies - convert to uppercase
Data.processedHobbies = []
foreach hobby in Data.hobbies do
    var upperHobby = Upper(hobby)
    Append(Data.processedHobbies, upperHobby)
end

# Count hobbies
Data.hobbyCount = Length(Data.hobbies)
```

Given this JSON input

```json
{
  "name": "Alice Johnson",
  "age": 28,
  "city": "Seattle",
  "country": "USA",
  "favoriteColor": "blue",
  "hobbies": [
    "reading",
    "gaming",
    "cooking"
  ]
}
```

The script produces

```json
{
  "name": "Alice Johnson",
  "age": 28,
  "city": "Seattle",
  "country": "USA",
  "favoriteColor": "blue",
  "hobbies": [
    "reading",
    "gaming",
    "cooking"
  ],
  "greeting": "Hello, Alice Johnson!",
  "fullLocation": "Seattle, USA",
  "isAdult": true,
  "ageCategory": "Adult",
  "funFact": "Alice Johnson is 28 years old and loves the color blue",
  "processedHobbies": [
    "READING",
    "GAMING",
    "COOKING"
  ],
  "hobbyCount": 3
}
```

## Installation

```bash
dotnet add package Mesch.Jyro
```

## Resources

| Resource | Link |
|----------|------|
| Homepage | https://www.jyro.dev/ |
| Documentation | https://docs.mesch.cloud/jyro/ |
| NuGet Package | https://www.nuget.org/packages/Mesch.Jyro/ |
| VS Code Extension | https://marketplace.visualstudio.com/items?itemName=meschsystems.jyro-vscode |
| Online Playpen | https://playpen.jyro.dev/ |
| GitHub Repository | https://github.com/meschsystems/Mesch.Jyro |
| CLI Tool | https://github.com/meschsystems/Mesch.Jyro.Cli |
| Demo Web Server | https://github.com/meschsystems/Mesch.JyroWebServer |

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
    Data.greeting = ""Hello, "" + Data.name + ""!""
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

// We can get inputJson from anywhere - file, database, REST API etc.
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

## Typical Scenarios

The following sections demonstrate Jyro's key capabilities and versatility.

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
        elseif order.quantity >= 5 then
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
            ""orderId"": order.id,
            ""originalPrice"": order.total,
            ""discount"": discount * 100,
            ""shippingCost"": shippingCost,
            ""finalPrice"": finalPrice
        })
    end
";

var inputData = new JyroObject();
var rawOrders = new JyroArray();

// Sample order 1: Small order (no discount, standard shipping)
var order1 = new JyroObject();
order1.SetProperty("id", new JyroString("ORD-001"));
order1.SetProperty("quantity", new JyroNumber(2));
order1.SetProperty("total", new JyroNumber(45.00));
rawOrders.Add(order1);

// Sample order 2: Medium order (10% discount, standard shipping)
var order2 = new JyroObject();
order2.SetProperty("id", new JyroString("ORD-002"));
order2.SetProperty("quantity", new JyroNumber(7));
order2.SetProperty("total", new JyroNumber(85.00));
rawOrders.Add(order2);

// Sample order 3: Large order (15% discount, free shipping)
var order3 = new JyroObject();
order3.SetProperty("id", new JyroString("ORD-003"));
order3.SetProperty("quantity", new JyroNumber(12));
order3.SetProperty("total", new JyroNumber(250.00));
rawOrders.Add(order3);

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
// {
//  "customer": {
//    "paymentHistory": "poor",
//    "accountAge": 20
//  },
//  "order": {
//    "amount": 100
//  }
// }

var ruleScript = @"
    # Calculate risk score based on multiple factors
    var riskScore = 0

    if Data.customer.paymentHistory == ""poor"" then
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
        JyroExecutionContext executionContext)
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
        var sent = SendEmail(Data.email, ""Order Confirmation"", Data.message)
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

#### Per-Execution State with FunctionState

Custom functions may need to maintain state across multiple calls within a single script execution (e.g., caching, counters, sequence generation). The `JyroExecutionContext.FunctionState` dictionary provides tenant-isolated, per-execution state storage. State is isolated to a single script execution, preventing cross-tenant data leakage and eliminating timing side-channels that could occur with shared state. Since Jyro execution is single-threaded per context, no locking is needed when accessing `FunctionState`.

**Example - Caching expensive lookups:**

```csharp
public class GetUserFunction : JyroFunctionBase
{
    private const string CacheKey = "GetUser.Cache";
    private readonly IUserRepository _repository;

    public GetUserFunction(IUserRepository repository) : base(new JyroFunctionSignature(
        "GetUser",
        [new Parameter("userId", ParameterType.String)],
        ParameterType.Object))
    {
        _repository = repository;
    }

    public override JyroValue Execute(
        IReadOnlyList<JyroValue> arguments,
        JyroExecutionContext executionContext)
    {
        var userId = GetStringArgument(arguments, 0);

        // Get or create cache dictionary for this execution
        if (!executionContext.FunctionState.TryGetValue(CacheKey, out var cacheObj))
        {
            cacheObj = new Dictionary<string, JyroObject>();
            executionContext.FunctionState[CacheKey] = cacheObj;
        }
        var cache = (Dictionary<string, JyroObject>)cacheObj;

        // Return cached result if available
        if (cache.TryGetValue(userId, out var cachedUser))
        {
            return cachedUser;
        }

        // Fetch from repository (expensive operation)
        var user = _repository.GetUser(userId);
        var result = ConvertToJyroObject(user);

        // Cache for subsequent calls within this execution
        cache[userId] = result;

        return result;
    }
}
```

This cache is fresh for each script execution — no stale data across requests, no cross-tenant leakage.

**Key prefix convention**: Use `"FunctionName.StateKey"` format to avoid collisions between different functions (e.g., `"GetUser.Cache"`).

### 4. Plugin Architecture - Loading Functions from DLLs

Load custom Jyro functions dynamically from external assemblies at runtime, enabling a plugin-based architecture:

```csharp
// Create a plugin DLL with custom functions
public class GreetFunction : JyroFunctionBase
{
    public GreetFunction() : base(new JyroFunctionSignature(
        "Greet",
        new[] { new Parameter("name", ParameterType.String) },
        ParameterType.String))
    {
    }

    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var name = GetStringArgument(arguments, 0);
        return new JyroString($"Hello, {name}! Welcome to Jyro plugins!");
    }
}
```

```csharp
// Load from a single DLL file
var pluginPath = "path/to/MyPlugins.dll";
var result = JyroBuilder
    .Create(loggerFactory)
    .WithScript("Data.greeting = Greet(\"World\")")
    .WithData(data)
    .WithStandardLibrary()
    .WithFunctionsFromAssemblyPath(pluginPath)
    .Run();

// Or load all plugins from a directory
var pluginDirectory = "path/to/Plugins";
var result = JyroBuilder
    .Create(loggerFactory)
    .WithScript("Data.result = CustomFunction()")
    .WithData(data)
    .WithFunctionsFromDirectory(pluginDirectory)
    .Run();

// Load with custom search pattern
var result = JyroBuilder
    .Create(loggerFactory)
    .WithScript("Data.value = ProcessData()")
    .WithData(data)
    .WithFunctionsFromDirectory("C:\\Plugins", "*.JyroPlugin.dll", SearchOption.AllDirectories)
    .Run();
```

**Key features:**
- Automatically discovers all `IJyroFunction` implementations in assemblies
- Three loading methods: single file (`WithFunctionsFromAssemblyPath`), loaded assembly (`WithFunctionsFromAssembly`), or directory (`WithFunctionsFromDirectory`)
- Directory loading supports custom search patterns and recursive subdirectory search
- Combines seamlessly with standard library and manually added functions
- Supports compile-once, execute-many pattern for cached plugins
- Gracefully handles mixed assemblies (skips types without parameterless constructors when loading from directory)

**Requirements:**
- Plugin functions must have public parameterless constructors
- Plugin assemblies must reference the Mesch.Jyro package
- All function types must implement `IJyroFunction` (typically by inheriting from `JyroFunctionBase`)

See the `Mesch.Jyro.PluginExample` project for complete working examples including `ReverseString`, `Multiply`, and `Greet` functions.

### 5. Dynamic Configuration Processing

Load and process configuration with scripts:

```csharp
var configScript = @"
    # Transform configuration values
    Data.processedConfig = {
        ""apiUrl"": Data.rawConfig.baseUrl + ""/api/v1"",
        ""timeout"": Data.rawConfig.timeoutSeconds * 1000,
        ""retries"": Data.rawConfig.maxRetries,
        ""enableCache"": Data.rawConfig.cacheEnabled == true
    }

    # Validate configuration
    if Length(Data.processedConfig.apiUrl) == 0 then
        Data.errors = [""API URL is required""]
    end
";
```

### 6. REST API Integration

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
                executionTime = result.Metadata.ProcessingTime
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

### Script Resolution

Jyro provides a `JyroScriptResolver` that can allow scripts to call other scripts by name.

```csharp
var result = JyroBuilder.Create()
    .WithScript("var result = CallScriptByName(\"validate-customer\", Data)")
    .WithData(customerData)
    .WithStdlib()
    .WithResolver(name =>
    {
        // Return script source for the given name, or null if not found
        return name switch
        {
            "validate-customer" => "Data.valid = Data.age >= 18 and Data.status == \"active\"",
            "calculate-total" => "Data.total = Data.price * Data.quantity",
            "apply-discount" => "Data.finalPrice = Data.total * (1 - Data.discountRate)",
            _ => null
        };
    })
    .Run();
```

The resolver is a delegate of type `JyroScriptResolver`:

```csharp
public delegate string? JyroScriptResolver(string name);
```

Common resolver patterns:

```csharp
// File-based resolver
.WithResolver(name => File.Exists($"scripts/{name}.jyro")
    ? File.ReadAllText($"scripts/{name}.jyro")
    : null)

// Dictionary-based resolver
var scripts = new Dictionary<string, string>
{
    ["validate"] = "Data.valid = Data.value > 0",
    ["transform"] = "Data.result = Upper(Data.input)"
};
.WithResolver(name => scripts.TryGetValue(name, out var source) ? source : null)

// Database resolver (example pattern)
.WithResolver(name => scriptRepository.GetScriptByName(name))
```

If `CallScriptByName()` is called without a resolver configured, or if the resolver returns `null` for the script name, a runtime error is thrown.

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
- `TimeSpan ProcessingTime` - Total execution time
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

Jyro has a standardized set of diagnostic codes organized by processing stage:

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
- `UndeclaredVariable` (3001)
- `InvalidAssignmentTarget` (3002)
- `TypeMismatch` (3003)
- `LoopStatementOutsideOfLoop` (3004)
- `ExcessiveLoopNesting` (3005)
- `UnreachableCode` (3006)
- `ReservedIdentifier` (3007)
- `VariableAlreadyDeclared` (3008)

**Linking (4000-4999):**
- `UnknownLinkerError` (4000)
- `UndefinedFunction` (4001)
- `DuplicateFunction` (4002)
- `FunctionOverride` (4003)
- `InvalidNumberArguments` (4004)
- `TooFewArguments` (4005)
- `TooManyArguments` (4006)

**Execution (5000-5999):**

*General Execution Errors (5000-5099):*
- `ScriptReturn` (5000) - Script explicitly called `return` with a message
- `UnknownExecutorError` (5001)
- `RuntimeError` (5002)
- `CancelledByHost` (5003)
- `InvalidType` (5004)
- `InvalidArgumentType` (5005)

*Arithmetic Errors (5100-5199):*
- `DivisionByZero` (5100)
- `NegateNonNumber` (5101)
- `IncrementDecrementNonNumber` (5102)
- `IncompatibleOperandTypes` (5103)
- `IncompatibleComparison` (5104)

*Collection Access Errors (5200-5299):*
- `IndexOutOfRange` (5200)
- `NegativeIndex` (5201)
- `IndexAccessOnNull` (5202)
- `InvalidIndexTarget` (5203)
- `PropertyAccessOnNull` (5204)
- `PropertyAccessInvalidType` (5205)

*Type Errors (5300-5399):*
- `InvalidTypeCheck` (5300)
- `UnknownTypeName` (5301)
- `NotIterable` (5302)

*Function Errors (5400-5499):*
- `UndefinedFunctionRuntime` (5400)
- `InvalidFunctionTarget` (5401)

*Internal/Syntax Errors (5500-5599):*
- `InvalidExpressionSyntax` (5500)
- `UnknownOperator` (5501)

*Resource Limit Errors (5600-5699):*
- `StatementLimitExceeded` (5600)
- `LoopIterationLimitExceeded` (5601)
- `CallDepthLimitExceeded` (5602)
- `ScriptCallDepthLimitExceeded` (5603)
- `ExecutionTimeLimitExceeded` (5604)

*Parse Errors (5700-5799):*
- `InvalidNumberParse` (5700)

*Script Termination (5999):*
- `ScriptFailure` (5999) - Script explicitly called `fail` (business logic failure)

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

### Message Formatting

#### IMessageProvider Interface

Defines the contract for formatting diagnostic messages into human-readable strings. Enables localization and custom formatting.

```csharp
public interface IMessageProvider
{
    string Format(IMessage message);
    string? GetTemplate(MessageCode code);
}
```

#### MessageProvider Class

The default implementation provides English-language templates with standardized formatting:

```csharp
var messageProvider = new MessageProvider();
var formattedMessage = messageProvider.Format(message);
// Output: "Line 10, Column 5, JM5230: Cannot access index on type 'String'"
```

**Using MessageProvider for formatted output:**

```csharp
if (!result.IsSuccessful)
{
    var messageProvider = new MessageProvider();
    foreach (var msg in result.Messages)
    {
        Console.WriteLine(messageProvider.Format(msg));
    }
}
```

#### Custom Message Providers (Localization)

Implement `IMessageProvider` to customize error message formatting for different languages or formats:

```csharp
public class FrenchMessageProvider : IMessageProvider
{
    private readonly Dictionary<MessageCode, string> _templates = new()
    {
        { MessageCode.DivisionByZero, "Division par zéro" },
        { MessageCode.InvalidIndexTarget, "Accès à l'index impossible sur le type '{0}'" },
        { MessageCode.PropertyAccessOnNull, "Impossible d'accéder à la propriété '{0}' sur null" },
        // ... other templates
    };

    public string Format(IMessage message)
    {
        var template = GetTemplate(message.Code) ?? message.Code.ToString();
        var text = message.Arguments.Count > 0
            ? string.Format(template, message.Arguments.ToArray())
            : template;
        return $"Ligne {message.LineNumber}, Colonne {message.ColumnPosition}: {text}";
    }

    public string? GetTemplate(MessageCode code) =>
        _templates.TryGetValue(code, out var t) ? t : null;
}
```

**Configuring a custom provider:**

```csharp
var options = new JyroExecutionOptions
{
    MessageProvider = new FrenchMessageProvider()
};

var result = JyroBuilder
    .Create(loggerFactory)
    .WithScript(script)
    .WithData(data)
    .WithOptions(options)
    .Run();
```

**Alternative formats:**

```csharp
// Error code only
public class CodeOnlyMessageProvider : IMessageProvider
{
    public string Format(IMessage message) => $"JM{(int)message.Code:D4}";
    public string? GetTemplate(MessageCode code) => null;
}

// JSON format for API responses
public class JsonMessageProvider : IMessageProvider
{
    public string Format(IMessage message) => JsonSerializer.Serialize(new
    {
        code = $"JM{(int)message.Code:D4}",
        line = message.LineNumber,
        column = message.ColumnPosition,
        message = FormatText(message)
    });

    private string FormatText(IMessage message)
    {
        var provider = new MessageProvider();
        var template = provider.GetTemplate(message.Code) ?? message.Code.ToString();
        return message.Arguments.Count > 0
            ? string.Format(template, message.Arguments.ToArray())
            : template;
    }

    public string? GetTemplate(MessageCode code) => null;
}
```

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
int propertyCount = obj.Count;

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

#### Run() - Full Pipeline Execution

```csharp
var result = JyroBuilder
    .Create(loggerFactory)           // Required: ILoggerFactory
    .WithScript(scriptSource)        // Required: Script source code
    .WithData(dataObject)            // Required: Input data (can be empty)
    .WithStandardLibrary()           // Optional: Include standard functions
    .WithFunction(customFunction)    // Optional: Add custom functions
    .WithOptions(executionOptions)   // Optional: Configure resource limits
    .Run(cancellationToken);         // Optional: CancellationToken
```

`JyroBuilder.Create()` requires an `ILoggerFactory` parameter. Use `NullLoggerFactory.Instance` for no logging.

#### Compile() - Compilation Only

Compiles a script through Parse → Validate → Link stages without executing it. Returns a `JyroLinkingResult` containing the compiled `LinkedProgram`.

```csharp
var linkingResult = JyroBuilder
    .Create(loggerFactory)           // Required: ILoggerFactory
    .WithScript(scriptSource)        // Required: Script source code
    .WithStandardLibrary()           // Optional: Include standard functions
    .WithFunction(customFunction)    // Optional: Add custom functions
    .Compile();

// Check compilation result
if (linkingResult.IsSuccessful && linkingResult.Program != null)
{
    // Store compiled program for later execution
    var compiledProgram = linkingResult.Program;
}
```

**Use cases:**
- Compile once, execute multiple times with different data
- Validate script syntax and semantics before execution
- Cache compiled programs for hot-reload scenarios

#### Execute() - Execute Pre-compiled Program

Executes a previously compiled `LinkedProgram` with fresh data. Requires `WithCompiledProgram()` and `WithData()`.

```csharp
var result = JyroBuilder
    .Create(loggerFactory)           // Required: ILoggerFactory
    .WithCompiledProgram(program)    // Required: Pre-compiled LinkedProgram
    .WithData(dataObject)            // Required: Input data
    .WithOptions(executionOptions)   // Optional: Configure resource limits
    .Execute(cancellationToken);     // Optional: CancellationToken
```

This pattern avoids redundant parsing, validation, and linking.

#### WithCompiledProgram() - Set Pre-compiled Program

Configures the builder to use a pre-compiled `LinkedProgram` from a previous `Compile()` call.

```csharp
JyroBuilder WithCompiledProgram(LinkedProgram program)
```

> **See [Compile Once, Execute Many Times](#compile-once-execute-many-times)** in Performance Considerations for complete examples including error handling, hot-reload with FileSystemWatcher, and ASP.NET Core integration.

#### WithFunctionsFromAssembly() - Load Functions from Assembly

Loads all `IJyroFunction` implementations from a loaded assembly and adds them to the execution environment.

```csharp
JyroBuilder WithFunctionsFromAssembly(Assembly assembly)
```

**Parameters:**
- `assembly` - The assembly to scan for JyroFunction implementations

**Throws:**
- `ArgumentNullException` - When assembly is null
- `InvalidOperationException` - When a function type cannot be instantiated

**Example:**
```csharp
var assembly = Assembly.LoadFrom("path/to/MyPlugins.dll");

var result = JyroBuilder.Create(loggerFactory)
    .WithScript("Data.result = CustomFunction()")
    .WithData(data)
    .WithFunctionsFromAssembly(assembly)
    .Run();
```

#### WithFunctionsFromAssemblyPath() - Load Functions from DLL Path

Loads an assembly from the specified file path, scans it for all `IJyroFunction` implementations, and adds them to the execution environment.

```csharp
JyroBuilder WithFunctionsFromAssemblyPath(string assemblyPath)
```

**Parameters:**
- `assemblyPath` - The file path to the assembly DLL to load

**Throws:**
- `ArgumentNullException` - When assemblyPath is null
- `FileNotFoundException` - When the assembly file does not exist
- `InvalidOperationException` - When a function type cannot be instantiated

**Example:**
```csharp
var result = JyroBuilder.Create(loggerFactory)
    .WithScript("Data.greeting = Greet(\"World\")")
    .WithData(data)
    .WithStandardLibrary()
    .WithFunctionsFromAssemblyPath("C:\\Plugins\\MyFunctions.dll")
    .Run();
```

**Notes:**
- Uses `Assembly.LoadFrom` to load the assembly and its dependencies
- Automatically discovers and instantiates all concrete classes implementing `IJyroFunction`
- All function classes must have public parameterless constructors
- Can be combined with `WithStandardLibrary()` and `WithFunction()` calls

#### WithFunctionsFromDirectory() - Load Functions from Directory

Loads all DLL files from the specified directory, scans each for `IJyroFunction` implementations, and adds them to the execution environment.

```csharp
JyroBuilder WithFunctionsFromDirectory(
    string directoryPath,
    string searchPattern = "*.dll",
    SearchOption searchOption = SearchOption.TopDirectoryOnly)
```

**Parameters:**
- `directoryPath` - The directory path containing plugin DLL files
- `searchPattern` - Optional search pattern for filtering DLL files (default: "*.dll")
- `searchOption` - Optional search option to specify whether to search subdirectories (default: TopDirectoryOnly)

**Throws:**
- `ArgumentNullException` - When directoryPath or searchPattern is null
- `DirectoryNotFoundException` - When the directory does not exist

**Example:**
```csharp
// Load all DLLs from directory
var result = JyroBuilder.Create(loggerFactory)
    .WithScript("Data.result = CustomFunction()")
    .WithData(data)
    .WithFunctionsFromDirectory("C:\\Plugins")
    .Run();

// Load with custom search pattern
var result = JyroBuilder.Create(loggerFactory)
    .WithScript("Data.greeting = Greet(\"World\")")
    .WithData(data)
    .WithFunctionsFromDirectory("C:\\Plugins", "*.JyroPlugin.dll", SearchOption.AllDirectories)
    .Run();
```

**Notes:**
- Automatically skips non-.NET assemblies and DLLs that can't be loaded
- Gracefully handles types without parameterless constructors (skips them instead of throwing)
- Useful for plugin-based architectures where multiple plugin assemblies are deployed to a common directory
- Can be combined with `WithStandardLibrary()` and `WithFunction()` calls

### Creating Error Results

You can manually create error results (e.g., for script-not-found scenarios):

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

The Jyro standard library provides a [comprehensive set of functions](https://docs.mesch.cloud/jyro/functions/stdlib) organized into logical categories for efficient data transformation and processing tasks.

### Mathematical Functions

Functions for numeric calculations and mathematical operations.

- [**Abs**](https://docs.mesch.cloud/jyro/functions/stdlib/math/abs/) - Calculate absolute value of a number
- [**Average**](https://docs.mesch.cloud/jyro/functions/stdlib/math/average/) - Calculate arithmetic mean of multiple numbers
- [**Clamp**](https://docs.mesch.cloud/jyro/functions/stdlib/math/clamp/) - Constrain a value to be within a specified range
- [**Max**](https://docs.mesch.cloud/jyro/functions/stdlib/math/max/) - Find maximum value from multiple arguments
- [**Median**](https://docs.mesch.cloud/jyro/functions/stdlib/math/median/) - Find middle value of sorted numbers
- [**Min**](https://docs.mesch.cloud/jyro/functions/stdlib/math/min/) - Find minimum value from multiple arguments
- [**Mode**](https://docs.mesch.cloud/jyro/functions/stdlib/math/mode/) - Find most frequently occurring value
- [**RandomInt**](https://docs.mesch.cloud/jyro/functions/stdlib/math/randomint/) - Generate cryptographically secure random integer within range
- [**Round**](https://docs.mesch.cloud/jyro/functions/stdlib/math/round/) - Round number to specified decimal places with configurable mode (floor, ceiling, away)
- [**Sum**](https://docs.mesch.cloud/jyro/functions/stdlib/math/sum/) - Calculate sum of multiple numeric arguments

### String Manipulation

Functions for processing and transforming text data.

- [**Contains**](https://docs.mesch.cloud/jyro/functions/stdlib/string/contains/) - Test if string contains substring or array contains value
- [**EndsWith**](https://docs.mesch.cloud/jyro/functions/stdlib/string/endswith/) - Test if string ends with specified suffix
- [**Join**](https://docs.mesch.cloud/jyro/functions/stdlib/string/join/) - Join array elements into single string with delimiter
- [**Lower**](https://docs.mesch.cloud/jyro/functions/stdlib/string/lower/) - Convert string to lowercase
- [**PadLeft**](https://docs.mesch.cloud/jyro/functions/stdlib/string/padleft/) - Pad string on left to specified length
- [**PadRight**](https://docs.mesch.cloud/jyro/functions/stdlib/string/padright/) - Pad string on right to specified length
- [**RandomString**](https://docs.mesch.cloud/jyro/functions/stdlib/string/randomstring/) - Generate cryptographically secure random string from character set
- [**Replace**](https://docs.mesch.cloud/jyro/functions/stdlib/string/replace/) - Replace all occurrences of substring with replacement
- [**Split**](https://docs.mesch.cloud/jyro/functions/stdlib/string/split/) - Split string into array using delimiter
- [**StartsWith**](https://docs.mesch.cloud/jyro/functions/stdlib/string/startswith/) - Test if string begins with specified prefix
- [**Substring**](https://docs.mesch.cloud/jyro/functions/stdlib/string/substring/) - Extract portion of string from start position
- [**Trim**](https://docs.mesch.cloud/jyro/functions/stdlib/string/trim/) - Remove leading and trailing whitespace
- [**Upper**](https://docs.mesch.cloud/jyro/functions/stdlib/string/upper/) - Convert string to uppercase
- [**ToNumber**](https://docs.mesch.cloud/jyro/functions/stdlib/string/tonumber/) - Convert a string to a number
- [**RegexMatch**](https://docs.mesch.cloud/jyro/functions/stdlib/string/regexmatch/) - Extract first regex match as string
- [**RegexMatchAll**](https://docs.mesch.cloud/jyro/functions/stdlib/string/regexmatchall/) - Extract all regex matches as array of strings
- [**RegexMatchDetail**](https://docs.mesch.cloud/jyro/functions/stdlib/string/regexmatchdetail/) - Extract first match with metadata (index, capture groups)
- [**RegexTest**](https://docs.mesch.cloud/jyro/functions/stdlib/string/regextest/) - Test if pattern matches anywhere in text

### Array Operations

Functions for manipulating and processing array data structures.

- [**All**](https://docs.mesch.cloud/jyro/functions/stdlib/array/all/) - Check if all elements match a condition (short-circuits on first non-match)
- [**Any**](https://docs.mesch.cloud/jyro/functions/stdlib/array/any/) - Check if any element matches a condition (short-circuits on first match)
- [**Append**](https://docs.mesch.cloud/jyro/functions/stdlib/array/append/) - Add value to end of array
- [**Clear**](https://docs.mesch.cloud/jyro/functions/stdlib/array/clear/) - Remove all elements from array
- [**CountIf**](https://docs.mesch.cloud/jyro/functions/stdlib/array/countif/) - Count elements where field matches value using comparison operator
- [**Distinct**](https://docs.mesch.cloud/jyro/functions/stdlib/array/distinct/) - Remove duplicate values from array using deep equality
- [**Filter**](https://docs.mesch.cloud/jyro/functions/stdlib/array/filter/) - Return new array with elements matching field comparison criteria
- [**Find**](https://docs.mesch.cloud/jyro/functions/stdlib/array/find/) - Find first matching element (short-circuits, returns null if not found)
- [**First**](https://docs.mesch.cloud/jyro/functions/stdlib/array/first/) - Return first element of array without modifying it
- [**GroupBy**](https://docs.mesch.cloud/jyro/functions/stdlib/array/groupby/) - Group array of objects by field value into keyed object
- [**IndexOf**](https://docs.mesch.cloud/jyro/functions/stdlib/array/indexof/) - Find index of substring in string or element in array
- [**Insert**](https://docs.mesch.cloud/jyro/functions/stdlib/array/insert/) - Insert value at specific array index
- [**Last**](https://docs.mesch.cloud/jyro/functions/stdlib/array/last/) - Return last element of array without modifying it
- [**MergeArrays**](https://docs.mesch.cloud/jyro/functions/stdlib/array/mergearrays/) - Combine multiple arrays into single array
- [**Pop**](https://docs.mesch.cloud/jyro/functions/stdlib/array/pop/) - Remove and return last array element
- [**Project**](https://docs.mesch.cloud/jyro/functions/stdlib/array/project/) - Extract multiple fields from each object into new objects
- [**RandomChoice**](https://docs.mesch.cloud/jyro/functions/stdlib/array/randomchoice/) - Select random element from array using cryptographically secure randomization
- [**RemoveAt**](https://docs.mesch.cloud/jyro/functions/stdlib/array/removeat/) - Remove element at specific index and return modified array
- [**RemoveLast**](https://docs.mesch.cloud/jyro/functions/stdlib/array/removelast/) - Remove last element and return modified array
- [**Reverse**](https://docs.mesch.cloud/jyro/functions/stdlib/array/reverse/) - Return new array with elements in reversed order
- [**Select**](https://docs.mesch.cloud/jyro/functions/stdlib/array/select/) - Extract a single field from each object in an array
- [**SelectMany**](https://docs.mesch.cloud/jyro/functions/stdlib/array/selectmany/) - Extract array fields and flatten into a single array
- [**Sort**](https://docs.mesch.cloud/jyro/functions/stdlib/array/sort/) - Return new sorted array using type-aware comparison
- [**SortByField**](https://docs.mesch.cloud/jyro/functions/stdlib/array/sortbyfield/) - Sort array of objects by specified field
- [**Take**](https://docs.mesch.cloud/jyro/functions/stdlib/array/take/) - Return new array containing first n elements without modifying original
- [**Length**](https://docs.mesch.cloud/jyro/functions/stdlib/array/length/) - Get length/count of strings, arrays, or objects

### Date and Time Functions

Functions for handling date parsing, formatting, and calculations.

- [**DateAdd**](https://docs.mesch.cloud/jyro/functions/stdlib/dateandtime/dateadd/) - Add time interval to date
- [**DateDiff**](https://docs.mesch.cloud/jyro/functions/stdlib/dateandtime/datediff/) - Calculate difference between two dates
- [**DatePart**](https://docs.mesch.cloud/jyro/functions/stdlib/dateandtime/datepart/) - Extract specific component from date
- [**FormatDate**](https://docs.mesch.cloud/jyro/functions/stdlib/dateandtime/formatdate/) - Format date using specified pattern
- [**Now**](https://docs.mesch.cloud/jyro/functions/stdlib/dateandtime/now/) - Get current date and time in UTC
- [**ParseDate**](https://docs.mesch.cloud/jyro/functions/stdlib/dateandtime/parsedate/) - Parse date string into normalized format
- [**Today**](https://docs.mesch.cloud/jyro/functions/stdlib/dateandtime/today/) - Get current date without time components

### Utility Functions

Miscellaneous functions for inspecting and testing data types, value generation, and calling scripts.

- [**Base64Decode**](https://docs.mesch.cloud/jyro/functions/stdlib/utility/base64decode) - Decode Base64-encoded string back to original format
- [**Base64Encode**](https://docs.mesch.cloud/jyro/functions/stdlib/utility/base64encode) - Encode a string to Base64 format
- [**CallScript**](https://docs.mesch.cloud/jyro/functions/stdlib/utility/callscript/) - Execute Jyro script with isolated data context
- [**CallScriptByName**](https://docs.mesch.cloud/jyro/functions/stdlib/utility/callscriptbyname/) - Execute Jyro script by name using configured resolver
- [**Equal**](https://docs.mesch.cloud/jyro/functions/stdlib/utility/equal/) - Test equality between two values
- [**Exists**](https://docs.mesch.cloud/jyro/functions/stdlib/utility/exists/) - Test if value is not null
- [**InvokeRestMethod**](https://docs.mesch.cloud/jyro/functions/stdlib/utility/invokerestmethod/) - Execute HTTP REST API requests with configurable security options
- [**IsNull**](https://docs.mesch.cloud/jyro/functions/stdlib/utility/isnull/) - Test if value is null
- [**Keys**](https://docs.mesch.cloud/jyro/functions/stdlib/utility/keys/) - Get array of property names from an object
- [**Merge**](https://docs.mesch.cloud/jyro/functions/stdlib/utility/merge/) - Combine multiple objects into one (later args override earlier)
- [**NewGuid**](https://docs.mesch.cloud/jyro/functions/stdlib/utility/newguid) - Generate a new globally unique identifier (GUID)
- [**NotEqual**](https://docs.mesch.cloud/jyro/functions/stdlib/utility/notequal/) - Test inequality between two values
- [**TypeOf**](https://docs.mesch.cloud/jyro/functions/stdlib/utility/typeof/) - Get type name of value as string
- [**Values**](https://docs.mesch.cloud/jyro/functions/stdlib/utility/values/) - Get array of property values from an object

### Schema Validation

Functions for validating data against JSON Schema definitions.

- [**ValidateSchema**](https://docs.mesch.cloud/jyro/functions/stdlib/schema/validateschema/) - Validate data against a JSON Schema and return true/false
- [**GetSchemaErrors**](https://docs.mesch.cloud/jyro/functions/stdlib/schema/getschemaerrors/) - Validate data and return detailed error information

### Usage Patterns

Most functions follow consistent patterns for parameter handling and return values:

- **Immutable Transformations**: Functions like `Sort`, `Reverse`, and `Filter` return new arrays without modifying the original, enabling predictable data flow and safe composition
- **Chainable Operations**: Array-returning functions can be composed together (e.g., `Filter(Sort(Filter(...)))`) for powerful data transformations
- **Type Coercion**: Functions automatically handle reasonable type conversions (e.g., converting values to strings in `Join`)
- **Error Handling**: Functions return null or default values for invalid inputs rather than throwing exceptions
- **Safe Access**: Functions like `First`, `Last`, and `Pop` return null for empty arrays instead of throwing errors
- **Consistent Operators**: Functions like `Filter` and `CountIf` support comparison operators: `"=="`, `"!="`, `"<"`, `"<="`, `">"`, `">="`

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
elseif age >= 13 then
    Data.status = "teen"
else
    Data.status = "child"
end
```

#### Ternary Operator

Use the ternary operator for inline conditional expressions:

```jyro
var status = age >= 18 ? "adult" : "minor"
Data.discount = isMember ? 0.20 : 0.05
```

#### Switch Statement

Use `switch` for matching a value against multiple cases:

```jyro
switch Data.status do
    case "pending" then
        Data.message = "Awaiting processing"
    case "approved" then
        Data.message = "Request approved"
    case "rejected" then
        Data.message = "Request denied"
    default then
        Data.message = "Unknown status"
end
```

Note: Unlike C-style switch statements, Jyro's switch does not fall through to subsequent cases. Only the matching case (or default) is executed. Use `break` to exit a case block early if needed.

### Loops
```jyro
# While loop
var count = 0
while count < 100 do
    count = count + 1
end

# Foreach loop (arrays)
Data.total = 0

foreach item in Data.items do
    Data.total = Data.total + item.price
end

# Foreach loop (object keys)
var obj = { "a": 1, "b": 2, "c": 3 }

Data.pairs = []

foreach key in obj do
    Append(Data.pairs, {
        "key": key,
        "value": obj[key]
    })
end
```

#### Loop Control: break and continue

Use `break` to exit a loop early, and `continue` to skip to the next iteration:

```jyro
# Find the first negative number and stop
var firstNegative = null

foreach num in Data.numbers do
    if num < 0 then
        firstNegative = num
        break
    end
end

# Sum only positive numbers
var positiveSum = 0

foreach num in Data.numbers do
    if num < 0 then
        continue
    end
    positiveSum = positiveSum + num
end
```

#### Increment and Decrement

Use `++` and `--` as shorthand for adding or subtracting 1:

```jyro
var count = 0

while count < 10 do
    count++
end

# Works with Data properties too
Data.counter = 100
Data.counter--
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

Jyro uses word based operators.

```jyro
if isActive and age > 18 then
    # ...
end

if not isDeleted or isArchived then
    # ...
end
```

### Script Termination: return and fail

Jyro provides two keywords for explicit script termination with optional messages:

#### return - Successful Exit
```jyro
# Exit immediately with success
return

# Exit with a success message (available in result.Messages)
return "Operation completed successfully"

# Message can be an expression
return "Processed " + Length(Data.items) + " items"
```

#### fail - Business Logic Failure
```jyro
# Exit immediately with failure (IsSuccessful = false)
fail

# Exit with an error message
fail "Validation failed: email is required"

# Conditional failure
if Data.age < 18 then
    fail "Must be 18 or older to proceed"
end

# Message can be an expression
fail "Invalid value: " + Data.input + " is not allowed"
```

**Behavior:**

| Keyword | `IsSuccessful` | Message Severity | MessageCode |
|---------|----------------|------------------|-------------|
| `return` | `true` | (no message) | - |
| `return "msg"` | `true` | Info | `ScriptReturn` (5000) |
| `fail` | `false` | Error | `ScriptFailure` (5999) |
| `fail "msg"` | `false` | Error | `ScriptFailure` (5999) |

**Accessing messages in the host:**
```csharp
var result = JyroBuilder.Create(loggerFactory)
    .WithScript(script)
    .WithData(data)
    .WithStandardLibrary()
    .Run();

if (!result.IsSuccessful)
{
    // Check for explicit script failure
    var failMessage = result.Messages
        .FirstOrDefault(m => m.Code == MessageCode.ScriptFailure);

    if (failMessage != null)
    {
        // Script explicitly called fail
        Console.WriteLine($"Script failed: {string.Join(" ", failMessage.Arguments)}");
    }
}
else
{
    // Check for return message
    var returnMessage = result.Messages
        .FirstOrDefault(m => m.Code == MessageCode.ScriptReturn);

    if (returnMessage != null)
    {
        Console.WriteLine($"Script returned: {string.Join(" ", returnMessage.Arguments)}");
    }
}
```

The message expression must be on the same line as the `return` or `fail` keyword.

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

- **Parsing**: Parse trees are generated on each execution. For frequently-used scripts, use the Compile/Execute pattern (see below)
- **Resource Limits**: Tune `JyroExecutionOptions` based on your use case
- **Standard Library**: Only include `WithStandardLibrary()` if needed
- **Custom Functions**: Keep host function implementations fast to avoid blocking script execution
- **Logging**: Use `NullLoggerFactory.Instance` in production for maximum performance

### Compile Once, Execute Many Times

For scenarios where the same script is executed repeatedly with different data (e.g., API endpoints, hot-reload development, batch processing), you can compile the script once and execute it multiple times. This provides significant performance improvements by avoiding redundant parsing, validation, and linking.

#### Basic Compile/Execute Pattern

```csharp
// Compile the script once
var linkingResult = JyroBuilder
    .Create(loggerFactory)
    .WithScript("Data.result = Data.value * 2")
    .WithStandardLibrary()
    .Compile();

if (!linkingResult.IsSuccessful)
{
    // Handle compilation errors
    Console.WriteLine("Compilation failed!");
    foreach (var error in linkingResult.Messages)
    {
        Console.WriteLine($"{error.Code} at {error.LineNumber}:{error.ColumnPosition}");
    }
    return;
}

// Store the compiled program
var compiledProgram = linkingResult.Program;

// Execute multiple times with different data
for (int i = 1; i <= 5; i++)
{
    var data = new JyroObject();
    data.SetProperty("value", new JyroNumber(i));

    var result = JyroBuilder
        .Create(loggerFactory)
        .WithCompiledProgram(compiledProgram)
        .WithData(data)
        .WithOptions(executionOptions)  // Optional
        .Execute();

    if (result.IsSuccessful)
    {
        var output = (JyroObject)result.Data;
        var resultValue = ((JyroNumber)output.GetProperty("result")).Value;
        Console.WriteLine($"Input: {i}, Output: {resultValue}");
    }
}
```

**Performance improvement**: Typically 5-10x faster for cached executions compared to full compilation on every request.

#### Hot-Reload with FileSystemWatcher

For development scenarios where scripts are frequently modified, combine compilation caching with `FileSystemWatcher` for automatic cache invalidation:

```csharp
public class JyroScriptCacheService : IDisposable
{
    private readonly ConcurrentDictionary<string, LinkedProgram> _cache = new();
    private readonly FileSystemWatcher _watcher;
    private readonly string _scriptsPath;

    public JyroScriptCacheService(string scriptsPath)
    {
        _scriptsPath = scriptsPath;

        // Watch for .jyro file changes
        _watcher = new FileSystemWatcher(scriptsPath)
        {
            Filter = "*.jyro",
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        _watcher.Changed += OnScriptChanged;
        _watcher.Created += OnScriptChanged;
        _watcher.Deleted += (s, e) => _cache.TryRemove(e.FullPath, out _);
    }

    private void OnScriptChanged(object sender, FileSystemEventArgs e)
    {
        // Invalidate cache when file changes
        _cache.TryRemove(e.FullPath, out _);
    }

    public LinkedProgram? GetOrCompile(string scriptPath)
    {
        // Try to get cached compiled program
        if (_cache.TryGetValue(scriptPath, out var program))
        {
            return program;
        }

        // Cache miss - compile the script
        var scriptContent = File.ReadAllText(scriptPath);

        var linkingResult = JyroBuilder
            .Create(loggerFactory)
            .WithScript(scriptContent)
            .WithStandardLibrary()
            .Compile();

        if (!linkingResult.IsSuccessful || linkingResult.Program == null)
        {
            return null;
        }

        // Cache the compiled program
        _cache[scriptPath] = linkingResult.Program;
        return linkingResult.Program;
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _cache.Clear();
    }
}
```

#### Usage in ASP.NET Core

```csharp
[ApiController]
[Route("api/scripts")]
public class ScriptController : ControllerBase
{
    private readonly JyroScriptCacheService _cacheService;
    private readonly ILoggerFactory _loggerFactory;

    [HttpPost("execute/{scriptName}")]
    public IActionResult ExecuteScript(string scriptName, [FromBody] JyroObject data)
    {
        var scriptPath = Path.Combine(_scriptsPath, $"{scriptName}.jyro");

        // Get cached compiled program (or compile if not cached)
        var compiledProgram = _cacheService.GetOrCompile(scriptPath);
        if (compiledProgram == null)
        {
            return BadRequest("Script compilation failed");
        }

        // Execute with fresh data
        var result = JyroBuilder
            .Create(_loggerFactory)
            .WithCompiledProgram(compiledProgram)
            .WithData(data)
            .Execute();

        if (!result.IsSuccessful)
        {
            return BadRequest(result.Messages);
        }

        return Ok(result.Data);
    }
}
```

**Benefits of this approach:**
- **Fast execution**: Scripts are compiled once and executed many times
- **Hot-reload**: File changes automatically invalidate the cache
- **Memory efficient**: Only compiled programs are cached, not intermediate parse trees
- **Thread-safe**: Uses `ConcurrentDictionary` for safe concurrent access

#### JyroLinkingResult

The `Compile()` method returns a `JyroLinkingResult` containing:

**Properties:**
- `bool IsSuccessful` - Whether compilation succeeded
- `LinkedProgram? Program` - The compiled program (null if compilation failed)
- `IReadOnlyList<IMessage> Messages` - Compilation errors and warnings
- `LinkingMetadata Metadata` - Compilation statistics

```csharp
public class LinkingMetadata
{
    public TimeSpan ProcessingTime { get; }      // Compilation time
    public int FunctionCount { get; }            // Number of resolved functions
    public DateTimeOffset StartedAt { get; }     // When compilation started
}
```

## Security

Jyro is designed for safe execution of untrusted scripts:

- ✅ **Sandboxed execution** - No file system, network, or system access
- ✅ **Resource limits** - Prevent infinite loops and resource exhaustion
- ✅ **Type safety** - Strong typing prevents type confusion attacks
- ✅ **No reflection** - Scripts cannot access .NET reflection APIs
- ✅ **Linear, predictable execution** - Scripts run atomically from start to finish; No yields, callbacks, coroutines, or continuations; no async/await, no multithreading, no way to pause and resume with a potentially tampered state
- ✅ **Fail-fast** - Runtime errors stop execution immediately

Host functions are the primary extension point. Ensure your custom functions validate inputs and don't expose sensitive functionality.

## License

MIT License

Copyright © Mesch Systems

## Support

For issues, feature requests, or questions:
- Open an issue on [GitHub](https://github.com/meschsystems/Mesch.Jyro/issues)
- Visit the [documentation](https://docs.mesch.cloud/jyro/)
- Try the [online playpen](https://playpen.jyro.dev/) to experiment with Jyro
