namespace Mesch.Jyro;

public sealed partial class Parser
{
    // Contains utility methods and helper functions that support the main parsing operations.
    // These methods provide common functionality used across different parsing contexts
    // and maintain consistency in error handling and diagnostic reporting.
    // Most token stream manipulation is now handled by the TokenStream class itself,
    // which provides a cleaner abstraction and better encapsulation of stream state.
    // These helpers focus on parsing-specific utilities that don't belong in the
    // general-purpose TokenStream implementation.

    #region Diagnostic Helpers

    /// <summary>
    /// Adds an error-level diagnostic message to the specified collection with parser-specific context.
    /// This method provides a consistent way to report parsing errors with proper categorization.
    /// </summary>
    /// <param name="messageCollection">The collection to add the diagnostic message to.</param>
    /// <param name="errorCode">The specific error code that categorizes the type of parsing error.</param>
    /// <param name="lineNumber">The line number where the error occurred in the source code.</param>
    /// <param name="columnNumber">The column position where the error occurred in the source code.</param>
    /// <param name="messageArguments">Optional arguments for message formatting and localization.</param>
    private static void AddParsingError(List<IMessage> messageCollection, MessageCode errorCode, int lineNumber, int columnNumber, params string[] messageArguments) =>
        messageCollection.Add(new Message(errorCode, lineNumber, columnNumber, MessageSeverity.Error, ProcessingStage.Parsing, messageArguments));

    #endregion
}