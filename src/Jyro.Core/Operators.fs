namespace Mesch.Jyro

/// Binary operators supported by Jyro
type BinaryOp =
    // Arithmetic
    | Add
    | Subtract
    | Multiply
    | Divide
    | Modulo
    // Comparison
    | Equal
    | NotEqual
    | LessThan
    | LessThanOrEqual
    | GreaterThan
    | GreaterThanOrEqual
    // Logical
    | And
    | Or
    // Null coalescing
    | Coalesce

/// Unary operators supported by Jyro
type UnaryOp =
    | Negate
    | Not

/// Assignment operators supported by Jyro
type AssignOp =
    | Assign
    | AddAssign
    | SubtractAssign
    | MultiplyAssign
    | DivideAssign
    | ModuloAssign

/// Helper module for operator utilities
module Operators =
    /// Get the string representation of a binary operator
    let binaryOpToString = function
        | Add -> "+"
        | Subtract -> "-"
        | Multiply -> "*"
        | Divide -> "/"
        | Modulo -> "%"
        | Equal -> "=="
        | NotEqual -> "!="
        | LessThan -> "<"
        | LessThanOrEqual -> "<="
        | GreaterThan -> ">"
        | GreaterThanOrEqual -> ">="
        | And -> "and"
        | Or -> "or"
        | Coalesce -> "??"

    /// Get the string representation of a unary operator
    let unaryOpToString = function
        | Negate -> "-"
        | Not -> "not"

    /// Get the string representation of an assignment operator
    let assignOpToString = function
        | Assign -> "="
        | AddAssign -> "+="
        | SubtractAssign -> "-="
        | MultiplyAssign -> "*="
        | DivideAssign -> "/="
        | ModuloAssign -> "%="

    /// Get the precedence of a binary operator (higher = binds tighter)
    let binaryOpPrecedence = function
        | Or -> 1
        | And -> 2
        | Equal | NotEqual -> 3
        | LessThan | LessThanOrEqual | GreaterThan | GreaterThanOrEqual -> 4
        | Add | Subtract -> 5
        | Multiply | Divide | Modulo -> 6
        | Coalesce -> 7
