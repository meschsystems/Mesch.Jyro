namespace Mesch.Jyro

open System

/// Exception thrown during Jyro script execution with a diagnostic code and structured arguments.
type JyroRuntimeException(code: MessageCode, message: string, args: obj[], line: int, column: int) =
    inherit Exception(message)
    new(code, message, args) = JyroRuntimeException(code, message, args, 0, 0)
    new(code, message) = JyroRuntimeException(code, message, Array.empty, 0, 0)
    new(code, message, line, column) = JyroRuntimeException(code, message, Array.empty, line, column)
    member _.Code = code
    member _.Args = args
    member _.Line = line
    member _.Column = column
    member _.HasLocation = line > 0

/// Helper module for raising JyroRuntimeException with template-formatted messages.
module JyroError =
    /// Raise a JyroRuntimeException using the default English template for the given code.
    let raiseRuntime (code: MessageCode) (args: obj[]) : 'T =
        let template = MessageTemplates.get code
        let message = if args.Length > 0 then String.Format(template, args) else template
        raise (JyroRuntimeException(code, message, args))
