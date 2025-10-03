﻿namespace Mesch.Jyro;

/// <summary>
/// Provides the default message formatting implementation for Jyro diagnostic messages.
/// This implementation offers English-language templates with standardized diagnostic codes
/// and consistent formatting suitable for development environments and user interfaces.
/// </summary>
/// <remarks>
/// The default message provider serves as both the reference implementation and a production-ready
/// solution for most Jyro applications. It provides:
/// 
/// <list type="bullet">
/// <item><description>Comprehensive message templates covering all diagnostic codes</description></item>
/// <item><description>Consistent formatting with location information and diagnostic codes</description></item>
/// <item><description>Parameterized messages supporting context-specific information</description></item>
/// <item><description>Clear, user-friendly text suitable for both developers and end users</description></item>
/// </list>
/// 
/// Organizations requiring localization or custom formatting can implement the IMessageProvider
/// interface to provide specialized message formatting while maintaining compatibility with
/// the Jyro diagnostic system.
/// </remarks>
public sealed class MessageProvider : IMessageProvider
{
    private readonly Dictionary<MessageCode, string> _messageTemplates = new()
    {
        // Lexical Analysis Templates
        { MessageCode.UnexpectedCharacter, "Unexpected character '{0}'" },
        { MessageCode.UnterminatedString, "Unterminated string literal" },
        
        // Parsing Templates
        { MessageCode.UnexpectedToken, "Unexpected token '{0}'" },
        { MessageCode.MissingToken, "Missing token '{0}'" },
        { MessageCode.InvalidNumberFormat, "Invalid number format: '{0}'" },
        
        // Validation Templates
        { MessageCode.InvalidVariableReference, "Invalid variable reference '{0}'" },
        { MessageCode.InvalidAssignmentTarget, "Invalid assignment target '{0}'" },
        { MessageCode.TypeMismatch, "Type mismatch: expected {0}, got {1}" },
        { MessageCode.LoopStatementOutsideOfLoop, "Loop control statement '{0}' used outside of loop context" },
        { MessageCode.ExcessiveLoopNesting, "Loop nesting depth exceeded maximum limit of {0}" },
        { MessageCode.UnreachableCode, "Unreachable code detected after {0}" },
        
        // Linking Templates
        { MessageCode.UndefinedFunction, "Undefined function '{0}'" },
        { MessageCode.DuplicateFunction, "Duplicate function definition '{0}'" },
        { MessageCode.FunctionOverride, "Function '{0}' overrides existing definition" },
        { MessageCode.InvalidNumberArguments, "Function '{0}' expects {1} arguments, but {2} were provided" },
        
        // Execution Templates
        { MessageCode.RuntimeError, "Runtime error: {0}" },
        { MessageCode.CancelledByHost, "Execution was cancelled by the host application" },
        { MessageCode.InvalidType, "Invalid type '{0}' for operation '{1}'" },
        { MessageCode.InvalidArgumentType, "Function '{0}' expects argument {1} to be of type {2}, but got {3}" },
        
        // General Error Templates
        { MessageCode.UnknownExecutorError, "Unknown execution error: {0}" },
        { MessageCode.UnknownLexerError, "Unknown lexical analysis error: {0}" },
        { MessageCode.UnknownParserError, "Unknown parsing error: {0}" },
        { MessageCode.UnknownValidatorError, "Unknown validation error: {0}" },
        { MessageCode.UnknownLinkerError, "Unknown linking error: {0}" }
    };

    /// <summary>
    /// Formats a diagnostic message into a comprehensive human-readable string with location information,
    /// diagnostic code, and formatted message text with argument substitution.
    /// </summary>
    /// <param name="message">
    /// The diagnostic message containing all information needed for formatting.
    /// </param>
    /// <returns>
    /// A formatted string in the format: "Line {line}, Column {column}, JM{code}: {formatted_message}"
    /// where the message text includes any provided arguments substituted into the template.
    /// </returns>
    public string Format(IMessage message)
    {
        var diagnosticCode = $"JM{(int)message.Code:D4}";
        var messageTemplate = GetTemplate(message.Code) ?? message.Code.ToString();

        var formattedMessageText = message.Arguments.Count > 0
            ? string.Format(messageTemplate, message.Arguments.ToArray())
            : messageTemplate;

        return $"Line {message.LineNumber}, Column {message.ColumnPosition}, {diagnosticCode}: {formattedMessageText}";
    }

    /// <summary>
    /// Retrieves the default message template for the specified diagnostic code.
    /// Templates use standard .NET string formatting placeholders ({0}, {1}, etc.) for argument substitution.
    /// </summary>
    /// <param name="code">
    /// The diagnostic code for which to retrieve the message template.
    /// </param>
    /// <returns>
    /// The message template string with formatting placeholders, or null if no template
    /// is defined for the specified code. Undefined codes will fall back to the code name itself.
    /// </returns>
    public string? GetTemplate(MessageCode code) =>
        _messageTemplates.TryGetValue(code, out var template) ? template : null;
}