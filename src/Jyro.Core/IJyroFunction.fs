namespace Mesch.Jyro

open System.Collections.Generic
open System.Threading

/// Parameter types for function signatures
type ParameterType =
    | AnyParam
    | NumberParam
    | StringParam
    | BooleanParam
    | ObjectParam
    | ArrayParam
    | NullParam

/// Parameter definition for function signatures
type Parameter =
    { Name: string
      Type: ParameterType
      IsOptional: bool }

    static member Required(name: string, paramType: ParameterType) =
        { Name = name; Type = paramType; IsOptional = false }

    static member Optional(name: string, paramType: ParameterType) =
        { Name = name; Type = paramType; IsOptional = true }

/// Function signature for type checking and documentation
type JyroFunctionSignature =
    { Name: string
      Parameters: Parameter list
      ReturnType: ParameterType
      MinArgs: int
      MaxArgs: int }

    member this.ValidateArgCount(argCount: int) : bool =
        argCount >= this.MinArgs && argCount <= this.MaxArgs

/// Interface for Jyro functions (stdlib and host functions)
type IJyroFunction =
    abstract member Name: string
    abstract member Signature: JyroFunctionSignature
    abstract member Execute: args: IReadOnlyList<JyroValue> * ctx: JyroExecutionContext -> JyroValue

/// Execution context for function calls and resource limiting.
/// Delegation methods are called from compiled LINQ Expression Trees at runtime.
and JyroExecutionContext(?limiter: JyroResourceLimiter, ?cancellationToken: CancellationToken) =
    let messages = ResizeArray<DiagnosticMessage>()
    let limiterInstance = limiter
    let externalCt = defaultArg cancellationToken CancellationToken.None
    let mutable returnMessage: string option = None

    member _.Messages = messages :> IReadOnlyList<DiagnosticMessage>
    member _.Limiter = limiterInstance

    /// Cancellation token for cooperative cancellation of blocking operations.
    /// When a resource limiter is configured, this token auto-cancels after MaxExecutionTime.
    /// Hosts can also supply an external token via the builder.
    member _.CancellationToken =
        match limiterInstance with
        | Some l -> l.CancellationToken
        | None -> externalCt

    member _.AddMessage(msg: DiagnosticMessage) = messages.Add(msg)

    /// Optional message set by a return or fail statement
    member _.ReturnMessage
        with get() = returnMessage
        and set(v) = returnMessage <- v

    /// Called from compiled expression trees to store the return/fail message
    member _.SetReturnMessage(msg: string) = returnMessage <- Some msg

    // Resource limiter delegation - no-op when no limiter is configured

    member _.CheckAndCountStatement() =
        match limiterInstance with Some l -> l.CheckAndCountStatement() | None -> ()

    member _.CheckAndEnterLoop() =
        match limiterInstance with Some l -> l.CheckAndEnterLoop() | None -> ()

    member _.CheckAndEnterCall() =
        match limiterInstance with Some l -> l.CheckAndEnterCall() | None -> ()

    member _.ExitCall() =
        match limiterInstance with Some l -> l.ExitCall() | None -> ()
