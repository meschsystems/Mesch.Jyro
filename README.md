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
- **VS Code Extension**: Full language support with syntax highlighting, error checking, and IntelliSense - [install from VS Code Marketplace](https://marketplace.visualstudio.com/items?itemName=meschsystems.jyro-vscode)

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

*General Execution Errors (5000-5099):*
- `UnknownExecutorError` (5000)
- `RuntimeError` (5001)
- `CancelledByHost` (5002)
- `InvalidType` (5003)
- `InvalidArgumentType` (5004)

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

#### Run() - Full Pipeline Execution

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

**Performance benefit:** Avoids redundant parsing, validation, and linking. Typically 5x-10x faster than `Run()` for cached programs.

#### WithCompiledProgram() - Set Pre-compiled Program

Configures the builder to use a pre-compiled `LinkedProgram` from a previous `Compile()` call.

```csharp
JyroBuilder WithCompiledProgram(LinkedProgram program)
```

**Example:**
```csharp
// Compile once
var linkingResult = JyroBuilder.Create(loggerFactory)
    .WithScript(script)
    .Compile();

var program = linkingResult.Program;

// Execute many times
for (int i = 0; i < 1000; i++)
{
    var result = JyroBuilder.Create(loggerFactory)
        .WithCompiledProgram(program)
        .WithData(GenerateData(i))
        .Execute();
}
```

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

The Jyro standard library provides a comprehensive set of functions organized into logical categories for efficient data transformation and processing tasks.

### Mathematical Functions

Functions for numeric calculations and mathematical operations.

- [**Abs**](stdlib/math/abs/) - Calculate absolute value of a number
- [**Max**](stdlib/math/max/) - Find maximum value from multiple arguments
- [**Min**](stdlib/math/min/) - Find minimum value from multiple arguments
- [**RandomInt**](stdlib/math/randomint/) - Generate cryptographically secure random integer within range
- [**Round**](stdlib/math/round/) - Round number to specified decimal places
- [**Sum**](stdlib/math/sum/) - Calculate sum of multiple numeric arguments

### String Manipulation

Functions for processing and transforming text data.

- [**Contains**](stdlib/string/contains/) - Test if string contains substring or array contains value
- [**EndsWith**](stdlib/string/endswith/) - Test if string ends with specified suffix
- [**Join**](stdlib/string/join/) - Join array elements into single string with delimiter
- [**Lower**](stdlib/string/lower/) - Convert string to lowercase
- [**RandomString**](stdlib/string/randomstring/) - Generate cryptographically secure random string from character set
- [**Replace**](stdlib/string/replace/) - Replace all occurrences of substring with replacement
- [**Split**](stdlib/string/split/) - Split string into array using delimiter
- [**StartsWith**](stdlib/string/startswith/) - Test if string begins with specified prefix
- [**Trim**](stdlib/string/trim/) - Remove leading and trailing whitespace
- [**Upper**](stdlib/string/upper/) - Convert string to uppercase
- [**ToNumber**](stdlib/string/tonumber/) - Convert a string to a number

### Array Operations

Functions for manipulating and processing array data structures.

- [**Append**](stdlib/array/append/) - Add value to end of array
- [**Clear**](stdlib/array/clear/) - Remove all elements from array
- [**CountIf**](stdlib/array/countif/) - Count elements where field matches value using comparison operator
- [**Filter**](stdlib/array/filter/) - Return new array with elements matching field comparison criteria
- [**First**](stdlib/array/first/) - Return first element of array without modifying it
- [**GroupBy**](stdlib/array/groupby/) - Group array of objects by field value into keyed object
- [**IndexOf**](stdlib/array/indexof/) - Find index of element in array using deep equality
- [**Insert**](stdlib/array/insert/) - Insert value at specific array index
- [**Last**](stdlib/array/last/) - Return last element of array without modifying it
- [**MergeArrays**](stdlib/array/mergearrays/) - Combine multiple arrays into single array
- [**Pop**](stdlib/array/pop/) - Remove and return last array element
- [**RandomChoice**](stdlib/array/randomchoice/) - Select random element from array using cryptographically secure randomization
- [**RemoveAt**](stdlib/array/removeat/) - Remove element at specific index and return modified array
- [**RemoveLast**](stdlib/array/removelast/) - Remove last element and return modified array
- [**Reverse**](stdlib/array/reverse/) - Return new array with elements in reversed order
- [**Sort**](stdlib/array/sort/) - Return new sorted array using type-aware comparison
- [**SortByField**](stdlib/array/sortbyfield/) - Sort array of objects by specified field
- [**Take**](stdlib/array/take/) - Return new array containing first n elements without modifying original

### Date and Time Functions

Functions for handling date parsing, formatting, and calculations.

- [**DateAdd**](stdlib/dateandtime/dateadd/) - Add time interval to date
- [**DateDiff**](stdlib/dateandtime/datediff/) - Calculate difference between two dates
- [**DatePart**](stdlib/dateandtime/datepart/) - Extract specific component from date
- [**FormatDate**](stdlib/dateandtime/formatdate/) - Format date using specified pattern
- [**Now**](stdlib/dateandtime/now/) - Get current date and time in UTC
- [**ParseDate**](stdlib/dateandtime/parsedate/) - Parse date string into normalized format
- [**Today**](stdlib/dateandtime/today/) - Get current date without time components

### Utility Functions

Miscellaneous functions for inspecting and testing data types, value generation, and calling scripts.

- [**Base64Decode**](stdlib/utility/base64decode) - Decode Base64-encoded string back to original format
- [**Base64Encode**](stdlib/utility/base64encode) - Encode a string to Base64 format
- [**CallScript**](stdlib/utility/callscript/) - Execute Jyro script with isolated data context
- [**Equal**](stdlib/utility/equal/) - Test equality between two values
- [**Exists**](stdlib/utility/exists/) - Test if value is not null
- [**InvokeRestMethod**](stdlib/utility/invokerestmethod/) - Execute HTTP REST API requests with configurable security options
- [**IsNull**](stdlib/utility/isnull/) - Test if value is null
- [**Keys**](stdlib/utility/keys/) - Get array of property names from an object
- [**Length**](stdlib/utility/length/) - Get length/count of strings, arrays, or objects
- [**NewGuid**](stdlib/utility/newguid) - Generate a new globally unique identifier (GUID)
- [**NotEqual**](stdlib/utility/notequal/) - Test inequality between two values
- [**TypeOf**](stdlib/utility/typeof/) - Get type name of value as string

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
else if age >= 13 then
    Data.status = "teen"
else
    Data.status = "child"
end
```

### Loops
```jyro
# While loop
var count = 0
while count < 100 do
    count = count + 1
end

# Foreach loop (arrays)
foreach item in Data.items do
    Data.total = Data.total + item.price
end

# Foreach loop (object keys)
var obj = { "a": 1, "b": 2, "c": 3 }
foreach key in obj do
    # key is a string ("a", "b", "c")
    var value = obj[key]
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

**Performance improvement**: Typically 20-50x faster for cached executions compared to full compilation on every request.

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
- ✅ **Fail-fast** - Runtime errors stop execution immediately

**Important**: Host functions are the primary extension point. Ensure your custom functions validate inputs and don't expose sensitive functionality.

## License

MIT License

Copyright © Mesch Systems

## Support

For issues, feature requests, or questions:
- Open an issue on [GitHub](https://github.com/meschsystems/Mesch.Jyro/issues)
- Visit the [documentation](https://docs.mesch.cloud/jyro/)
- Try the [online playpen](https://playpen.jyro.dev/) to experiment with Jyro
