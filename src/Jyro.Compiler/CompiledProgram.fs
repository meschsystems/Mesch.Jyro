namespace Mesch.Jyro

open System
open System.Linq.Expressions
open System.Security.Cryptography
open System.Text
open Mesch.Jyro.ExpressionCompiler
open Mesch.Jyro.StatementCompiler
open Mesch.Jyro.Linker
open Mesch.Jyro.BinaryFormat

/// <summary>A compiled Jyro program ready for execution.</summary>
type CompiledProgram =
    { /// <summary>The linked program that was compiled, containing resolved function references and the validated AST.</summary>
      LinkedProgram: LinkedProgram
      /// <summary>The compiled delegate that executes the program. Accepts input data and an execution context, and returns the resulting data.</summary>
      CompiledFunction: Func<JyroValue, JyroExecutionContext, JyroValue> }

/// <summary>Module for compiling, executing, and building Jyro programs.</summary>
module Compiler =
    /// <summary>Compiles a linked program into an executable delegate.</summary>
    /// <param name="linkedProgram">The linked program containing resolved function references and a validated AST.</param>
    /// <returns>A <see cref="JyroResult{T}"/> containing the compiled program on success, or diagnostic messages on failure.</returns>
    let compile (linkedProgram: LinkedProgram) : JyroResult<CompiledProgram> =
        try
            // Create the return label first so statements can reference it
            let returnLabel = Expression.Label(typeof<JyroValue>, "return")
            let baseCtx = CompilationContext.Create(linkedProgram.Functions)
            let ctx = baseCtx.WithReturnLabel(returnLabel)
            let bodyExpr = compileBlock ctx linkedProgram.Program.Statements

            // Wrap the body to return the Data parameter if no explicit return
            let finalBody = Expression.Block(
                typeof<JyroValue>,
                [| bodyExpr
                   Expression.Label(returnLabel, ctx.DataParam) |])

            let lambda = Expression.Lambda<Func<JyroValue, JyroExecutionContext, JyroValue>>(
                            finalBody, ctx.DataParam, ctx.ContextParam)
            let compiled = lambda.Compile()

            let program =
                { LinkedProgram = linkedProgram
                  CompiledFunction = compiled }

            JyroResult<CompiledProgram>.Success(program)
        with ex ->
            let msg = DiagnosticMessage.Error(MessageCode.UnknownExecutorError, ex.Message)
            JyroResult<CompiledProgram>.Failure(msg)

    /// <summary>Executes a compiled program with the given data and execution context.</summary>
    /// <param name="program">The compiled program to execute.</param>
    /// <param name="data">The input data to process. This value is mutated in place by the script.</param>
    /// <param name="ctx">The execution context providing resource limits, options, and runtime state.</param>
    /// <returns>
    /// A <see cref="JyroResult{T}"/> containing the data value. Data is always returned even on failure,
    /// since the script mutates it in place during execution.
    /// </returns>
    let execute (program: CompiledProgram) (data: JyroValue) (ctx: JyroExecutionContext) : JyroResult<JyroValue> =
        try
            match ctx.Limiter with
            | Some limiter -> limiter.Start()
            | None -> ()

            let _result = program.CompiledFunction.Invoke(data, ctx)

            match ctx.Limiter with
            | Some limiter -> limiter.Stop()
            | None -> ()

            JyroResult<JyroValue>.Success(data)
        with
        | :? JyroRuntimeException as ex ->
            match ctx.Limiter with
            | Some limiter -> limiter.Stop()
            | None -> ()
            let location = if ex.HasLocation then Some (SourceLocation.Create(ex.Line, ex.Column)) else None
            let msg = DiagnosticMessage.Error(ex.Code, ex.Message, args = ex.Args, ?location = location)
            { Value = Some data; Messages = [msg]; IsSuccess = false }
        | :? OperationCanceledException ->
            match ctx.Limiter with
            | Some limiter -> limiter.Stop()
            | None -> ()
            let msg = DiagnosticMessage.Error(MessageCode.CancelledByHost, "Script execution was cancelled by the host")
            { Value = Some data; Messages = [msg]; IsSuccess = false }
        | ex ->
            match ctx.Limiter with
            | Some limiter -> limiter.Stop()
            | None -> ()
            let msg = DiagnosticMessage.Error(MessageCode.RuntimeError, ex.Message)
            { Value = Some data; Messages = [msg]; IsSuccess = false }

    /// <summary>Executes a compiled program without resource limiting using a default execution context.</summary>
    /// <param name="program">The compiled program to execute.</param>
    /// <param name="data">The input data to process. This value is mutated in place by the script.</param>
    /// <returns>
    /// A <see cref="JyroResult{T}"/> containing the data value. Data is always returned even on failure,
    /// since the script mutates it in place during execution.
    /// </returns>
    let executeSimple (program: CompiledProgram) (data: JyroValue) : JyroResult<JyroValue> =
        execute program data (JyroExecutionContext())

    /// <summary>
    /// Runs the full compilation pipeline from source code to a compiled program.
    /// Performs parsing, validation, linking, and compilation in sequence.
    /// </summary>
    /// <param name="source">The Jyro source code to compile.</param>
    /// <param name="functions">The available functions (stdlib and custom) to link against.</param>
    /// <returns>A <see cref="JyroResult{T}"/> containing the compiled program on success, or diagnostic messages from the first failing stage.</returns>
    let compileFromSource (source: string) (functions: IJyroFunction seq) : JyroResult<CompiledProgram> =
        match Parser.parseToResult source with
        | { IsSuccess = false; Messages = msgs } ->
            JyroResult<CompiledProgram>.Failure(msgs)
        | { Value = Some program } ->
            match Validator.validate program with
            | { IsSuccess = false; Messages = msgs } ->
                JyroResult<CompiledProgram>.Failure(msgs)
            | { Value = Some validatedProgram } ->
                match link validatedProgram functions with
                | { IsSuccess = false; Messages = msgs } ->
                    JyroResult<CompiledProgram>.Failure(msgs)
                | { Value = Some linkedProgram } ->
                    compile linkedProgram
                | _ -> JyroResult<CompiledProgram>.Failure(DiagnosticMessage.Error(MessageCode.UnknownLinkerError, "Unknown linker error"))
            | _ -> JyroResult<CompiledProgram>.Failure(DiagnosticMessage.Error(MessageCode.UnknownValidatorError, "Unknown validator error"))
        | _ -> JyroResult<CompiledProgram>.Failure(DiagnosticMessage.Error(MessageCode.UnknownParserError, "Unknown parser error"))

    /// <summary>
    /// Compiles source code to .jyrx binary format. Performs the full pipeline (parse, validate, link)
    /// then serializes the validated AST and function dependency table.
    /// </summary>
    /// <param name="source">The Jyro source code to compile.</param>
    /// <param name="functions">The available functions (stdlib and custom) to link against.</param>
    /// <returns>A <see cref="JyroResult{T}"/> containing the .jyrx byte array on success.</returns>
    let compileToJyrx (source: string) (functions: IJyroFunction seq) : JyroResult<byte[]> =
        match Parser.parseToResult source with
        | { IsSuccess = false; Messages = msgs } ->
            JyroResult<byte[]>.Failure(msgs)
        | { Value = Some program } ->
            match Validator.validate program with
            | { IsSuccess = false; Messages = msgs } ->
                JyroResult<byte[]>.Failure(msgs)
            | { Value = Some validatedProgram } ->
                match link validatedProgram functions with
                | { IsSuccess = false; Messages = msgs } ->
                    JyroResult<byte[]>.Failure(msgs)
                | { Value = Some linkedProgram } ->
                    let functionNames = linkedProgram.Functions |> Map.toList |> List.map fst
                    let sourceHash = SHA256.HashData(Encoding.UTF8.GetBytes(source))
                    let bytes = serialize linkedProgram.Program functionNames sourceHash
                    JyroResult<byte[]>.Success(bytes)
                | _ -> JyroResult<byte[]>.Failure(DiagnosticMessage.Error(MessageCode.UnknownLinkerError, "Unknown linker error"))
            | _ -> JyroResult<byte[]>.Failure(DiagnosticMessage.Error(MessageCode.UnknownValidatorError, "Unknown validator error"))
        | _ -> JyroResult<byte[]>.Failure(DiagnosticMessage.Error(MessageCode.UnknownParserError, "Unknown parser error"))

    /// <summary>
    /// Loads and compiles a program from .jyrx binary format. Deserializes the AST, resolves functions
    /// by name against the provided registry, and compiles the Expression Tree.
    /// Skips parsing and validation entirely.
    /// </summary>
    /// <param name="data">The .jyrx binary data.</param>
    /// <param name="functions">The available functions (stdlib and custom) to resolve against.</param>
    /// <returns>A <see cref="JyroResult{T}"/> containing the compiled program on success.</returns>
    let compileFromJyrx (data: byte[]) (functions: IJyroFunction seq) : JyroResult<CompiledProgram> =
        match deserialize data with
        | { IsSuccess = false; Messages = msgs } ->
            JyroResult<CompiledProgram>.Failure(msgs)
        | { Value = Some deserialized } ->
            // Resolve required functions by name
            let funcMap = functions |> Seq.map (fun f -> f.Name, f) |> Map.ofSeq
            let mutable errors = []
            for name in deserialized.RequiredFunctions do
                if not (funcMap.ContainsKey(name)) then
                    errors <- DiagnosticMessage.Error(MessageCode.UndefinedFunction, sprintf "Required function '%s' not available" name) :: errors
            if not errors.IsEmpty then
                JyroResult<CompiledProgram>.Failure(List.rev errors)
            else
                // Build linked program with resolved functions
                let resolvedFunctions =
                    deserialized.RequiredFunctions
                    |> List.map (fun name -> name, funcMap.[name])
                    |> Map.ofList
                let linkedProgram = { Program = deserialized.Program; Functions = resolvedFunctions }
                compile linkedProgram
        | _ -> JyroResult<CompiledProgram>.Failure(DiagnosticMessage.Error(MessageCode.UnknownParserError, "Failed to deserialize .jyrx file"))

    /// <summary>Compiles from source with per-stage timing recorded into the stats collector.</summary>
    let compileFromSourceTimed (source: string) (functions: IJyroFunction seq) (stats: JyroPipelineStats) : JyroResult<CompiledProgram> =
        match JyroPipelineStats.TimeStage((fun t -> stats.Parse <- t), fun () -> Parser.parseToResult source) with
        | { IsSuccess = false; Messages = msgs } ->
            JyroResult<CompiledProgram>.Failure(msgs)
        | { Value = Some program } ->
            match JyroPipelineStats.TimeStage((fun t -> stats.Validate <- t), fun () -> Validator.validate program) with
            | { IsSuccess = false; Messages = msgs } ->
                JyroResult<CompiledProgram>.Failure(msgs)
            | { Value = Some validatedProgram } ->
                match JyroPipelineStats.TimeStage((fun t -> stats.Link <- t), fun () -> link validatedProgram functions) with
                | { IsSuccess = false; Messages = msgs } ->
                    JyroResult<CompiledProgram>.Failure(msgs)
                | { Value = Some linkedProgram } ->
                    JyroPipelineStats.TimeStage((fun t -> stats.Compile <- t), fun () -> compile linkedProgram)
                | _ -> JyroResult<CompiledProgram>.Failure(DiagnosticMessage.Error(MessageCode.UnknownLinkerError, "Unknown linker error"))
            | _ -> JyroResult<CompiledProgram>.Failure(DiagnosticMessage.Error(MessageCode.UnknownValidatorError, "Unknown validator error"))
        | _ -> JyroResult<CompiledProgram>.Failure(DiagnosticMessage.Error(MessageCode.UnknownParserError, "Unknown parser error"))

    /// <summary>Loads and compiles from .jyrx binary format with per-stage timing.</summary>
    let compileFromJyrxTimed (data: byte[]) (functions: IJyroFunction seq) (stats: JyroPipelineStats) : JyroResult<CompiledProgram> =
        match JyroPipelineStats.TimeStage((fun t -> stats.Deserialize <- t), fun () -> deserialize data) with
        | { IsSuccess = false; Messages = msgs } ->
            JyroResult<CompiledProgram>.Failure(msgs)
        | { Value = Some deserialized } ->
            let funcMap = functions |> Seq.map (fun f -> f.Name, f) |> Map.ofSeq
            let mutable errors = []
            for name in deserialized.RequiredFunctions do
                if not (funcMap.ContainsKey(name)) then
                    errors <- DiagnosticMessage.Error(MessageCode.UndefinedFunction, sprintf "Required function '%s' not available" name) :: errors
            if not errors.IsEmpty then
                JyroResult<CompiledProgram>.Failure(List.rev errors)
            else
                let resolvedFunctions =
                    deserialized.RequiredFunctions
                    |> List.map (fun name -> name, funcMap.[name])
                    |> Map.ofList
                let linkedProgram = { Program = deserialized.Program; Functions = resolvedFunctions }
                JyroPipelineStats.TimeStage((fun t -> stats.Compile <- t), fun () -> compile linkedProgram)
        | _ -> JyroResult<CompiledProgram>.Failure(DiagnosticMessage.Error(MessageCode.UnknownParserError, "Failed to deserialize .jyrx file"))

    /// <summary>Executes a compiled program with timing for the execute stage.</summary>
    let executeTimed (program: CompiledProgram) (data: JyroValue) (ctx: JyroExecutionContext) (stats: JyroPipelineStats) : JyroResult<JyroValue> =
        JyroPipelineStats.TimeStage((fun t -> stats.Execute <- t), fun () -> execute program data ctx)
