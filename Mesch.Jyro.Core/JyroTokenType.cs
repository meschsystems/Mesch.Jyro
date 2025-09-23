namespace Mesch.Jyro;

/// <summary>
/// Defines the complete set of lexical token types recognized by the Jyro lexer.
/// These tokens represent the fundamental building blocks of Jyro source code
/// and are used throughout the parsing and compilation pipeline.
/// </summary>
public enum JyroTokenType
{
    #region Keywords 

    #region Variable declaration

    /// <summary>
    /// Represents the 'var' keyword used for variable declarations.
    /// </summary>
    Var,

    #endregion

    #region Type keywords

    /// <summary>
    /// Represents the 'number' type keyword for numeric type annotations.
    /// </summary>
    NumberType,

    /// <summary>
    /// Represents the 'string' type keyword for string type annotations.
    /// </summary>
    StringType,

    /// <summary>
    /// Represents the 'boolean' type keyword for boolean type annotations.
    /// </summary>
    BooleanType,

    /// <summary>
    /// Represents the 'object' type keyword for object type annotations.
    /// </summary>
    ObjectType,

    /// <summary>
    /// Represents the 'array' type keyword for array type annotations.
    /// </summary>
    ArrayType,

    #endregion

    #region Flow control keywords

    /// <summary>
    /// Represents the 'if' keyword used to begin conditional statements.
    /// </summary>
    If,

    /// <summary>
    /// Represents the 'then' keyword used in conditional statements after the condition.
    /// </summary>
    Then,

    /// <summary>
    /// Represents the 'else' keyword used for alternative branches in conditional statements.
    /// </summary>
    Else,

    /// <summary>
    /// Represents the 'end' keyword used to terminate block statements.
    /// </summary>
    End,

    /// <summary>
    /// Represents the 'switch' keyword used to begin switch statements.
    /// </summary>
    Switch,

    /// <summary>
    /// Represents the 'case' keyword used for individual cases in switch statements.
    /// </summary>
    Case,

    /// <summary>
    /// Represents the 'default' keyword used for the default case in switch statements.
    /// </summary>
    Default,

    /// <summary>
    /// Represents the 'return' keyword used to exit from functions or procedures.
    /// </summary>
    Return,

    #endregion

    #region Loop keywords

    /// <summary>
    /// Represents the 'foreach' keyword used to begin iteration over collections.
    /// </summary>
    ForEach,

    /// <summary>
    /// Represents the 'in' keyword used in foreach loops to specify the collection.
    /// </summary>
    In,

    /// <summary>
    /// Represents the 'do' keyword used in loop constructs to begin the loop body.
    /// </summary>
    Do,

    /// <summary>
    /// Represents the 'while' keyword used for conditional loops.
    /// </summary>
    While,

    /// <summary>
    /// Represents the 'break' keyword used to exit from loops prematurely.
    /// </summary>
    Break,

    /// <summary>
    /// Represents the 'continue' keyword used to skip to the next iteration of a loop.
    /// </summary>
    Continue,

    #endregion

    #region Logical operators

    /// <summary>
    /// Represents the 'and' logical operator for boolean conjunction.
    /// </summary>
    And,

    /// <summary>
    /// Represents the 'or' logical operator for boolean disjunction.
    /// </summary>
    Or,

    /// <summary>
    /// Represents the 'not' logical operator for boolean negation.
    /// </summary>
    Not,

    #endregion

    #region Type checking

    /// <summary>
    /// Represents the 'is' operator used for type checking and comparison.
    /// </summary>
    Is,

    #endregion

    #region Literals

    /// <summary>
    /// Represents boolean literal values ('true' or 'false').
    /// </summary>
    BooleanLiteral,

    /// <summary>
    /// Represents the 'null' literal value.
    /// </summary>
    NullLiteral,

    #endregion

    #endregion

    #region Operators

    /// <summary>
    /// Represents the '+' operator for addition and string concatenation.
    /// </summary>
    Plus,

    /// <summary>
    /// Represents the '-' operator for subtraction and numeric negation.
    /// </summary>
    Minus,

    /// <summary>
    /// Represents the '*' operator for multiplication.
    /// </summary>
    Star,

    /// <summary>
    /// Represents the '/' operator for division.
    /// </summary>
    Slash,

    /// <summary>
    /// Represents the '%' operator for modulo arithmetic.
    /// </summary>
    Percent,

    /// <summary>
    /// Represents the '=' operator for assignment.
    /// </summary>
    Equal,

    /// <summary>
    /// Represents the '==' operator for equality comparison.
    /// </summary>
    EqualEqual,

    /// <summary>
    /// Represents the '!' operator for logical negation (alternative to 'not').
    /// </summary>
    Bang,

    /// <summary>
    /// Represents the '!=' operator for inequality comparison.
    /// </summary>
    BangEqual,

    /// <summary>
    /// Represents the '>' operator for greater-than comparison.
    /// </summary>
    Greater,

    /// <summary>
    /// Represents the '>=' operator for greater-than-or-equal comparison.
    /// </summary>
    GreaterEqual,

    /// <summary>
    /// Represents the '&lt;' operator for less-than comparison.
    /// </summary>
    Less,

    /// <summary>
    /// Represents the '&lt;=' operator for less-than-or-equal comparison.
    /// </summary>
    LessEqual,

    /// <summary>
    /// Represents the '?' operator used in ternary conditional expressions.
    /// </summary>
    QuestionMark,

    #endregion

    #region Punctuation

    /// <summary>
    /// Represents the '(' character for grouping expressions and function calls.
    /// </summary>
    LeftParenthesis,

    /// <summary>
    /// Represents the ')' character for closing grouping expressions and function calls.
    /// </summary>
    RightParenthesis,

    /// <summary>
    /// Represents the '[' character for array indexing and array literals.
    /// </summary>
    LeftBracket,

    /// <summary>
    /// Represents the ']' character for closing array indexing and array literals.
    /// </summary>
    RightBracket,

    /// <summary>
    /// Represents the '{' character for object literals and block statements.
    /// </summary>
    LeftBrace,

    /// <summary>
    /// Represents the '}' character for closing object literals and block statements.
    /// </summary>
    RightBrace,

    /// <summary>
    /// Represents the ',' character for separating elements in lists and parameters.
    /// </summary>
    Comma,

    /// <summary>
    /// Represents the '.' character for property access and member navigation.
    /// </summary>
    Dot,

    /// <summary>
    /// Represents the ':' character for type annotations and object property definitions.
    /// </summary>
    Colon,

    #endregion

    #region Literals and Identifiers

    /// <summary>
    /// Represents user-defined identifiers such as variable names and function names.
    /// </summary>
    Identifier,

    /// <summary>
    /// Represents numeric literal values (integers and floating-point numbers).
    /// </summary>
    NumberLiteral,

    /// <summary>
    /// Represents string literal values enclosed in double quotes.
    /// </summary>
    StringLiteral,

    #endregion

    #region Special

    /// <summary>
    /// Represents the end of the input source, indicating no more tokens are available.
    /// This token is used by the parser to detect the completion of parsing.
    /// </summary>
    EndOfFile,

    #endregion
}