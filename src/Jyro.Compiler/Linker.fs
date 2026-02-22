namespace Mesch.Jyro


/// Linker for resolving function references
module Linker =
    /// Linking context
    type LinkContext =
        { Functions: Map<string, IJyroFunction>
          ReferencedFunctions: Set<string>
          Messages: DiagnosticMessage list }

        static member Empty =
            { Functions = Map.empty
              ReferencedFunctions = Set.empty
              Messages = [] }

        member this.AddFunction(func: IJyroFunction) =
            { this with Functions = this.Functions.Add(func.Name, func) }

        member this.MarkReferenced(name: string) =
            { this with ReferencedFunctions = this.ReferencedFunctions.Add(name) }

        member this.AddError(code: MessageCode, message: string, args: obj[], location: SourceLocation) =
            let msg = DiagnosticMessage.Error(code, message, args = args, location = location)
            { this with Messages = msg :: this.Messages }

    /// Check function calls in an expression
    let rec private linkExpr (ctx: LinkContext) (expr: Expr) : LinkContext =
        match expr with
        | Call(name, args, pos) ->
            let ctx' = args |> List.fold linkExpr ctx
            match ctx'.Functions.TryFind(name) with
            | Some func ->
                if not (func.Signature.ValidateArgCount(args.Length)) then
                    let loc = SourceLocation.Create(pos.Line, pos.Column)
                    if args.Length < func.Signature.MinArgs then
                        let msg = sprintf "Function '%s' requires at least %d arguments, but %d were provided" name func.Signature.MinArgs args.Length
                        let msgArgs = [| box name; box func.Signature.MinArgs; box args.Length |]
                        ctx'.AddError(MessageCode.TooFewArguments, msg, msgArgs, loc)
                    else
                        let msg = sprintf "Function '%s' accepts at most %d arguments, but %d were provided" name func.Signature.MaxArgs args.Length
                        let msgArgs = [| box name; box func.Signature.MaxArgs; box args.Length |]
                        ctx'.AddError(MessageCode.TooManyArguments, msg, msgArgs, loc)
                else
                    ctx'.MarkReferenced(name)
            | None ->
                let loc = SourceLocation.Create(pos.Line, pos.Column)
                let msg = sprintf "Undefined function '%s'" name
                ctx'.AddError(MessageCode.UndefinedFunction, msg, [| box name |], loc)
        | Binary(left, _, right, _) ->
            ctx |> linkExpr <| left |> linkExpr <| right
        | Unary(_, operand, _) ->
            ctx |> linkExpr <| operand
        | Ternary(cond, thenExpr, elseExpr, _) ->
            ctx |> linkExpr <| cond |> linkExpr <| thenExpr |> linkExpr <| elseExpr
        | PropertyAccess(target, _, _) ->
            ctx |> linkExpr <| target
        | IndexAccess(target, index, _) ->
            ctx |> linkExpr <| target |> linkExpr <| index
        | ObjectLiteral(props, _) ->
            props |> List.fold (fun c (_, e) -> linkExpr c e) ctx
        | ArrayLiteral(elems, _) ->
            elems |> List.fold linkExpr ctx
        | Lambda(_, body, _) ->
            ctx |> linkExpr <| body
        | TypeCheck(e, _, _, _) ->
            ctx |> linkExpr <| e
        | IncrementDecrement(e, _, _, _) ->
            ctx |> linkExpr <| e
        | _ -> ctx

    /// Check function calls in a statement
    let rec private linkStmt (ctx: LinkContext) (stmt: Stmt) : LinkContext =
        match stmt with
        | VarDecl(_, _, init, _) ->
            match init with
            | Some expr -> linkExpr ctx expr
            | None -> ctx
        | Assignment(target, _, value, _) ->
            ctx |> linkExpr <| target |> linkExpr <| value
        | If(cond, thenBlock, elseIfs, elseBlock, _) ->
            let ctx' = linkExpr ctx cond
            let ctx'' = thenBlock |> List.fold linkStmt ctx'
            let ctx''' = elseIfs |> List.fold (fun c (e, stmts) ->
                let c' = linkExpr c e
                stmts |> List.fold linkStmt c') ctx''
            match elseBlock with
            | Some stmts -> stmts |> List.fold linkStmt ctx'''
            | None -> ctx'''
        | While(cond, body, _) ->
            let ctx' = linkExpr ctx cond
            body |> List.fold linkStmt ctx'
        | ForEach(_, collection, body, _) ->
            let ctx' = linkExpr ctx collection
            body |> List.fold linkStmt ctx'
        | For(_, startExpr, endExpr, stepExpr, _, body, _) ->
            let ctx' = linkExpr ctx startExpr
            let ctx' = linkExpr ctx' endExpr
            let ctx' = match stepExpr with Some e -> linkExpr ctx' e | None -> ctx'
            body |> List.fold linkStmt ctx'
        | Switch(expr, cases, defaultCase, _) ->
            let ctx' = linkExpr ctx expr
            let ctx'' = cases |> List.fold (fun c case ->
                let c' = case.Values |> List.fold linkExpr c
                case.Body |> List.fold linkStmt c') ctx'
            match defaultCase with
            | Some stmts -> stmts |> List.fold linkStmt ctx''
            | None -> ctx''
        | Return(valueOpt, _) ->
            match valueOpt with
            | Some expr -> linkExpr ctx expr
            | None -> ctx
        | Fail(msgOpt, _) ->
            match msgOpt with
            | Some expr -> linkExpr ctx expr
            | None -> ctx
        | Break _ | Continue _ -> ctx
        | ExprStmt(expr, _) ->
            linkExpr ctx expr

    /// Linked program ready for execution
    type LinkedProgram =
        { Program: Program
          Functions: Map<string, IJyroFunction> }

    /// Link a program with the available functions
    let link (program: Program) (functions: IJyroFunction seq) : JyroResult<LinkedProgram> =
        let ctx = functions |> Seq.fold (fun (c: LinkContext) (f: IJyroFunction) -> c.AddFunction(f)) LinkContext.Empty
        let ctx' = program.Statements |> List.fold linkStmt ctx
        let messages = ctx'.Messages |> List.rev
        if messages |> List.exists (fun m -> m.Severity = MessageSeverity.Error) then
            JyroResult<LinkedProgram>.Failure(messages)
        else
            let referencedFunctionMap =
                ctx'.ReferencedFunctions
                |> Set.toSeq
                |> Seq.choose (fun name -> ctx'.Functions.TryFind(name) |> Option.map (fun f -> name, f))
                |> Map.ofSeq
            let linkedProgram = { Program = program; Functions = referencedFunctionMap }
            { Value = Some linkedProgram; Messages = messages; IsSuccess = true }
