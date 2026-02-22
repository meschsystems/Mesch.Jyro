namespace Mesch.Jyro

open System.Diagnostics
open System.Threading

/// Enforces runtime resource quotas and execution limits to protect against
/// runaway scripts and ensure system stability during Jyro script execution.
[<Sealed>]
type JyroResourceLimiter(options: JyroExecutionOptions, ?externalToken: CancellationToken) =
    let stopwatch = Stopwatch()
    let externalCt = defaultArg externalToken CancellationToken.None
    let mutable cts: CancellationTokenSource option = None
    let mutable statementCount = 0
    let mutable loopIterations = 0
    let mutable callDepth = 0

    member _.Options = options
    member _.StatementCount = statementCount
    member _.LoopIterations = loopIterations
    member _.CallDepth = callDepth
    member _.ElapsedTime = stopwatch.Elapsed

    /// Cancellation token that fires when MaxExecutionTime is exceeded or the host cancels
    member _.CancellationToken =
        match cts with
        | Some c -> c.Token
        | None -> externalCt

    /// Start the execution timer and create the cancellation token source
    member _.Start() =
        stopwatch.Start()
        let newCts =
            if externalCt.CanBeCanceled then
                CancellationTokenSource.CreateLinkedTokenSource(externalCt)
            else
                new CancellationTokenSource()
        if options.MaxExecutionTime < System.TimeSpan.MaxValue then
            newCts.CancelAfter(options.MaxExecutionTime)
        cts <- Some newCts

    /// Stop the execution timer and dispose the cancellation token source
    member _.Stop() =
        stopwatch.Stop()
        match cts with
        | Some c -> c.Dispose(); cts <- None
        | None -> ()

    /// Check execution time limit
    member _.CheckExecutionTime() =
        if stopwatch.Elapsed > options.MaxExecutionTime then
            JyroError.raiseRuntime MessageCode.ExecutionTimeLimitExceeded [| box (int options.MaxExecutionTime.TotalMilliseconds) |]

    /// Check and count a statement execution
    member this.CheckAndCountStatement() =
        statementCount <- statementCount + 1
        if statementCount > options.MaxStatements then
            JyroError.raiseRuntime MessageCode.StatementLimitExceeded [| box options.MaxStatements |]
        this.CheckExecutionTime()

    /// Check and count a loop iteration (cumulative across all loops)
    member this.CheckAndEnterLoop() =
        loopIterations <- loopIterations + 1
        if loopIterations > options.MaxLoopIterations then
            JyroError.raiseRuntime MessageCode.LoopIterationLimitExceeded [| box options.MaxLoopIterations |]
        this.CheckExecutionTime()

    /// Check and enter a function call
    member this.CheckAndEnterCall() =
        callDepth <- callDepth + 1
        if callDepth > options.MaxCallDepth then
            JyroError.raiseRuntime MessageCode.CallDepthLimitExceeded [| box options.MaxCallDepth |]
        this.CheckExecutionTime()

    /// Exit a function call
    member _.ExitCall() =
        callDepth <- callDepth - 1
