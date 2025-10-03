namespace Mesch.Jyro;

/// <summary>
/// Standard library function that executes a Jyro script in an isolated execution context.
/// The called script receives the provided data as its Data context and returns the modified
/// data upon completion. Compilation and execution errors are handled gracefully by returning
/// the original data unchanged.
/// </summary>
public sealed class CallScriptFunction : JyroFunctionBase
{
    private const string CallStackKey = "__CallScript_Stack";

    /// <summary>
    /// Initializes a new instance of the <see cref="CallScriptFunction"/> class.
    /// </summary>
    public CallScriptFunction() : base(new JyroFunctionSignature(
        "CallScript",
        new[] {
            new Parameter("scriptSource", ParameterType.String),
            new Parameter("data", ParameterType.Any)
        },
        ParameterType.Any))
    {
    }

    /// <summary>
    /// Executes a Jyro script within the current execution context using the provided data
    /// as the Data variable. The child script shares all resource limits and execution state
    /// with the parent script, ensuring cumulative resource consumption and preventing
    /// resource multiplication attacks. Includes cycle detection to prevent infinite recursion
    /// and enforces a maximum call depth limit for additional protection.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The Jyro script source code to execute (JyroString)
    /// - arguments[1]: The data object to provide as the Data context for the child script (any JyroValue type)
    /// </param>
    /// <param name="executionContext">
    /// The execution context that will be shared with the child script. All resource limits,
    /// counters, and execution state are preserved and shared between parent and child.
    /// </param>
    /// <returns>
    /// The modified data object returned by the child script execution. If compilation or
    /// execution fails, returns the original data argument unchanged. If a cycle is detected
    /// or maximum call depth is exceeded, throws a JyroRuntimeException.
    /// </returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when a script execution cycle is detected (script calls itself directly or
    /// indirectly) or when the maximum script call depth is exceeded.
    /// </exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var scriptSource = GetArgument<JyroString>(arguments, 0);
        var dataArgument = arguments[1];
        var scriptHash = scriptSource.Value.GetHashCode().ToString("X8");

        // Check for cycles using the execution context's call stack
        if (executionContext.ScriptCallStack.Contains(scriptHash))
        {
            throw new JyroRuntimeException("CallScript cycle detected - called script has been previously seen in the call chain");
        }

        try
        {
            executionContext.ScriptCallStack.Add(scriptHash);
            executionContext.Limiter.CheckAndEnterScriptCall();

            // Create pipeline components
            var validator = new Validator(
                Microsoft.Extensions.Logging.Abstractions.NullLogger<Validator>.Instance,
                executionContext.Functions.Keys);
            var linker = new Linker(
                Microsoft.Extensions.Logging.Abstractions.NullLogger<Linker>.Instance);
            var interpreter = new Interpreter();
            var jyro = new Jyro(validator, linker, interpreter);

            // Parse the script
            var parsingResult = jyro.Parse(scriptSource.Value);
            if (!parsingResult.IsSuccessful || parsingResult.ProgramContext == null)
            {
                return dataArgument;
            }

            // Validate the script
            var validationResult = jyro.Validate(parsingResult.ProgramContext);
            if (!validationResult.IsSuccessful)
            {
                return dataArgument;
            }

            // Link the script
            var linkingResult = jyro.Link(parsingResult.ProgramContext, executionContext.Functions.Values);
            if (!linkingResult.IsSuccessful || linkingResult.Program is null)
            {
                return dataArgument;
            }

            // Store the original Data value and replace it with the provided data
            var originalData = executionContext.Variables.TryGet(ExecutionContext.RootIdentifier, out var currentData) ? currentData : JyroNull.Instance;
            executionContext.Variables.Declare(ExecutionContext.RootIdentifier, dataArgument);

            try
            {
                var executionResult = interpreter.Execute(linkingResult.Program, executionContext);

                if (!executionResult.IsSuccessful)
                {
                    return dataArgument;
                }

                // Return the modified data from the child script execution
                return executionResult.Data;
            }
            finally
            {
                // Restore the original Data value
                executionContext.Variables.Declare(ExecutionContext.RootIdentifier, originalData);
            }
        }
        catch (OperationCanceledException)
        {
            return dataArgument;
        }
        catch (JyroRuntimeException)
        {
            throw; // Re-throw runtime exceptions (including cycle detection)
        }
        catch (Exception)
        {
            return dataArgument;
        }
        finally
        {
            // Remove current script from call stack and exit resource tracking
            if (executionContext.ScriptCallStack.Count > 0 &&
                executionContext.ScriptCallStack[^1] == scriptHash)
            {
                executionContext.ScriptCallStack.RemoveAt(executionContext.ScriptCallStack.Count - 1);
            }
            executionContext.Limiter.ExitScriptCall();
        }
    }

    /// <summary>
    /// Gets the call stack from the execution context, creating it if it doesn't exist.
    /// </summary>
    /// <param name="executionContext">The execution context to examine.</param>
    /// <returns>A list representing the current call stack.</returns>
    private static List<string> GetCallStack(ExecutionContext executionContext)
    {
        if (executionContext.Variables.TryGet(CallStackKey, out var stackValue) &&
            stackValue is JyroObject stackObject &&
            stackObject.GetProperty("stack") is JyroArray stackArray)
        {
            // Convert JyroArray back to List<string>
            var result = new List<string>();
            foreach (var item in stackArray)
            {
                if (item is JyroString stringItem)
                {
                    result.Add(stringItem.Value);
                }
            }
            return result;
        }

        // Create new call stack
        var newStack = new List<string>();
        SetCallStack(executionContext, newStack);
        return newStack;
    }

    /// <summary>
    /// Stores the call stack in the execution context.
    /// </summary>
    /// <param name="executionContext">The execution context to update.</param>
    /// <param name="callStack">The call stack to store.</param>
    private static void SetCallStack(ExecutionContext executionContext, List<string> callStack)
    {
        // Convert List<string> to JyroArray
        var stackArray = new JyroArray();
        foreach (var item in callStack)
        {
            stackArray.Add(new JyroString(item));
        }

        var stackObject = new JyroObject();
        stackObject.SetProperty("stack", stackArray);

        executionContext.Variables.Declare(CallStackKey, stackObject);
    }

    /// <summary>
    /// Creates a hash of the script source for cycle detection.
    /// Uses a simple hash that's sufficient for cycle detection without cryptographic requirements.
    /// </summary>
    /// <param name="scriptSource">The script source to hash.</param>
    /// <returns>A string hash of the script source.</returns>
    private static string GetScriptHash(string scriptSource)
    {
        // Simple hash implementation - could use SHA256 for more robustness
        return scriptSource.GetHashCode().ToString("X8");
    }
}