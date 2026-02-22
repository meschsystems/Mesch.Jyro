namespace Mesch.Jyro

open Mesch.Jyro.Lexer
open Mesch.Jyro.StatementParser

/// Main parser module for the Jyro language
module Parser =
    open FParsec

    /// Parse a complete Jyro program
    let private pProgram : Parser<Program, unit> =
        initParser >>. parseStmts .>> eof
        |>> fun stmts -> { Statements = stmts }

    /// Result of parsing a Jyro script
    type JyroParseResult =
        | ParseSuccess of Program
        | ParseFailure of message: string * line: int * column: int

    /// Parse a Jyro script from a string
    let parse (source: string) : JyroParseResult =
        let result = runParserOnString pProgram () "script" source
        match result with
        | ParserResult.Success(program, _, _) ->
            ParseSuccess program
        | ParserResult.Failure(msg, err, _) ->
            ParseFailure(msg, int err.Position.Line, int err.Position.Column)

    /// Parse a Jyro script and return a JyroResult
    let parseToResult (source: string) : JyroResult<Program> =
        match parse source with
        | ParseSuccess program ->
            JyroResult<Program>.Success(program)
        | ParseFailure(msg, line, column) ->
            let location = SourceLocation.Create(line, column)
            JyroResult<Program>.Failure(DiagnosticMessage.Error(MessageCode.UnknownParserError, msg, args = [| box msg |], location = location))

    /// Parse a single expression from a string
    let parseExpression (source: string) : Result<Expr, string> =
        let parser : Parser<Expr, unit> = initParser >>. ExpressionParser.parseExpr .>> eof
        let result = runParserOnString parser () "expression" source
        match result with
        | ParserResult.Success(expr, _, _) -> Result.Ok expr
        | ParserResult.Failure(msg, _, _) -> Result.Error msg
