namespace Mesch.Jyro


/// Semantic validation for Jyro programs
module Validator =
    /// Variable scope tracking
    type Scope =
        { Variables: Set<string>
          Parent: Scope option
          InLoop: bool }

        static member Empty = { Variables = Set.empty; Parent = None; InLoop = false }

        member this.WithVariable(name: string) =
            { this with Variables = this.Variables.Add(name) }

        member this.Append() =
            { Variables = Set.empty; Parent = Some this; InLoop = this.InLoop }

        member this.EnterLoop() =
            { this with InLoop = true }

        member this.IsDeclared(name: string) : bool =
            if this.Variables.Contains(name) then true
            elif this.Parent.IsSome then this.Parent.Value.IsDeclared(name)
            else false

    /// Validation context
    type ValidationContext =
        { Scope: Scope
          Messages: DiagnosticMessage list
          DefinedFunctions: Set<string> }

        static member Empty =
            { Scope = Scope.Empty
              Messages = []
              DefinedFunctions = Set.ofList [ "Data" ] }

        member this.AddError(code: MessageCode, message: string, args: obj[], location: SourceLocation) =
            let msg = DiagnosticMessage.Error(code, message, args = args, location = location)
            { this with Messages = msg :: this.Messages }

        member this.AddWarning(code: MessageCode, message: string, args: obj[], location: SourceLocation) =
            let msg = DiagnosticMessage.Warning(code, message, args = args, location = location)
            { this with Messages = msg :: this.Messages }

    /// Validate an expression
    let rec validateExpr (ctx: ValidationContext) (expr: Expr) : ValidationContext =
        match expr with
        | Identifier(name, pos) when name <> "Data" && not (ctx.Scope.IsDeclared(name)) ->
            let loc = SourceLocation.Create(pos.Line, pos.Column)
            let msg = sprintf "Undeclared variable '%s'" name
            ctx.AddError(MessageCode.UndeclaredVariable, msg, [| box name |], loc)
        | Binary(left, _, right, _) ->
            ctx |> validateExpr <| left |> validateExpr <| right
        | Unary(_, operand, _) ->
            ctx |> validateExpr <| operand
        | Ternary(cond, thenExpr, elseExpr, _) ->
            ctx |> validateExpr <| cond |> validateExpr <| thenExpr |> validateExpr <| elseExpr
        | Call(_, args, _) ->
            args |> List.fold validateExpr ctx
        | PropertyAccess(target, _, _) ->
            ctx |> validateExpr <| target
        | IndexAccess(target, index, _) ->
            ctx |> validateExpr <| target |> validateExpr <| index
        | ObjectLiteral(props, _) ->
            props |> List.fold (fun c (_, e) -> validateExpr c e) ctx
        | ArrayLiteral(elems, _) ->
            elems |> List.fold validateExpr ctx
        | Lambda(params', body, _) ->
            let innerCtx = { ctx with Scope = params' |> List.fold (fun s p -> s.WithVariable(p)) (ctx.Scope.Append()) }
            let validated = validateExpr innerCtx body
            { ctx with Messages = validated.Messages }
        | TypeCheck(e, _, _, _) ->
            ctx |> validateExpr <| e
        | IncrementDecrement(e, _, _, _) ->
            ctx |> validateExpr <| e
        | _ -> ctx

    /// Validate a statement
    let rec validateStmt (ctx: ValidationContext) (stmt: Stmt) : ValidationContext =
        match stmt with
        | VarDecl(name, typeHint, init, pos) ->
            let ctx' =
                match init with
                | Some expr -> validateExpr ctx expr
                | None -> ctx
            let ctx' =
                match typeHint, init with
                | Some hint, None when hint <> AnyType ->
                    let loc = SourceLocation.Create(pos.Line, pos.Column)
                    let msg = sprintf "Typed variable '%s' must have an initializer" name
                    ctx'.AddError(MessageCode.TypeMismatch, msg, [| box msg |], loc)
                | _ -> ctx'
            if ctx'.Scope.Variables.Contains(name) then
                let loc = SourceLocation.Create(pos.Line, pos.Column)
                let msg = sprintf "Variable '%s' is already declared" name
                ctx'.AddError(MessageCode.VariableAlreadyDeclared, msg, [| box name |], loc)
            else
                { ctx' with Scope = ctx'.Scope.WithVariable(name) }
        | Assignment(target, _, value, pos) ->
            let ctx' = validateExpr ctx target |> validateExpr <| value
            if not (Ast.isAssignmentTarget target) then
                let loc = SourceLocation.Create(pos.Line, pos.Column)
                ctx'.AddError(MessageCode.InvalidAssignmentTarget, "Invalid assignment target", Array.empty, loc)
            else
                ctx'
        | If(cond, thenBlock, elseIfs, elseBlock, _) ->
            let ctx' = validateExpr ctx cond
            let thenCtx = thenBlock |> List.fold validateStmt { ctx' with Scope = ctx'.Scope.Append() }
            let ctx'' = { thenCtx with Scope = ctx'.Scope }
            let ctx''' = elseIfs |> List.fold (fun c (e, stmts) ->
                let c' = validateExpr c e
                let bodyCtx = stmts |> List.fold validateStmt { c' with Scope = c'.Scope.Append() }
                { bodyCtx with Scope = c'.Scope }) ctx''
            match elseBlock with
            | Some stmts ->
                let bodyCtx = stmts |> List.fold validateStmt { ctx''' with Scope = ctx'''.Scope.Append() }
                { bodyCtx with Scope = ctx'''.Scope }
            | None -> ctx'''
        | While(cond, body, _) ->
            let ctx' = validateExpr ctx cond
            let loopCtx = { ctx' with Scope = ctx'.Scope.Append().EnterLoop() }
            let bodyCtx = body |> List.fold validateStmt loopCtx
            { bodyCtx with Scope = ctx'.Scope }
        | ForEach(varName, collection, body, _) ->
            let ctx' = validateExpr ctx collection
            let ctx' =
                match collection with
                | Literal(v, pos) when v.ValueType = JyroValueType.Null
                                    || v.ValueType = JyroValueType.Number
                                    || v.ValueType = JyroValueType.Boolean ->
                    let loc = SourceLocation.Create(pos.Line, pos.Column)
                    let msg = sprintf "Value of type %A is not iterable" v.ValueType
                    ctx'.AddError(MessageCode.NotIterableLiteral, msg, [| box v.ValueType |], loc)
                | _ -> ctx'
            let loopScope = ctx'.Scope.Append().EnterLoop().WithVariable(varName)
            let loopCtx = { ctx' with Scope = loopScope }
            let bodyCtx = body |> List.fold validateStmt loopCtx
            { bodyCtx with Scope = ctx'.Scope }
        | For(varName, startExpr, endExpr, stepExpr, _, body, _) ->
            let ctx' = validateExpr ctx startExpr
            let ctx' = validateExpr ctx' endExpr
            let ctx' = match stepExpr with Some e -> validateExpr ctx' e | None -> ctx'
            let loopScope = ctx'.Scope.Append().EnterLoop().WithVariable(varName)
            let bodyCtx = body |> List.fold validateStmt { ctx' with Scope = loopScope }
            { bodyCtx with Scope = ctx'.Scope }
        | Switch(expr, cases, defaultCase, _) ->
            let ctx' = validateExpr ctx expr
            let ctx'' = cases |> List.fold (fun c case ->
                let c' = case.Values |> List.fold validateExpr c
                let bodyCtx = case.Body |> List.fold validateStmt { c' with Scope = c'.Scope.Append() }
                { bodyCtx with Scope = c'.Scope }) ctx'
            match defaultCase with
            | Some stmts ->
                let bodyCtx = stmts |> List.fold validateStmt { ctx'' with Scope = ctx''.Scope.Append() }
                { bodyCtx with Scope = ctx''.Scope }
            | None -> ctx''
        | Return(valueOpt, _) ->
            match valueOpt with
            | Some expr -> validateExpr ctx expr
            | None -> ctx
        | Fail(msgOpt, _) ->
            match msgOpt with
            | Some expr -> validateExpr ctx expr
            | None -> ctx
        | Break(pos) ->
            if not ctx.Scope.InLoop then
                let loc = SourceLocation.Create(pos.Line, pos.Column)
                ctx.AddError(MessageCode.LoopStatementOutsideOfLoop, "Break statement outside of loop", [| box "Break" |], loc)
            else
                ctx
        | Continue(pos) ->
            if not ctx.Scope.InLoop then
                let loc = SourceLocation.Create(pos.Line, pos.Column)
                ctx.AddError(MessageCode.LoopStatementOutsideOfLoop, "Continue statement outside of loop", [| box "Continue" |], loc)
            else
                ctx
        | ExprStmt(expr, _) ->
            validateExpr ctx expr

    /// Validate a program
    let validate (program: Program) : JyroResult<Program> =
        let ctx = program.Statements |> List.fold validateStmt ValidationContext.Empty
        let messages = ctx.Messages |> List.rev
        if messages |> List.exists (fun m -> m.Severity = MessageSeverity.Error) then
            JyroResult<Program>.Failure(messages)
        else
            { Value = Some program; Messages = messages; IsSuccess = true }
