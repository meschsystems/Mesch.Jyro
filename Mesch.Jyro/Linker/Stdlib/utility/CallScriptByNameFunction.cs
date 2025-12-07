namespace Mesch.Jyro;

/// <summary>
/// Standard library function that executes a Jyro script by name using the configured script resolver.
/// The script resolver must be configured on the execution context to resolve script names to source code.
/// The called script receives the provided data as its Data context and returns the modified
/// data upon completion.
/// </summary>
public sealed class CallScriptByNameFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CallScriptByNameFunction"/> class.
    /// </summary>
    public CallScriptByNameFunction() : base(new JyroFunctionSignature(
        "CallScriptByName",
        new[] {
            new Parameter("scriptName", ParameterType.String),
            new Parameter("data", ParameterType.Any)
        },
        ParameterType.Any))
    {
    }

    /// <summary>
    /// Executes a Jyro script resolved by name using the configured script resolver.
    /// The child script shares all resource limits and execution state with the parent script,
    /// ensuring cumulative resource consumption and preventing resource multiplication attacks.
    /// Includes cycle detection to prevent infinite recursion and enforces a maximum call depth limit.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The name of the script to resolve and execute (JyroString)
    /// - arguments[1]: The data object to provide as the Data context for the child script (any JyroValue type)
    /// </param>
    /// <param name="executionContext">
    /// The execution context that will be shared with the child script. Must have a script resolver
    /// configured to resolve script names to source code.
    /// </param>
    /// <returns>
    /// The modified data object returned by the child script execution. If compilation or
    /// execution fails, returns the original data argument unchanged.
    /// </returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the script resolver is not configured, the script name cannot be resolved,
    /// a script execution cycle is detected, or the maximum script call depth is exceeded.
    /// </exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var scriptName = GetArgument<JyroString>(arguments, 0);
        var dataArgument = arguments[1];

        // Validate script resolver is configured
        if (executionContext.Resolver == null)
        {
            throw new JyroRuntimeException(
                "Script resolver not configured - cannot use CallScriptByName");
        }

        // Resolve script name to source code
        var scriptSource = executionContext.Resolver(scriptName.Value);
        if (scriptSource == null)
        {
            throw new JyroRuntimeException(
                $"Script not found: '{scriptName.Value}'");
        }

        // Use a hash of the script name for cycle detection (more stable than source hash)
        var scriptHash = $"name:{scriptName.Value}";

        // Check for cycles using the execution context's call stack
        if (executionContext.ScriptCallStack.Contains(scriptHash))
        {
            throw new JyroRuntimeException("CallScriptByName cycle detected - script has been previously seen in the call chain");
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
            var parsingResult = jyro.Parse(scriptSource);
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
            var originalData = executionContext.Variables.TryGet(JyroExecutionContext.RootIdentifier, out var currentData) ? currentData : JyroNull.Instance;
            executionContext.Variables.Declare(JyroExecutionContext.RootIdentifier, dataArgument);

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
                executionContext.Variables.Declare(JyroExecutionContext.RootIdentifier, originalData);
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
}
