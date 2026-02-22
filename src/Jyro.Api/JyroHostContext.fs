namespace Mesch.Jyro

open System.Collections.Generic
open System.Threading

/// Extended execution context for host integration
type JyroHostContext(?limiter: JyroResourceLimiter, ?cancellationToken: CancellationToken) =
    inherit Mesch.Jyro.JyroExecutionContext(?limiter = limiter, ?cancellationToken = cancellationToken)

    let hostFunctions = Dictionary<string, IJyroFunction>()
    let variables = Dictionary<string, JyroValue>()

    /// Host-provided functions
    member _.HostFunctions = hostFunctions :> IReadOnlyDictionary<string, IJyroFunction>

    /// Script-level variables
    member _.Variables = variables :> IReadOnlyDictionary<string, JyroValue>

    /// Register a host function
    member _.RegisterFunction(func: IJyroFunction) =
        hostFunctions.[func.Name] <- func

    /// Set a variable value
    member _.SetVariable(name: string, value: JyroValue) =
        variables.[name] <- value

    /// Get a variable value
    member _.GetVariable(name: string) : JyroValue =
        match variables.TryGetValue(name) with
        | true, v -> v
        | _ -> JyroNull.Instance
