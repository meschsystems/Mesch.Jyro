namespace Mesch.Jyro;

/// <summary>
/// Provides a stream-like interface over a sequence of tokens with lookahead capabilities and position tracking.
/// This utility class enables parsers to navigate through token sequences efficiently while maintaining
/// precise position information for error reporting and backtracking operations.
/// </summary>
/// <remarks>
/// The TokenStream class abstracts token sequence navigation and provides essential parsing utilities:
/// <list type="bullet">
/// <item><description>Sequential token consumption with automatic bounds checking</description></item>
/// <item><description>Lookahead capabilities for predictive parsing decisions</description></item>
/// <item><description>Position tracking and restoration for backtracking algorithms</description></item>
/// <item><description>Context generation for meaningful error reporting</description></item>
/// <item><description>Token type matching utilities for parser decision making</description></item>
/// </list>
/// This design enables robust recursive descent parsing while maintaining clear error reporting
/// and efficient token sequence processing.
/// </remarks>
public sealed class TokenStream
{
    private readonly IReadOnlyList<JyroToken> _tokenSequence;
    private int _currentPosition;

    /// <summary>
    /// Initializes a new token stream over the specified sequence of tokens.
    /// </summary>
    /// <param name="tokens">
    /// The sequence of tokens to stream over. This should include an end-of-file token
    /// as the final element to properly terminate parsing operations.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when tokens is null.</exception>
    public TokenStream(IReadOnlyList<JyroToken> tokens)
    {
        _tokenSequence = tokens ?? throw new ArgumentNullException(nameof(tokens));
        _currentPosition = 0;
    }

    /// <summary>
    /// Gets the current token without advancing the stream position.
    /// When at the end of the stream, returns the final token (typically end-of-file).
    /// </summary>
    public JyroToken Current => IsAtEnd ? _tokenSequence[^1] : _tokenSequence[_currentPosition];

    /// <summary>
    /// Gets the previously consumed token (the last token that was advanced past).
    /// This is useful for error reporting and context tracking in parser implementations.
    /// </summary>
    public JyroToken Previous => _tokenSequence[Math.Max(0, _currentPosition - 1)];

    /// <summary>
    /// Gets the current zero-based position in the token sequence.
    /// This position can be used for checkpointing and restoration operations.
    /// </summary>
    public int Position => _currentPosition;

    /// <summary>
    /// Gets a value indicating whether the stream has reached the end of the token sequence.
    /// When true, no more tokens are available for consumption.
    /// </summary>
    public bool IsAtEnd => _currentPosition >= _tokenSequence.Count;

    /// <summary>
    /// Advances the stream position by one and returns the token that was current before advancing.
    /// This is the primary method for consuming tokens during parsing operations.
    /// </summary>
    /// <returns>
    /// The token that was at the current position before advancing.
    /// If already at the end, returns the final token without advancing further.
    /// </returns>
    public JyroToken Advance()
    {
        if (_currentPosition < _tokenSequence.Count)
        {
            _currentPosition++;
        }
        return _tokenSequence[_currentPosition - 1];
    }

    /// <summary>
    /// Checks if the current token matches any of the specified types without advancing the stream.
    /// This method is essential for predictive parsing decisions.
    /// </summary>
    /// <param name="tokenTypes">The token types to check against the current token.</param>
    /// <returns>
    /// True if the current token's type matches any of the specified types, otherwise false.
    /// Returns false if at the end of the stream.
    /// </returns>
    public bool Check(params JyroTokenType[] tokenTypes)
    {
        return IsAtEnd ? false : tokenTypes.Contains(Current.Type);
    }

    /// <summary>
    /// Checks if the current token matches any of the specified types and advances if a match is found.
    /// This combines checking and consumption into a single atomic operation for parser convenience.
    /// </summary>
    /// <param name="tokenTypes">The token types to match against the current token.</param>
    /// <returns>
    /// True if a match was found and the stream was advanced, otherwise false.
    /// If no match is found, the stream position remains unchanged.
    /// </returns>
    public bool Match(params JyroTokenType[] tokenTypes)
    {
        if (Check(tokenTypes))
        {
            Advance();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Looks ahead at the token at the specified offset from the current position without advancing.
    /// This enables parsers to examine upcoming tokens for decision making.
    /// </summary>
    /// <param name="offset">
    /// The number of positions to look ahead from the current position.
    /// 0 returns the current token, 1 returns the next token, etc.
    /// </param>
    /// <returns>
    /// The token at the specified offset, or the final token if the offset extends beyond the sequence.
    /// </returns>
    public JyroToken Peek(int offset = 0)
    {
        var targetPosition = _currentPosition + offset;
        if (targetPosition >= _tokenSequence.Count)
        {
            return _tokenSequence[^1]; // Return the final token (typically EOF)
        }
        return _tokenSequence[targetPosition];
    }

    /// <summary>
    /// Resets the stream position to the beginning of the token sequence.
    /// This enables full re-parsing of the token sequence if needed.
    /// </summary>
    public void Reset()
    {
        _currentPosition = 0;
    }

    /// <summary>
    /// Sets the stream position to the specified index, clamped to the valid range.
    /// This enables precise position control for backtracking and error recovery.
    /// </summary>
    /// <param name="position">
    /// The target position to set. Values outside the valid range are clamped
    /// to ensure the stream remains in a consistent state.
    /// </param>
    public void SetPosition(int position)
    {
        _currentPosition = Math.Clamp(position, 0, _tokenSequence.Count);
    }

    /// <summary>
    /// Generates a context string showing tokens around the current position for debugging and error reporting.
    /// This provides valuable context for understanding parser state during development and error diagnosis.
    /// </summary>
    /// <param name="contextSize">
    /// The number of tokens to show before and after the current position.
    /// Larger values provide more context but may become unwieldy for display.
    /// </param>
    /// <returns>
    /// A formatted string showing tokens around the current position with a marker
    /// indicating the current token for easy visual identification.
    /// </returns>
    public string GetContext(int contextSize = 3)
    {
        var startPosition = Math.Max(0, _currentPosition - contextSize);
        var endPosition = Math.Min(_tokenSequence.Count, _currentPosition + contextSize + 1);

        var contextLines = new List<string>();
        for (int index = startPosition; index < endPosition; index++)
        {
            var token = _tokenSequence[index];
            var positionMarker = index == _currentPosition ? ">>>" : "   ";
            contextLines.Add($"{positionMarker} {token.Type}({token.Lexeme})");
        }

        return string.Join("\n", contextLines);
    }

    /// <summary>
    /// Creates a checkpoint representing the current stream position that can be restored later.
    /// This enables backtracking patterns in parser implementations.
    /// </summary>
    /// <returns>A checkpoint containing the current position information.</returns>
    public StreamCheckpoint CreateCheckpoint() => new(_currentPosition);

    /// <summary>
    /// Restores the stream position to a previously created checkpoint.
    /// This enables parser backtracking and speculative parsing operations.
    /// </summary>
    /// <param name="checkpoint">The checkpoint to restore to.</param>
    public void RestoreCheckpoint(StreamCheckpoint checkpoint)
    {
        SetPosition(checkpoint.Position);
    }

    /// <summary>
    /// Represents a saved position in the token stream that can be restored later.
    /// This structure enables efficient backtracking operations in parser implementations.
    /// </summary>
    public readonly struct StreamCheckpoint
    {
        /// <summary>
        /// Initializes a new checkpoint with the specified position.
        /// </summary>
        /// <param name="position">The stream position to save in this checkpoint.</param>
        internal StreamCheckpoint(int position)
        {
            Position = position;
        }

        /// <summary>
        /// Gets the saved stream position represented by this checkpoint.
        /// This position can be used to restore the stream to its state when the checkpoint was created.
        /// </summary>
        public int Position { get; }
    }
}