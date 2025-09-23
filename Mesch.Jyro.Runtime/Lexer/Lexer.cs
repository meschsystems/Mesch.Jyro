using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Mesch.Jyro;

/// <summary>
/// Provides the default implementation of the Jyro lexical analyzer.
/// This lexer converts Jyro script source text into a structured sequence of tokens
/// for consumption by the parser, handling all language constructs including keywords,
/// operators, literals, identifiers, and comments.
/// </summary>
/// <remarks>
/// The lexer implementation follows a single-pass scanning approach, processing the source
/// text character by character to identify and classify tokens according to the Jyro
/// language specification. It provides comprehensive error handling with precise location
/// information and supports robust recovery from lexical errors.
/// 
/// <para>
/// Key features include:
/// <list type="bullet">
/// <item><description>Complete keyword recognition using the language specification</description></item>
/// <item><description>Numeric literal parsing with decimal support</description></item>
/// <item><description>String literal processing with proper escaping</description></item>
/// <item><description>Operator tokenization including multi-character operators</description></item>
/// <item><description>Comment handling and whitespace normalization</description></item>
/// <item><description>Precise error location tracking for diagnostic reporting</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class Lexer : ILexer
{
    private readonly ILogger<Lexer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Lexer"/> class with the specified logger.
    /// </summary>
    /// <param name="logger">
    /// The logger instance used for diagnostic output and debugging information during tokenization.
    /// Provides visibility into the lexing process for development and troubleshooting purposes.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public Lexer(ILogger<Lexer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Tokenizes the provided source text and returns a comprehensive result containing
    /// the token sequence, diagnostic messages, and processing metadata.
    /// </summary>
    /// <param name="source">The Jyro source code to tokenize.</param>
    /// <returns>
    /// A <see cref="JyroLexingResult"/> containing the complete tokenization outcome.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    public JyroLexingResult Tokenize(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var processingStartTime = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        var tokenCollection = new List<JyroToken>();
        var diagnosticMessages = new List<IMessage>();

        try
        {
            _logger.LogTrace("Lexical analysis started for source of length {SourceLength}", source.Length);
            ScanTokens(source, tokenCollection, diagnosticMessages);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error during lexical analysis");
            diagnosticMessages.Add(new Message(
                MessageCode.UnknownLexerError,
                0, 0,
                MessageSeverity.Error,
                ProcessingStage.Lexing,
                exception.Message));
        }

        stopwatch.Stop();
        var isSuccessful = !diagnosticMessages.Any(message => message.Severity == MessageSeverity.Error);
        var processingMetadata = new LexingMetadata(stopwatch.Elapsed, tokenCollection.Count, processingStartTime);

        _logger.LogTrace("Lexical analysis completed: success={IsSuccessful}, tokens={TokenCount}, errors={ErrorCount}, elapsed={ElapsedMilliseconds}ms",
            isSuccessful, tokenCollection.Count, diagnosticMessages.Count(message => message.Severity == MessageSeverity.Error), stopwatch.ElapsedMilliseconds);

        return new JyroLexingResult(isSuccessful, tokenCollection, diagnosticMessages, processingMetadata);
    }

    /// <summary>
    /// Performs the core tokenization logic, scanning through the source text character by character
    /// to identify and extract tokens while handling whitespace, comments, and error conditions.
    /// </summary>
    /// <param name="sourceText">The source code to scan for tokens.</param>
    /// <param name="tokenCollection">The collection to populate with discovered tokens.</param>
    /// <param name="diagnosticMessages">The collection to populate with error and warning messages.</param>
    private void ScanTokens(string sourceText, List<JyroToken> tokenCollection, List<IMessage> diagnosticMessages)
    {
        var currentPosition = 0;
        var currentLine = 1;
        var currentColumn = 1;

        while (!IsAtEndOfSource(sourceText, currentPosition))
        {
            var tokenStartPosition = currentPosition;
            var tokenStartLine = currentLine;
            var tokenStartColumn = currentColumn;

            // Skip whitespace and comments in a loop until no more can be skipped
            bool contentWasSkipped;
            do
            {
                contentWasSkipped = false;
                var positionBeforeSkipping = currentPosition;

                SkipWhitespace(sourceText, ref currentPosition, ref currentLine, ref currentColumn);
                SkipComments(sourceText, ref currentPosition, ref currentLine, ref currentColumn);

                if (currentPosition != positionBeforeSkipping)
                {
                    contentWasSkipped = true;
                }
            }
            while (contentWasSkipped);

            if (IsAtEndOfSource(sourceText, currentPosition))
            {
                break;
            }

            var discoveredToken = ScanToken(sourceText, ref currentPosition, ref currentLine, ref currentColumn,
                tokenStartLine, tokenStartColumn, diagnosticMessages);
            if (discoveredToken != null)
            {
                tokenCollection.Add(discoveredToken);
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("Token discovered: {TokenType} '{Lexeme}' at {Line}:{Column}",
                        discoveredToken.Type, discoveredToken.Lexeme, discoveredToken.LineNumber, discoveredToken.ColumnPosition);
                }
            }

            // Ensure forward progress to prevent infinite loops
            if (currentPosition == tokenStartPosition)
            {
                var unexpectedCharacter = sourceText[currentPosition];
                _logger.LogTrace("Unexpected character '{Character}' encountered at {Line}:{Column}",
                    unexpectedCharacter, tokenStartLine, tokenStartColumn);
                AddDiagnosticError(diagnosticMessages, MessageCode.UnexpectedCharacter, tokenStartLine, tokenStartColumn, unexpectedCharacter.ToString());
                AdvancePosition(sourceText, ref currentPosition, ref currentLine, ref currentColumn);
            }
        }

        tokenCollection.Add(new JyroToken(JyroTokenType.EndOfFile, string.Empty, currentLine, currentColumn));
    }

    /// <summary>
    /// Attempts to scan and classify a single token starting at the current position in the source text.
    /// </summary>
    /// <param name="sourceText">The source code being tokenized.</param>
    /// <param name="currentPosition">The current position in the source text (updated by reference).</param>
    /// <param name="currentLine">The current line number (updated by reference).</param>
    /// <param name="currentColumn">The current column position (updated by reference).</param>
    /// <param name="tokenStartLine">The line number where this token begins.</param>
    /// <param name="tokenStartColumn">The column position where this token begins.</param>
    /// <param name="diagnosticMessages">The collection to add any error messages to.</param>
    /// <returns>The scanned token, or null if an error occurred.</returns>
    private JyroToken? ScanToken(
        string sourceText,
        ref int currentPosition,
        ref int currentLine,
        ref int currentColumn,
        int tokenStartLine,
        int tokenStartColumn,
        List<IMessage> diagnosticMessages)
    {
        var currentCharacter = sourceText[currentPosition];

        // Handle punctuation and single-character operators
        switch (currentCharacter)
        {
            case '(': return ConsumeSingleCharacterToken(JyroTokenType.LeftParenthesis, "(", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
            case ')': return ConsumeSingleCharacterToken(JyroTokenType.RightParenthesis, ")", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
            case '[': return ConsumeSingleCharacterToken(JyroTokenType.LeftBracket, "[", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
            case ']': return ConsumeSingleCharacterToken(JyroTokenType.RightBracket, "]", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
            case '{': return ConsumeSingleCharacterToken(JyroTokenType.LeftBrace, "{", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
            case '}': return ConsumeSingleCharacterToken(JyroTokenType.RightBrace, "}", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
            case ':': return ConsumeSingleCharacterToken(JyroTokenType.Colon, ":", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
            case ',': return ConsumeSingleCharacterToken(JyroTokenType.Comma, ",", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
            case '.': return ConsumeSingleCharacterToken(JyroTokenType.Dot, ".", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
            case '+': return ConsumeSingleCharacterToken(JyroTokenType.Plus, "+", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
            case '-': return ConsumeSingleCharacterToken(JyroTokenType.Minus, "-", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
            case '*': return ConsumeSingleCharacterToken(JyroTokenType.Star, "*", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
            case '/': return ConsumeSingleCharacterToken(JyroTokenType.Slash, "/", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
            case '%': return ConsumeSingleCharacterToken(JyroTokenType.Percent, "%", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
            case '?': return ConsumeSingleCharacterToken(JyroTokenType.QuestionMark, "?", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
        }

        // Handle multi-character operators
        if (currentCharacter == '=' && PeekNextCharacter(sourceText, currentPosition + 1) == '=')
        {
            return ConsumeDoubleCharacterToken(JyroTokenType.EqualEqual, "==", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
        }
        if (currentCharacter == '!' && PeekNextCharacter(sourceText, currentPosition + 1) == '=')
        {
            return ConsumeDoubleCharacterToken(JyroTokenType.BangEqual, "!=", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
        }
        if (currentCharacter == '<' && PeekNextCharacter(sourceText, currentPosition + 1) == '=')
        {
            return ConsumeDoubleCharacterToken(JyroTokenType.LessEqual, "<=", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
        }
        if (currentCharacter == '>' && PeekNextCharacter(sourceText, currentPosition + 1) == '=')
        {
            return ConsumeDoubleCharacterToken(JyroTokenType.GreaterEqual, ">=", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
        }

        // Handle remaining single-character comparison operators
        switch (currentCharacter)
        {
            case '=': return ConsumeSingleCharacterToken(JyroTokenType.Equal, "=", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
            case '!': return ConsumeSingleCharacterToken(JyroTokenType.Bang, "!", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
            case '<': return ConsumeSingleCharacterToken(JyroTokenType.Less, "<", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
            case '>': return ConsumeSingleCharacterToken(JyroTokenType.Greater, ">", sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
        }

        // Handle string literals
        if (currentCharacter == '"')
        {
            return ScanStringLiteral(sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn, diagnosticMessages);
        }

        // Handle numeric literals
        if (IsDigitCharacter(currentCharacter))
        {
            return ScanNumericLiteral(sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
        }

        // Handle identifiers and keywords
        if (IsAlphabeticCharacter(currentCharacter))
        {
            return ScanIdentifierOrKeyword(sourceText, ref currentPosition, ref currentLine, ref currentColumn, tokenStartLine, tokenStartColumn);
        }

        // Handle unrecognized characters
        AddDiagnosticError(diagnosticMessages, MessageCode.UnexpectedCharacter, tokenStartLine, tokenStartColumn, currentCharacter.ToString());
        AdvancePosition(sourceText, ref currentPosition, ref currentLine, ref currentColumn);
        return null;
    }

    /// <summary>
    /// Consumes a single character token and advances the position.
    /// </summary>
    private static JyroToken ConsumeSingleCharacterToken(JyroTokenType tokenType, string lexeme, string sourceText,
        ref int currentPosition, ref int currentLine, ref int currentColumn, int tokenStartLine, int tokenStartColumn)
    {
        AdvancePosition(sourceText, ref currentPosition, ref currentLine, ref currentColumn);
        return new JyroToken(tokenType, lexeme, tokenStartLine, tokenStartColumn);
    }

    /// <summary>
    /// Consumes a two-character token and advances the position.
    /// </summary>
    private static JyroToken ConsumeDoubleCharacterToken(JyroTokenType tokenType, string lexeme, string sourceText,
        ref int currentPosition, ref int currentLine, ref int currentColumn, int tokenStartLine, int tokenStartColumn)
    {
        AdvancePosition(sourceText, ref currentPosition, ref currentLine, ref currentColumn);
        AdvancePosition(sourceText, ref currentPosition, ref currentLine, ref currentColumn);
        return new JyroToken(tokenType, lexeme, tokenStartLine, tokenStartColumn);
    }

    /// <summary>
    /// Scans a string literal token, handling proper termination and error conditions.
    /// </summary>
    private JyroToken? ScanStringLiteral(
        string sourceText,
        ref int currentPosition,
        ref int currentLine,
        ref int currentColumn,
        int tokenStartLine,
        int tokenStartColumn,
        List<IMessage> diagnosticMessages)
    {
        var stringBuilder = new StringBuilder();
        AdvancePosition(sourceText, ref currentPosition, ref currentLine, ref currentColumn); // Skip opening quote

        while (!IsAtEndOfSource(sourceText, currentPosition) && sourceText[currentPosition] != '"')
        {
            if (sourceText[currentPosition] == '\n')
            {
                _logger.LogTrace("Unterminated string literal detected at {Line}:{Column}", tokenStartLine, tokenStartColumn);
                AddDiagnosticError(diagnosticMessages, MessageCode.UnterminatedString, tokenStartLine, tokenStartColumn);
                return null;
            }
            stringBuilder.Append(sourceText[currentPosition]);
            AdvancePosition(sourceText, ref currentPosition, ref currentLine, ref currentColumn);
        }

        if (IsAtEndOfSource(sourceText, currentPosition))
        {
            _logger.LogTrace("Unterminated string literal at end of file, started at {Line}:{Column}", tokenStartLine, tokenStartColumn);
            AddDiagnosticError(diagnosticMessages, MessageCode.UnterminatedString, tokenStartLine, tokenStartColumn);
            return null;
        }

        AdvancePosition(sourceText, ref currentPosition, ref currentLine, ref currentColumn); // Skip closing quote
        return new JyroToken(JyroTokenType.StringLiteral, stringBuilder.ToString(), tokenStartLine, tokenStartColumn);
    }

    /// <summary>
    /// Scans a numeric literal token, handling both integer and decimal formats.
    /// </summary>
    private static JyroToken ScanNumericLiteral(
        string sourceText,
        ref int currentPosition,
        ref int currentLine,
        ref int currentColumn,
        int tokenStartLine,
        int tokenStartColumn)
    {
        var stringBuilder = new StringBuilder();

        // Consume integer part
        while (!IsAtEndOfSource(sourceText, currentPosition) && IsDigitCharacter(sourceText[currentPosition]))
        {
            stringBuilder.Append(sourceText[currentPosition]);
            AdvancePosition(sourceText, ref currentPosition, ref currentLine, ref currentColumn);
        }

        // Handle decimal point and fractional part
        if (!IsAtEndOfSource(sourceText, currentPosition) && sourceText[currentPosition] == '.' && IsDigitCharacter(PeekNextCharacter(sourceText, currentPosition + 1)))
        {
            stringBuilder.Append('.');
            AdvancePosition(sourceText, ref currentPosition, ref currentLine, ref currentColumn);

            while (!IsAtEndOfSource(sourceText, currentPosition) && IsDigitCharacter(sourceText[currentPosition]))
            {
                stringBuilder.Append(sourceText[currentPosition]);
                AdvancePosition(sourceText, ref currentPosition, ref currentLine, ref currentColumn);
            }
        }

        return new JyroToken(JyroTokenType.NumberLiteral, stringBuilder.ToString(), tokenStartLine, tokenStartColumn);
    }

    /// <summary>
    /// Scans an identifier or keyword token, determining the appropriate classification.
    /// </summary>
    private static JyroToken ScanIdentifierOrKeyword(
        string sourceText,
        ref int currentPosition,
        ref int currentLine,
        ref int currentColumn,
        int tokenStartLine,
        int tokenStartColumn)
    {
        var stringBuilder = new StringBuilder();

        while (!IsAtEndOfSource(sourceText, currentPosition) && IsAlphanumericCharacter(sourceText[currentPosition]))
        {
            stringBuilder.Append(sourceText[currentPosition]);
            AdvancePosition(sourceText, ref currentPosition, ref currentLine, ref currentColumn);
        }

        var identifierText = stringBuilder.ToString();
        var tokenType = JyroLanguageSpecification.Keywords.TryGetValue(identifierText, out var keywordType)
            ? keywordType
            : JyroTokenType.Identifier;

        return new JyroToken(tokenType, identifierText, tokenStartLine, tokenStartColumn);
    }

    /// <summary>
    /// Skips whitespace characters while updating position tracking.
    /// </summary>
    private static void SkipWhitespace(string sourceText, ref int currentPosition, ref int currentLine, ref int currentColumn)
    {
        while (!IsAtEndOfSource(sourceText, currentPosition))
        {
            switch (sourceText[currentPosition])
            {
                case ' ':
                case '\r':
                case '\t':
                    AdvancePosition(sourceText, ref currentPosition, ref currentLine, ref currentColumn);
                    continue;
                case '\n':
                    currentPosition++;
                    currentLine++;
                    currentColumn = 1;
                    continue;
            }
            break;
        }
    }

    /// <summary>
    /// Skips comment lines starting with '#' character.
    /// </summary>
    private void SkipComments(string sourceText, ref int currentPosition, ref int currentLine, ref int currentColumn)
    {
        if (!IsAtEndOfSource(sourceText, currentPosition) && sourceText[currentPosition] == '#')
        {
            // Consume characters until newline or end of file
            while (!IsAtEndOfSource(sourceText, currentPosition) && sourceText[currentPosition] != '\n')
            {
                AdvancePosition(sourceText, ref currentPosition, ref currentLine, ref currentColumn);
            }

            // Consume the newline character as part of the comment
            if (!IsAtEndOfSource(sourceText, currentPosition) && sourceText[currentPosition] == '\n')
            {
                AdvancePosition(sourceText, ref currentPosition, ref currentLine, ref currentColumn);
            }

            _logger.LogTrace("Comment skipped, now at line {Line}", currentLine);
        }
    }

    /// <summary>
    /// Checks if the current position is at or beyond the end of the source text.
    /// </summary>
    private static bool IsAtEndOfSource(string sourceText, int position) => position >= sourceText.Length;

    /// <summary>
    /// Peeks at the character at the specified position without advancing.
    /// </summary>
    private static char PeekNextCharacter(string sourceText, int position) => position < sourceText.Length ? sourceText[position] : '\0';

    /// <summary>
    /// Checks if the character is a decimal digit.
    /// </summary>
    private static bool IsDigitCharacter(char character) => character >= '0' && character <= '9';

    /// <summary>
    /// Checks if the character is alphabetic or underscore.
    /// </summary>
    private static bool IsAlphabeticCharacter(char character) => char.IsLetter(character) || character == '_';

    /// <summary>
    /// Checks if the character is alphanumeric or underscore.
    /// </summary>
    private static bool IsAlphanumericCharacter(char character) => IsAlphabeticCharacter(character) || IsDigitCharacter(character);

    /// <summary>
    /// Advances the current position and updates line/column tracking appropriately.
    /// </summary>
    private static void AdvancePosition(string sourceText, ref int currentPosition, ref int currentLine, ref int currentColumn)
    {
        if (IsAtEndOfSource(sourceText, currentPosition))
        {
            return;
        }

        if (sourceText[currentPosition] == '\n')
        {
            currentLine++;
            currentColumn = 1;
        }
        else
        {
            currentColumn++;
        }

        currentPosition++;
    }

    /// <summary>
    /// Adds an error-level diagnostic message to the collection.
    /// </summary>
    private static void AddDiagnosticError(List<IMessage> messageCollection, MessageCode errorCode, int lineNumber, int columnNumber, params string[] messageArguments) =>
        messageCollection.Add(new Message(errorCode, lineNumber, columnNumber, MessageSeverity.Error, ProcessingStage.Lexing, messageArguments));
}