using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Mesch.Jyro;

/// <summary>
/// Provides a recursive descent parser implementation for the Jyro language with structural guarantees
/// against infinite loops and comprehensive error recovery. This parser transforms token sequences
/// into abstract syntax trees while maintaining robust error handling and detailed diagnostic reporting.
/// </summary>
/// <remarks>
/// The parser implementation follows several key design principles to ensure reliability and performance:
/// 
/// <para><strong>Structural Progress Guarantee:</strong></para>
/// Every parsing method either succeeds and advances the token stream, or fails and leaves the stream
/// unchanged. The main parsing loop ensures forward progress by consuming at least one token per
/// iteration when errors occur, preventing infinite loops even with malformed input.
/// 
/// <para><strong>Transactional Semantics:</strong></para>
/// All parsing operations use checkpoint and rollback mechanisms to ensure that failed parsing
/// attempts do not leave the parser in an inconsistent state. This enables robust error recovery
/// and backtracking where necessary.
/// 
/// <para><strong>Comprehensive Error Recovery:</strong></para>
/// The parser continues processing after encountering errors, attempting to parse as much of the
/// input as possible to provide comprehensive diagnostic information and partial AST structures
/// for development tool integration.
/// 
/// <para><strong>Grammar Fidelity:</strong></para>
/// The parsing methods directly correspond to the formal Jyro grammar specification, ensuring
/// consistent syntax recognition and making the parser implementation auditable against the
/// language specification.
/// </remarks>
public sealed partial class Parser : IParser
{
    private readonly ILogger<Parser> _logger;
    private ParsingMetrics _currentMetrics;

    /// <summary>
    /// Initializes a new instance of the <see cref="Parser"/> class with the specified logger.
    /// </summary>
    /// <param name="logger">
    /// The logger instance used for diagnostic output and debugging information during parsing.
    /// Provides visibility into the parsing process for development and troubleshooting.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public Parser(ILogger<Parser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Parses the provided token sequence and returns a comprehensive result containing
    /// the abstract syntax tree, diagnostic messages, and processing metadata.
    /// </summary>
    /// <param name="tokens">
    /// The sequence of tokens produced by lexical analysis. Must include an end-of-file token
    /// to properly terminate parsing operations.
    /// </param>
    /// <returns>
    /// A <see cref="JyroParsingResult"/> containing the complete parsing outcome.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when tokens is null.</exception>
    public JyroParsingResult Parse(IReadOnlyList<JyroToken> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        var processingStartTime = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogTrace("Parsing started for {TokenCount} tokens", tokens.Count);

        var tokenStream = new TokenStream(tokens);
        var statementCollection = new List<IJyroStatement>();
        var diagnosticMessages = new List<IMessage>();
        _currentMetrics = new ParsingMetrics();

        try
        {
            // Main parsing loop with structural progress guarantee
            while (!tokenStream.IsAtEnd && tokenStream.Current.Type != JyroTokenType.EndOfFile)
            {
                var streamCheckpoint = tokenStream.CreateCheckpoint();
                var statementResult = ParseStatement(tokenStream);

                if (statementResult.IsSuccess)
                {
                    statementCollection.Add(statementResult.Value);
                    _currentMetrics.StatementCount++;
                    _logger.LogTrace("Successfully parsed {StatementType} at {Line}:{Column}",
                        statementResult.Value.GetType().Name, statementResult.Value.LineNumber, statementResult.Value.ColumnPosition);
                }
                else
                {
                    // Parsing failed - restore stream position and record error
                    tokenStream.RestoreCheckpoint(streamCheckpoint);
                    diagnosticMessages.Add(statementResult.Error.ToMessage());

                    _logger.LogTrace("Parsing error {ErrorCode} at {Line}:{Column}: {Description}",
                        statementResult.Error.Code, statementResult.Error.Token.LineNumber,
                        statementResult.Error.Token.ColumnPosition, statementResult.Error.Description);

                    // STRUCTURAL GUARANTEE: Always advance at least one token to prevent infinite loops
                    ConsumeInvalidToken(tokenStream, diagnosticMessages);
                }
            }
        }
        catch (Exception exception)
        {
            var currentToken = tokenStream.Current;
            _logger.LogError(exception, "Unexpected parser error at stream position {Position}", tokenStream.Position);
            diagnosticMessages.Add(new Message(
                MessageCode.UnknownParserError,
                currentToken.LineNumber, currentToken.ColumnPosition,
                MessageSeverity.Error,
                ProcessingStage.Parsing,
                exception.Message));
        }

        stopwatch.Stop();
        var isSuccessful = diagnosticMessages.TrueForAll(message => message.Severity != MessageSeverity.Error);

        var processingMetadata = new ParsingMetadata(
            stopwatch.Elapsed,
            _currentMetrics.StatementCount,
            _currentMetrics.MaxDepth,
            processingStartTime);

        _logger.LogTrace("Parsing completed: success={IsSuccessful}, statements={StatementCount}, maxDepth={MaxDepth}, errors={ErrorCount}, elapsed={ElapsedMilliseconds}ms",
            isSuccessful, _currentMetrics.StatementCount, _currentMetrics.MaxDepth,
            diagnosticMessages.Count(message => message.Severity == MessageSeverity.Error),
            stopwatch.ElapsedMilliseconds);

        return new JyroParsingResult(isSuccessful, statementCollection, diagnosticMessages, processingMetadata);
    }

    /// <summary>
    /// Attempts to parse a statement with transactional semantics.
    /// Either succeeds and advances the stream, or fails and leaves the stream unchanged.
    /// This method provides the core parsing logic for individual statements.
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing either the successfully parsed statement or error information.</returns>
    private ParseResult<IJyroStatement> TryParseStatement(TokenStream tokenStream)
    {
        if (tokenStream.IsAtEnd)
        {
            return ParseResult<IJyroStatement>.Failure(
                MessageCode.UnexpectedToken,
                tokenStream.Current,
                "Unexpected end of input while parsing statement");
        }

        _logger.LogTrace("Attempting to parse {TokenType} at {Line}:{Column}",
            tokenStream.Current.Type, tokenStream.Current.LineNumber, tokenStream.Current.ColumnPosition);

        var parseResult = tokenStream.Current.Type switch
        {
            JyroTokenType.Var => ParseVariableDeclaration(tokenStream),
            JyroTokenType.If => ParseIfStatement(tokenStream),
            JyroTokenType.Switch => ParseSwitchStatement(tokenStream),
            JyroTokenType.While => ParseWhileStatement(tokenStream),
            JyroTokenType.ForEach => ParseForEachStatement(tokenStream),
            JyroTokenType.Return => ParseReturnStatement(tokenStream),
            JyroTokenType.Break => ParseBreakStatement(tokenStream),
            JyroTokenType.Continue => ParseContinueStatement(tokenStream),
            _ => ParseExpressionOrAssignment(tokenStream)
        };

        _logger.LogTrace("Parse result: {IsSuccess} for {TokenType}",
            parseResult.IsSuccess, tokenStream.Current.Type);

        return parseResult;
    }

    /// <summary>
    /// Consumes one token when parsing fails to guarantee forward progress.
    /// This is the structural mechanism that prevents infinite loops by ensuring
    /// the parser always makes progress even when encountering invalid syntax.
    /// </summary>
    /// <param name="tokenStream">The token stream to advance.</param>
    /// <param name="diagnosticMessages">The collection to add error messages to.</param>
    private void ConsumeInvalidToken(TokenStream tokenStream, List<IMessage> diagnosticMessages)
    {
        var invalidToken = tokenStream.Advance(); // ALWAYS advance exactly one token

        _logger.LogTrace("Consumed invalid token {TokenType} at {Line}:{Column}",
            invalidToken.Type, invalidToken.LineNumber, invalidToken.ColumnPosition);

        // Add a specific error for the consumed token if it wasn't already reported
        diagnosticMessages.Add(new Message(
            MessageCode.UnexpectedToken,
            invalidToken.LineNumber,
            invalidToken.ColumnPosition,
            MessageSeverity.Error,
            ProcessingStage.Parsing,
            $"Unexpected token {invalidToken.Type} - skipping to continue parsing"));
    }

    /// <summary>
    /// Requires a specific token type with transactional semantics.
    /// If the expected token is found, it is consumed and returned.
    /// If not found, an error result is returned without advancing the stream.
    /// </summary>
    /// <param name="tokenStream">The token stream to check.</param>
    /// <param name="expectedType">The expected token type.</param>
    /// <param name="description">A human-readable description of what was expected.</param>
    /// <returns>A parse result containing either the expected token or error information.</returns>
    private ParseResult<JyroToken> Require(TokenStream tokenStream, JyroTokenType expectedType, string description)
    {
        if (tokenStream.Check(expectedType))
        {
            return ParseResult<JyroToken>.Success(tokenStream.Advance());
        }

        return ParseResult<JyroToken>.Failure(
            MessageCode.MissingToken,
            tokenStream.Current,
            $"Expected {description}, but found {tokenStream.Current.Type}");
    }

    /// <summary>
    /// Requires any of the specified token types with transactional semantics.
    /// If any of the expected tokens is found, it is consumed and returned.
    /// If none are found, an error result is returned without advancing the stream.
    /// </summary>
    /// <param name="tokenStream">The token stream to check.</param>
    /// <param name="description">A human-readable description of what was expected.</param>
    /// <param name="allowedTypes">The collection of acceptable token types.</param>
    /// <returns>A parse result containing either an acceptable token or error information.</returns>
    private ParseResult<JyroToken> RequireAny(TokenStream tokenStream, string description, params JyroTokenType[] allowedTypes)
    {
        if (tokenStream.Check(allowedTypes))
        {
            return ParseResult<JyroToken>.Success(tokenStream.Advance());
        }

        return ParseResult<JyroToken>.Failure(
            MessageCode.MissingToken,
            tokenStream.Current,
            $"Expected {description}, but found {tokenStream.Current.Type}");
    }

    /// <summary>
    /// Tracks the maximum nesting depth encountered during parsing for complexity analysis.
    /// This information is included in the parsing metadata for performance monitoring.
    /// </summary>
    /// <param name="depth">The current nesting depth being tracked.</param>
    private void TrackDepth(int depth)
    {
        _currentMetrics.MaxDepth = Math.Max(_currentMetrics.MaxDepth, depth);
    }

    #region Parsing Metrics

    /// <summary>
    /// Contains statistical information collected during the parsing process.
    /// This structure tracks complexity metrics and processing statistics.
    /// </summary>
    private struct ParsingMetrics
    {
        /// <summary>
        /// The total number of statements successfully parsed.
        /// </summary>
        public int StatementCount;

        /// <summary>
        /// The maximum nesting depth encountered during parsing.
        /// </summary>
        public int MaxDepth;
    }

    #endregion
}