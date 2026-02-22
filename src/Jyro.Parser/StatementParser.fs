namespace Mesch.Jyro

open FParsec.Primitives
open FParsec.CharParsers
open Mesch.Jyro.Lexer
open Mesch.Jyro.PositionTracking
open Mesch.Jyro.ExpressionParser

/// Statement parser for the Jyro language
module StatementParser =
    // Forward reference for recursive statement parsing
    let pStmt, pStmtRef = createParserForwardedToRef<Stmt, unit>()

    /// Parse a block of statements
    let pBlock = many pStmt

    /// Skip horizontal whitespace only (spaces and tabs, not newlines)
    let private hws : Parser<unit, unit> = skipMany (skipSatisfy (fun c -> c = ' ' || c = '\t'))

    /// Parse a keyword without consuming newline-crossing whitespace
    let private keywordRaw kw : Parser<unit, unit> =
        attempt (pstring kw .>> notFollowedBy (satisfy (fun c -> System.Char.IsLetterOrDigit(c) || c = '_'))) >>% ()

    /// Parse an optional expression that must start on the same line as the preceding keyword
    let private optSameLineExpr : Parser<Expr option, unit> =
        hws >>. (
            (followedBy ((newline >>% ()) <|> (pchar '#' >>% ()) <|> eof) >>. ws >>% None)
            <|>
            (parseExpr |>> Some)
        )

    // Variable declaration: var name [: type] [= expr]
    let private pVarDecl =
        withPos (
            keyword "var" >>. identifier .>>.
            opt (colon >>. typeKeyword) .>>.
            opt (symbolOp "=" >>. parseExpr)
        )
        |>> fun (((name, typeHint), init), pos) -> VarDecl(name, typeHint, init, pos)

    // Assignment statement: target op= value
    let private pAssignment =
        withPos (
            parseExpr .>>. assignOp .>>. parseExpr
        )
        |>> fun (((target, op), value), pos) ->
            if not (Ast.isAssignmentTarget target) then
                failwith "Invalid assignment target"
            Assignment(target, op, value, pos)

    // If statement
    let private pElseIf =
        keyword "elseif" >>. parseExpr .>> keyword "then" .>>. pBlock

    let private pElse =
        keyword "else" >>. pBlock

    let private pIf =
        withPos (
            keyword "if" >>. parseExpr .>> keyword "then" .>>.
            pBlock .>>.
            many pElseIf .>>.
            opt pElse .>>
            keyword "end"
        )
        |>> fun ((((cond, thenBlock), elseIfs), elseBlock), pos) ->
            If(cond, thenBlock, elseIfs, elseBlock, pos)

    // While loop
    let private pWhile =
        withPos (
            keyword "while" >>. parseExpr .>> keyword "do" .>>.
            pBlock .>>
            keyword "end"
        )
        |>> fun ((cond, body), pos) -> While(cond, body, pos)

    // Range-based for loop: for varName in startExpr to|downto endExpr [by stepExpr] do ... end
    let private pFor =
        withPos (
            keyword "for" >>. identifier .>> keyword "in" .>>. parseExpr .>>.
            ((keyword "downto" >>% Descending) <|> (keyword "to" >>% Ascending)) .>>.
            parseExpr .>>.
            opt (keyword "by" >>. parseExpr) .>>
            keyword "do" .>>.
            pBlock .>>
            keyword "end"
        )
        |>> fun ((((((varName, startExpr), direction), endExpr), stepExpr), body), pos) ->
            For(varName, startExpr, endExpr, stepExpr, direction, body, pos)

    // Foreach loop
    let private pForEach =
        withPos (
            keyword "foreach" >>. identifier .>> keyword "in" .>>. parseExpr .>> keyword "do" .>>.
            pBlock .>>
            keyword "end"
        )
        |>> fun (((varName, collection), body), pos) -> ForEach(varName, collection, body, pos)

    // Switch statement
    let private pCaseValues = sepBy1 parseExpr comma

    let private pCase =
        keyword "case" >>. pCaseValues .>> keyword "then" .>>. pBlock
        |>> fun (values, body) -> { Values = values; Body = body }

    let private pDefaultCase =
        keyword "default" >>. keyword "then" >>. pBlock

    let private pSwitch =
        withPos (
            keyword "switch" >>. parseExpr .>> keyword "do" .>>.
            many pCase .>>.
            opt pDefaultCase .>>
            keyword "end"
        )
        |>> fun (((expr, cases), defaultCase), pos) -> Switch(expr, cases, defaultCase, pos)

    // Return statement (expression must be on same line as keyword)
    let private pReturn =
        withPos (keywordRaw "return" >>. optSameLineExpr)
        |>> fun (value, pos) -> Return(value, pos)

    // Fail statement (message must be on same line as keyword)
    let private pFail =
        withPos (keywordRaw "fail" >>. optSameLineExpr)
        |>> fun (message, pos) -> Fail(message, pos)

    // Break statement
    let private pBreak =
        withPos (keyword "break")
        |>> fun (_, pos) -> Break(pos)

    // Continue statement
    let private pContinue =
        withPos (keyword "continue")
        |>> fun (_, pos) -> Continue(pos)

    // Expression statement (expression evaluated for side effects)
    let private pExprStmt =
        withPos parseExpr
        |>> fun (expr, pos) -> ExprStmt(expr, pos)

    // All statements
    do pStmtRef.Value <-
        choice [
            attempt pVarDecl
            attempt pIf
            attempt pWhile
            attempt pFor
            attempt pForEach
            attempt pSwitch
            attempt pReturn
            attempt pFail
            attempt pBreak
            attempt pContinue
            attempt pAssignment
            attempt pExprStmt
        ]

    /// Parse a single statement
    let parseStmt = pStmt

    /// Parse multiple statements
    let parseStmts = pBlock
