# Jyro

A secure, sandboxed scripting language for executing untrusted code within .NET applications.

## Installation

The `Mesch.Jyro` NuGet package is distributed as a single package containing the parser, compiler, standard library, and host API. It can be installed via the .NET CLI:

```
dotnet add package Mesch.Jyro
```

All public types reside in the `Mesch.Jyro` namespace.

## Overview

Jyro is designed to be embedded in .NET applications where user-supplied or externally authored scripts must be executed safely. Scripts are compiled to LINQ Expression Trees and evaluated against input data provided by the host. The compilation pipeline consists of parsing, validation, linking, compilation, and execution stages, all of which are managed automatically by the builder API.

## Building and Executing Scripts

The primary entry point for host applications is the `JyroBuilder` class, which exposes a fluent interface for configuring and running Jyro scripts. A minimal example is shown below:

```csharp
var result = new JyroBuilder()
    .WithSource("return data.name")
    .WithJson("""{ "name": "Alice" }""")
    .Execute();

if (result.IsSuccess)
    Console.WriteLine(result.Value.ToStringValue()); // "Alice"
```

### Providing Script Source

A script may be supplied in one of two forms. Source code is provided via `WithSource`, which accepts a Jyro script as a string. Alternatively, if the script has been previously compiled to the `.jyrx` binary format, the precompiled bytes may be supplied via `WithCompiledBytes`, which bypasses the parsing and validation stages entirely. These two methods are mutually exclusive; calling one will clear the other.

### Providing Input Data

Input data is supplied to the script through one of the `WithData` or `WithJson` overloads. The `WithData(JyroValue)` overload accepts a pre-constructed Jyro value directly. The `WithData(obj)` overload accepts any .NET object and converts it to the corresponding Jyro representation automatically; dictionaries become Jyro objects, enumerables become Jyro arrays, and primitive types are mapped to their Jyro equivalents. The `WithJson` overload parses a JSON string into the Jyro type system. Within the script, the input data is accessible through the identifier `data`.

### Compilation and Execution

Three terminal methods are provided. `Execute` compiles the script (if source was provided) and evaluates it against the configured input data, returning a `JyroResult<JyroValue>`. `ExecuteOrThrow` behaves identically but throws an `InvalidOperationException` containing all diagnostic messages if the script fails. `Compile` performs only compilation without execution, returning a `JyroResult<CompiledProgram>` that may be retained and executed multiple times. `CompileToBytes` compiles the source to the `.jyrx` binary format, which is suitable for serialization and later reuse via `WithCompiledBytes`.

### Working with Results

All pipeline methods return a `JyroResult<T>`, which contains an `IsSuccess` flag, an optional `Value`, and a list of `DiagnosticMessage` records. Each diagnostic message carries a `MessageCode`, a `Severity` (Info, Warning, or Error), a human-readable `Message` string, and an optional `SourceLocation` indicating the line, column, and length of the relevant source span. The result's `HasErrors` property may be used to check whether any error-level diagnostics were produced.

The `JyroValue` returned on success can be converted back to .NET types through methods such as `ToStringValue()`, `ToDouble()`, `ToBoolean()`, `ToInt32()`, `ToInt64()`, and `ToObjectValue()`. The `ToJson()` method serializes the value to a JSON string. Compound values may be downcast via `AsObject()` or `AsArray()` to access `JyroObject` and `JyroArray` members respectively. A `JyroObject` exposes its entries through the `Properties` dictionary, and a `JyroArray` exposes its elements through the `Items` list.

## Diagnostics and Error Handling

Every diagnostic produced by the pipeline carries a unique `MessageCode` in the `JMXXXX` format, where the leading digit identifies the pipeline stage: `1xxx` for lexer errors, `2xxx` for parser errors, `3xxx` for validation errors, `4xxx` for linker errors, and `5xxx` for runtime errors. Within each stage, the hundreds digit groups related errors by category (e.g. `52xx` for arithmetic errors, `53xx` for index/property access errors). The full code reference is available in the [language documentation](https://docs.mesch.cloud/jyro/error-codes/).

### Formatting Diagnostics

The `DiagnosticFormatter` module provides a standard formatter that produces deterministically parseable output:

```csharp
foreach (var msg in result.Messages)
{
    Console.Error.WriteLine(DiagnosticFormatter.formatMessage(msg));
}
// [JM5200] Ln 12, Col 5: Division by zero
// [JM3100] Ln 3, Col 1: Undeclared variable 'foo'
```

### Structured Diagnostics

For programmatic consumption, `DiagnosticFormatter.toStructured` converts a diagnostic into a `StructuredDiagnostic` record containing the code, numeric code, severity, message, args, location, and subsystem name — suitable for JSON serialization or UI rendering.

```csharp
var structured = DiagnosticFormatter.toStructured(msg);
// structured.Code        = "JM5200"
// structured.NumericCode = 5200
// structured.Subsystem   = "runtime"
// structured.Message     = "Division by zero"
```

### Localization

Each diagnostic carries an `Args` array containing the raw values used to construct its message. Hosts can provide localized message templates by implementing `IMessageTemplateProvider`, which maps a `MessageCode` to a format string. The `DiagnosticFormatter.formatLocalized` function applies the provider's template with the diagnostic's args, falling back to the default English message when no template is found.

```csharp
public class FrenchTemplates : IMessageTemplateProvider
{
    public FSharpOption<string> GetTemplate(MessageCode code) => code switch
    {
        MessageCode.DivisionByZero => FSharpOption<string>.Some("Division par zéro"),
        MessageCode.InvalidType => FSharpOption<string>.Some(
            "Impossible d'assigner {0} à la variable '{1}' de type {2}"),
        _ => FSharpOption<string>.None
    };
}

var formatted = DiagnosticFormatter.formatLocalized(new FrenchTemplates(), msg);
```

## Resource Limits

Because Jyro is intended for executing untrusted code, the runtime supports configurable resource limits to prevent runaway scripts. Resource limits are **opt-in**: if no execution options are configured, no resource limiter is created and scripts run without constraints. This is appropriate for trusted environments such as CLI tools, but hosts executing untrusted code should always configure limits.

Limits are configured through `JyroExecutionOptions`, which may be supplied to the builder via `WithExecutionOptions` or configured individually through convenience methods. Calling any `WithMax*` method activates the resource limiter using `JyroExecutionOptions.Default` as the baseline:

| Limit | Default | Builder Method |
|---|---|---|
| Maximum execution time | 5 seconds | `WithMaxExecutionTime(TimeSpan)` |
| Maximum statement count | 10,000 | `WithMaxStatements(int)` |
| Maximum cumulative loop iterations | 1,000 | `WithMaxLoopIterations(int)` |
| Maximum call stack depth | 64 | `WithMaxCallDepth(int)` |

When a limit is exceeded, a `JyroRuntimeException` is raised and the result is returned as a failure with the corresponding diagnostic code (e.g., `StatementLimitExceeded`, `ExecutionTimeLimitExceeded`).

A preconfigured `JyroExecutionOptions.Unlimited` value is also available, which sets all limits to their maximum representable values.

```csharp
var result = new JyroBuilder()
    .WithSource(script)
    .WithJson(json)
    .WithMaxExecutionTime(TimeSpan.FromSeconds(2))
    .WithMaxLoopIterations(500)
    .Execute();
```

### Cancellation

The resource limiter checks limits at statement, loop, and call boundaries. However, custom functions that perform blocking I/O (such as HTTP requests) can bypass these checkpoints. To address this, the builder supports cooperative cancellation through `WithCancellationToken`:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
var result = new JyroBuilder()
    .WithSource(script)
    .WithJson(json)
    .WithMaxExecutionTime(TimeSpan.FromSeconds(5))
    .WithCancellationToken(cts.Token)
    .Execute();
```

When execution options are configured, the resource limiter creates a `CancellationTokenSource` that auto-cancels after `MaxExecutionTime`. If an external token is also provided via `WithCancellationToken`, the two are linked so that either one can trigger cancellation. Custom functions receive the combined token through `ctx.CancellationToken` and should pass it to any blocking operations. When the token fires, in-flight operations are cancelled immediately rather than waiting for the next checkpoint.

If a script is cancelled, the result is returned as a failure with the `CancelledByHost` diagnostic code.

## Pipeline Statistics

Per-stage timing information can be collected by passing a `JyroPipelineStats` instance to the builder via `WithStats`. After execution, the instance will contain the elapsed time for each stage of the pipeline: `Parse`, `Validate`, `Link`, `Compile`, `Execute`, and `Deserialize` (the last of which is populated only when loading from `.jyrx` bytes). A `Total` property returns the sum of all stages.

```csharp
var stats = new JyroPipelineStats();
var result = new JyroBuilder()
    .WithSource(script)
    .WithJson(json)
    .WithStats(stats)
    .Execute();

Console.WriteLine($"Parse: {stats.Parse.TotalMilliseconds}ms");
Console.WriteLine($"Execute: {stats.Execute.TotalMilliseconds}ms");
Console.WriteLine($"Total: {stats.Total.TotalMilliseconds}ms");
```

## Standard Library

The Jyro standard library is included by default and provides built-in functions for string manipulation, array operations, math, date/time handling, schema validation, querying, and general utilities. The standard library can be excluded by calling `UseStdlib(false)` on the builder, which may be useful when a minimal or fully custom function set is desired.

## Custom Functions

Host applications may extend the set of functions available to Jyro scripts by implementing the `IJyroFunction` interface or by subclassing `JyroFunctionBase`, which provides typed argument retrieval helpers such as `GetStringArgument`, `GetNumberArgument`, `GetArrayArgument`, and `GetObjectArgument`.

A custom function must define a `Name` (the identifier by which it is called from scripts), a `Signature` (specifying parameter names, types, optionality, and return type), and an `Execute` method. The following example demonstrates a function that reverses a string:

```csharp
public class ReverseStringFunction : JyroFunctionBase
{
    public ReverseStringFunction() : base("reverseString",
        new JyroFunctionSignature
        {
            Name = "reverseString",
            Parameters = [Parameter.Required("value", ParameterType.StringParam)],
            ReturnType = ParameterType.StringParam,
            MinArgs = 1,
            MaxArgs = 1
        }) { }

    public override JyroValue ExecuteImpl(
        IReadOnlyList<JyroValue> args, JyroExecutionContext ctx)
    {
        var input = GetStringArgument(args, 0);
        var reversed = new string(input.Reverse().ToArray());
        return new JyroString(reversed);
    }
}
```

Custom functions are registered on the builder via `AddFunction` or `AddFunctions`:

```csharp
var result = new JyroBuilder()
    .WithSource("return reverseString(data.text)")
    .WithJson("""{ "text": "hello" }""")
    .AddFunction(new ReverseStringFunction())
    .Execute();
```

The execution context passed to every function exposes a `CancellationToken` property. Functions that perform blocking I/O should pass `ctx.CancellationToken` to their underlying operations so that the host's resource limiter and external cancellation signals can terminate in-flight work:

```csharp
public override JyroValue ExecuteImpl(
    IReadOnlyList<JyroValue> args, JyroExecutionContext ctx)
{
    // Pass the token to blocking operations
    var response = _httpClient.Send(request, ctx.CancellationToken);
    ...
}
```

If a custom function is registered with the same name as a standard library function, the linker will report a `FunctionOverride` warning and the custom implementation will take precedence.

## Precompilation

For scenarios where the same script is executed repeatedly against different input data, the compilation overhead can be eliminated by precompiling the script to the `.jyrx` binary format. The compiled bytes may be stored, cached, or transmitted as needed and then loaded without repeating the parse, validate, or link stages.

```csharp
// Compile once
var bytesResult = new JyroBuilder()
    .WithSource(script)
    .CompileToBytes();

byte[] jyrxBytes = bytesResult.Value;

// Execute many times
var result = new JyroBuilder()
    .WithCompiledBytes(jyrxBytes)
    .WithJson(json)
    .Execute();
```

## C# Extension Methods

For convenience, extension methods are provided on `JyroValue` and `object` that allow scripts to be executed inline:

```csharp
var data = JyroValue.FromJson("""{ "x": 10 }""");
var result = data.ExecuteJyro("return data.x * 2");
```

## IL Trimming

Jyro compiles scripts to LINQ Expression Trees, which requires runtime reflection to resolve methods such as `GetProperty`, `EvaluateBinary`, and `ToBooleanTruthiness`. When a project is published with IL trimming enabled, the trimmer may remove these members because they are not referenced through direct calls.

Trimming is enabled by default in Blazor WebAssembly and .NET MAUI projects, and can be opted into by any project via `<PublishTrimmed>true</PublishTrimmed>`. Standard ASP.NET, console, and desktop applications are not affected unless trimming is explicitly enabled.

| Deployment | Trimming by default? | Affected? |
|---|---|---|
| Blazor WebAssembly (publish) | Yes | Yes |
| Azure Static Web Apps (Blazor WASM) | Yes | Yes |
| .NET MAUI / mobile | Yes | Yes |
| Console/desktop with `PublishTrimmed` | Yes | Yes |
| ASP.NET server-side / Blazor Server | No | No |
| Standard console/desktop app | No | No |
| Docker container (without trimming) | No | No |

To prevent the trimmer from removing Jyro members, add `TrimmerRootAssembly` entries for the Jyro assemblies in the consuming project's `.csproj`:

```xml
<ItemGroup>
    <TrimmerRootAssembly Include="Jyro.Core" />
    <TrimmerRootAssembly Include="Jyro.Compiler" />
    <TrimmerRootAssembly Include="Jyro.Stdlib" />
    <TrimmerRootAssembly Include="Jyro.Api" />
    <TrimmerRootAssembly Include="Jyro.Parser" />
</ItemGroup>
```

Without these entries, scripts that use lambda functions (`Map`, `Where`, `SortBy`, etc.) will fail at runtime with a `NullReferenceException`.

## F# Helper Functions

When used from F#, the `JyroBuilderFactory` module (which is `AutoOpen`) provides shorthand functions:

```fsharp
// Create a builder
let builder = jyro()

// One-shot execution with an object
let result = executeScript "return data.x + 1" {| x = 10 |}

// One-shot execution with JSON
let result = executeScriptWithJson "return data.x + 1" """{ "x": 10 }"""
```
