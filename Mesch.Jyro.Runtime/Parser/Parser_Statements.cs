using Microsoft.Extensions.Logging;

namespace Mesch.Jyro;

public sealed partial class Parser
{
    /// <summary>
    /// Parses a statement according to the formal Jyro grammar specification.
    /// This method serves as the primary dispatch point for all statement types
    /// and ensures proper categorization based on leading tokens.
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing either the successfully parsed statement or error information.</returns>
    private ParseResult<IJyroStatement> ParseStatement(TokenStream tokenStream)
    {
        if (tokenStream.IsAtEnd)
        {
            return ParseResult<IJyroStatement>.Failure(
                MessageCode.UnexpectedToken,
                tokenStream.Current,
                "Unexpected end of input while expecting statement");
        }

        return tokenStream.Current.Type switch
        {
            JyroTokenType.Var => ParseVariableDeclaration(tokenStream),
            JyroTokenType.If => ParseIfStatement(tokenStream),
            JyroTokenType.Switch => ParseSwitchStatement(tokenStream),
            JyroTokenType.While => ParseWhileStatement(tokenStream),
            JyroTokenType.ForEach => ParseForEachStatement(tokenStream),
            JyroTokenType.Return => ParseReturnStatement(tokenStream),
            JyroTokenType.Break => ParseBreakStatement(tokenStream),
            JyroTokenType.Continue => ParseContinueStatement(tokenStream),
            JyroTokenType.EndOfFile => ParseResult<IJyroStatement>.Failure(
                MessageCode.UnexpectedToken,
                tokenStream.Current,
                "Unexpected end of file while parsing statement"),
            _ => ParseExpressionOrAssignment(tokenStream)
        };
    }

    /// <summary>
    /// Parses variable declaration statements following the grammar:
    /// "var" Identifier [ ":" Type ] [ "=" Expression ]
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the variable declaration statement or error information.</returns>
    private ParseResult<IJyroStatement> ParseVariableDeclaration(TokenStream tokenStream)
    {
        var varToken = tokenStream.Advance(); // consume "var"

        var nameResult = Require(tokenStream, JyroTokenType.Identifier, "variable name");
        if (nameResult.IsFailure)
        {
            return ParseResult<IJyroStatement>.Failure(nameResult.Error);
        }

        var variableName = nameResult.Value.Lexeme;
        IExpression? typeHint = null;
        IExpression? initializer = null;

        // Optional type annotation: ":" Type
        if (tokenStream.Match(JyroTokenType.Colon))
        {
            var typeResult = ParseTypeExpression(tokenStream);
            if (typeResult.IsFailure)
            {
                return ParseResult<IJyroStatement>.Failure(typeResult.Error);
            }
            typeHint = typeResult.Value;
        }

        // Optional initializer: "=" Expression
        if (tokenStream.Match(JyroTokenType.Equal))
        {
            var initializerResult = ParseExpression(tokenStream);
            if (initializerResult.IsFailure)
            {
                return ParseResult<IJyroStatement>.Failure(initializerResult.Error);
            }
            initializer = initializerResult.Value;
        }

        return ParseResult<IJyroStatement>.Success(
            new VariableDeclarationStatement(variableName, typeHint, initializer, varToken.LineNumber, varToken.ColumnPosition));
    }

    /// <summary>
    /// Parses type expressions for variable declarations and type checking operations.
    /// Handles the fundamental Jyro type keywords: number, string, boolean, object, array.
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the type expression or error information.</returns>
    private ParseResult<IExpression> ParseTypeExpression(TokenStream tokenStream)
    {
        var typeTokenResult = RequireAny(tokenStream, "type keyword",
            JyroTokenType.NumberType, JyroTokenType.StringType, JyroTokenType.BooleanType,
            JyroTokenType.ObjectType, JyroTokenType.ArrayType);

        if (typeTokenResult.IsFailure)
        {
            return ParseResult<IExpression>.Failure(typeTokenResult.Error);
        }

        var typeToken = typeTokenResult.Value;
        return ParseResult<IExpression>.Success(
            new TypeExpression(typeToken.Type, typeToken.LineNumber, typeToken.ColumnPosition));
    }

    /// <summary>
    /// Parses if statements with support for else-if chains following the grammar:
    /// "if" Expression "then" { Statement } { "else" "if" Expression "then" { Statement } } [ "else" { Statement } ] "end"
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the if statement or error information.</returns>
    private ParseResult<IJyroStatement> ParseIfStatement(TokenStream tokenStream)
    {
        var ifToken = tokenStream.Advance(); // consume "if"

        var conditionResult = ParseExpression(tokenStream);
        if (conditionResult.IsFailure)
        {
            return ParseResult<IJyroStatement>.Failure(conditionResult.Error);
        }

        var thenTokenResult = Require(tokenStream, JyroTokenType.Then, "then");
        if (thenTokenResult.IsFailure)
        {
            return ParseResult<IJyroStatement>.Failure(thenTokenResult.Error);
        }

        TrackDepth(1); // Track nesting for then branch
        var thenStatementsResult = ParseStatementBlock(tokenStream, JyroTokenType.Else, JyroTokenType.End);
        if (thenStatementsResult.IsFailure)
        {
            return ParseResult<IJyroStatement>.Failure(thenStatementsResult.Error);
        }

        // Handle chained else-if constructs: { "else" "if" Expression "then" { Statement } }
        var elseIfClauses = new List<ElseIfClause>();
        while (tokenStream.Check(JyroTokenType.Else))
        {
            var elseToken = tokenStream.Advance(); // consume "else"

            // Check if this is "else if" or just "else"
            if (tokenStream.Check(JyroTokenType.If))
            {
                tokenStream.Advance(); // consume "if"

                var elseIfConditionResult = ParseExpression(tokenStream);
                if (elseIfConditionResult.IsFailure)
                {
                    return ParseResult<IJyroStatement>.Failure(elseIfConditionResult.Error);
                }

                var elseIfThenResult = Require(tokenStream, JyroTokenType.Then, "then");
                if (elseIfThenResult.IsFailure)
                {
                    return ParseResult<IJyroStatement>.Failure(elseIfThenResult.Error);
                }

                TrackDepth(1); // Track nesting for else-if branch
                var elseIfStatementsResult = ParseStatementBlock(tokenStream, JyroTokenType.Else, JyroTokenType.End);
                if (elseIfStatementsResult.IsFailure)
                {
                    return ParseResult<IJyroStatement>.Failure(elseIfStatementsResult.Error);
                }

                elseIfClauses.Add(new ElseIfClause(
                    elseIfConditionResult.Value,
                    elseIfStatementsResult.Value,
                    elseToken.LineNumber,
                    elseToken.ColumnPosition));
            }
            else
            {
                // This is a final "else" clause
                TrackDepth(1); // Track nesting for else branch
                var elseStatementsResult = ParseStatementBlock(tokenStream, JyroTokenType.End);
                if (elseStatementsResult.IsFailure)
                {
                    return ParseResult<IJyroStatement>.Failure(elseStatementsResult.Error);
                }

                var endTokenResult = Require(tokenStream, JyroTokenType.End, "end");
                if (endTokenResult.IsFailure)
                {
                    return ParseResult<IJyroStatement>.Failure(endTokenResult.Error);
                }

                return ParseResult<IJyroStatement>.Success(
                    new IfStatement(conditionResult.Value, thenStatementsResult.Value, elseIfClauses, elseStatementsResult.Value,
                                   ifToken.LineNumber, ifToken.ColumnPosition));
            }
        }

        // No else clause, just end
        var finalEndResult = Require(tokenStream, JyroTokenType.End, "end");
        if (finalEndResult.IsFailure)
        {
            return ParseResult<IJyroStatement>.Failure(finalEndResult.Error);
        }

        return ParseResult<IJyroStatement>.Success(
            new IfStatement(conditionResult.Value, thenStatementsResult.Value, elseIfClauses, new List<IJyroStatement>(),
                           ifToken.LineNumber, ifToken.ColumnPosition));
    }

    /// <summary>
    /// Parses while loop statements following the grammar:
    /// "while" Expression "do" { Statement } "end"
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the while statement or error information.</returns>
    private ParseResult<IJyroStatement> ParseWhileStatement(TokenStream tokenStream)
    {
        var whileToken = tokenStream.Advance(); // consume "while"

        var conditionResult = ParseExpression(tokenStream);
        if (conditionResult.IsFailure)
        {
            return ParseResult<IJyroStatement>.Failure(conditionResult.Error);
        }

        var doTokenResult = Require(tokenStream, JyroTokenType.Do, "do");
        if (doTokenResult.IsFailure)
        {
            return ParseResult<IJyroStatement>.Failure(doTokenResult.Error);
        }

        TrackDepth(1); // Track nesting for loop body
        var bodyResult = ParseStatementBlock(tokenStream, JyroTokenType.End);
        if (bodyResult.IsFailure)
        {
            return ParseResult<IJyroStatement>.Failure(bodyResult.Error);
        }

        var endTokenResult = Require(tokenStream, JyroTokenType.End, "end");
        if (endTokenResult.IsFailure)
        {
            return ParseResult<IJyroStatement>.Failure(endTokenResult.Error);
        }

        return ParseResult<IJyroStatement>.Success(
            new WhileStatement(conditionResult.Value, bodyResult.Value, whileToken.LineNumber, whileToken.ColumnPosition));
    }

    /// <summary>
    /// Parses foreach loop statements following the grammar:
    /// "foreach" Identifier "in" Expression "do" { Statement } "end"
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the foreach statement or error information.</returns>
    private ParseResult<IJyroStatement> ParseForEachStatement(TokenStream tokenStream)
    {
        var foreachToken = tokenStream.Advance(); // consume "foreach"

        var iteratorNameResult = Require(tokenStream, JyroTokenType.Identifier, "iterator variable name");
        if (iteratorNameResult.IsFailure)
        {
            return ParseResult<IJyroStatement>.Failure(iteratorNameResult.Error);
        }

        var inTokenResult = Require(tokenStream, JyroTokenType.In, "in");
        if (inTokenResult.IsFailure)
        {
            return ParseResult<IJyroStatement>.Failure(inTokenResult.Error);
        }

        var sourceResult = ParseExpression(tokenStream);
        if (sourceResult.IsFailure)
        {
            return ParseResult<IJyroStatement>.Failure(sourceResult.Error);
        }

        var doTokenResult = Require(tokenStream, JyroTokenType.Do, "do");
        if (doTokenResult.IsFailure)
        {
            return ParseResult<IJyroStatement>.Failure(doTokenResult.Error);
        }

        TrackDepth(1); // Track nesting for loop body
        var bodyResult = ParseStatementBlock(tokenStream, JyroTokenType.End);
        if (bodyResult.IsFailure)
        {
            return ParseResult<IJyroStatement>.Failure(bodyResult.Error);
        }

        var endTokenResult = Require(tokenStream, JyroTokenType.End, "end");
        if (endTokenResult.IsFailure)
        {
            return ParseResult<IJyroStatement>.Failure(endTokenResult.Error);
        }

        return ParseResult<IJyroStatement>.Success(
            new ForEachStatement(iteratorNameResult.Value.Lexeme, sourceResult.Value, bodyResult.Value,
                                foreachToken.LineNumber, foreachToken.ColumnPosition));
    }

    /// <summary>
    /// Parses switch statements with traditional equality-based semantics following the grammar:
    /// "switch" Expression { "case" Expression "then" { Statement } } [ "default" { Statement } ] "end"
    /// Each case expression is compared for equality with the switch expression.
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the switch statement or error information.</returns>
    private ParseResult<IJyroStatement> ParseSwitchStatement(TokenStream tokenStream)
    {
        var switchToken = tokenStream.Advance(); // consume "switch"

        var switchExpressionResult = ParseExpression(tokenStream);
        if (switchExpressionResult.IsFailure)
        {
            return ParseResult<IJyroStatement>.Failure(switchExpressionResult.Error);
        }

        var caseCollection = new List<CaseClause>();
        var defaultStatements = new List<IJyroStatement>();

        TrackDepth(1); // Track nesting for switch body

        while (!tokenStream.IsAtEnd && !tokenStream.Check(JyroTokenType.End))
        {
            if (tokenStream.Match(JyroTokenType.Case))
            {
                var caseToken = tokenStream.Previous;

                // Parse case value expression (will be compared for equality with switch expression)
                var caseValueResult = ParseExpression(tokenStream);
                if (caseValueResult.IsFailure)
                {
                    return ParseResult<IJyroStatement>.Failure(caseValueResult.Error);
                }

                var thenTokenResult = Require(tokenStream, JyroTokenType.Then, "then");
                if (thenTokenResult.IsFailure)
                {
                    return ParseResult<IJyroStatement>.Failure(thenTokenResult.Error);
                }

                var caseBodyResult = ParseStatementBlock(tokenStream, JyroTokenType.Case, JyroTokenType.Default, JyroTokenType.End);
                if (caseBodyResult.IsFailure)
                {
                    return ParseResult<IJyroStatement>.Failure(caseBodyResult.Error);
                }

                // Create case clause with the value expression
                // The executor will generate: switchExpression == caseValue
                caseCollection.Add(new CaseClause(caseValueResult.Value, caseBodyResult.Value, caseToken.LineNumber, caseToken.ColumnPosition));
            }
            else if (tokenStream.Match(JyroTokenType.Default))
            {
                var defaultBodyResult = ParseStatementBlock(tokenStream, JyroTokenType.End);
                if (defaultBodyResult.IsFailure)
                {
                    return ParseResult<IJyroStatement>.Failure(defaultBodyResult.Error);
                }
                defaultStatements.AddRange(defaultBodyResult.Value);
                break; // Default must be last
            }
            else
            {
                return ParseResult<IJyroStatement>.Failure(
                    MessageCode.UnexpectedToken,
                    tokenStream.Current,
                    $"Expected case, default, or end in switch statement, but found {tokenStream.Current.Type}");
            }
        }

        var endTokenResult = Require(tokenStream, JyroTokenType.End, "end");
        if (endTokenResult.IsFailure)
        {
            return ParseResult<IJyroStatement>.Failure(endTokenResult.Error);
        }

        return ParseResult<IJyroStatement>.Success(
            new SwitchStatement(switchExpressionResult.Value, caseCollection, defaultStatements,
                               switchToken.LineNumber, switchToken.ColumnPosition));
    }

    /// <summary>
    /// Parses return statements following the grammar:
    /// "return" [ Expression ]
    /// Currently simplified to not include optional return values.
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the return statement or error information.</returns>
    private ParseResult<IJyroStatement> ParseReturnStatement(TokenStream tokenStream)
    {
        var returnToken = tokenStream.Advance(); // consume "return"

        return ParseResult<IJyroStatement>.Success(
            new ReturnStatement(null, returnToken.LineNumber, returnToken.ColumnPosition));
    }

    /// <summary>
    /// Parses break statements following the grammar: "break"
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the break statement or error information.</returns>
    private ParseResult<IJyroStatement> ParseBreakStatement(TokenStream tokenStream)
    {
        var breakToken = tokenStream.Advance(); // consume "break"
        return ParseResult<IJyroStatement>.Success(
            new BreakStatement(breakToken.LineNumber, breakToken.ColumnPosition));
    }

    /// <summary>
    /// Parses continue statements following the grammar: "continue"
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the continue statement or error information.</returns>
    private ParseResult<IJyroStatement> ParseContinueStatement(TokenStream tokenStream)
    {
        var continueToken = tokenStream.Advance(); // consume "continue"
        return ParseResult<IJyroStatement>.Success(
            new ContinueStatement(continueToken.LineNumber, continueToken.ColumnPosition));
    }

    /// <summary>
    /// Parses either an expression statement or an assignment statement by examining
    /// the expression pattern and checking for assignment operators.
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the statement or error information.</returns>
    private ParseResult<IJyroStatement> ParseExpressionOrAssignment(TokenStream tokenStream)
    {
        var expressionResult = ParseExpression(tokenStream);
        if (expressionResult.IsFailure)
        {
            return ParseResult<IJyroStatement>.Failure(expressionResult.Error);
        }

        var parsedExpression = expressionResult.Value;

        // Check for assignment operator
        if (tokenStream.Match(JyroTokenType.Equal))
        {
            // Validate that the left-hand side is a valid assignment target
            if (parsedExpression is not (VariableExpression or PropertyAccessExpression or IndexAccessExpression))
            {
                return ParseResult<IJyroStatement>.Failure(
                    MessageCode.InvalidAssignmentTarget,
                    tokenStream.Previous,
                    $"Cannot assign to {parsedExpression.GetType().Name} - only variables, properties, and indexed elements can be assigned");
            }

            var assignmentValueResult = ParseExpression(tokenStream);
            if (assignmentValueResult.IsFailure)
            {
                return ParseResult<IJyroStatement>.Failure(assignmentValueResult.Error);
            }

            return ParseResult<IJyroStatement>.Success(
                new AssignmentStatement(parsedExpression, assignmentValueResult.Value, parsedExpression.LineNumber, parsedExpression.ColumnPosition));
        }

        // Expression statement - evaluate expression for side effects
        return ParseResult<IJyroStatement>.Success(
            new ExpressionStatement(parsedExpression, parsedExpression.LineNumber, parsedExpression.ColumnPosition));
    }

    /// <summary>
    /// Parses a block of statements until one of the specified terminator tokens is encountered.
    /// Uses structural guarantees to prevent infinite loops and provides robust error recovery.
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <param name="terminatorTokens">The token types that indicate the end of the statement block.</param>
    /// <returns>A parse result containing the collection of parsed statements or error information.</returns>
    private ParseResult<IReadOnlyList<IJyroStatement>> ParseStatementBlock(TokenStream tokenStream, params JyroTokenType[] terminatorTokens)
    {
        var statementCollection = new List<IJyroStatement>();
        const int MaxBlockSize = 10000; // Reasonable upper bound to prevent excessive memory usage

        while (!tokenStream.IsAtEnd && !tokenStream.Check(terminatorTokens) && statementCollection.Count < MaxBlockSize)
        {
            var streamCheckpoint = tokenStream.CreateCheckpoint();
            var statementResult = TryParseStatement(tokenStream);

            if (statementResult.IsSuccess)
            {
                statementCollection.Add(statementResult.Value);
            }
            else
            {
                // Failed to parse statement - check if we're at a natural terminator
                if (tokenStream.Check(terminatorTokens))
                {
                    break; // Natural termination at block boundary
                }

                // Restore position and consume invalid token to maintain progress
                tokenStream.RestoreCheckpoint(streamCheckpoint);
                var invalidToken = tokenStream.Advance();

                _logger.LogTrace("Statement block parsing: consumed invalid token {TokenType} at {Line}:{Column}",
                    invalidToken.Type, invalidToken.LineNumber, invalidToken.ColumnPosition);

                // Check if consuming the token brought us to a terminator
                if (tokenStream.Check(terminatorTokens))
                {
                    break; // Reached terminator after error recovery
                }

                // Continue parsing to collect as many valid statements as possible
            }
        }

        if (statementCollection.Count >= MaxBlockSize)
        {
            return ParseResult<IReadOnlyList<IJyroStatement>>.Failure(
                MessageCode.UnknownParserError,
                tokenStream.Current,
                $"Statement block exceeded maximum size limit of {MaxBlockSize} statements");
        }

        return ParseResult<IReadOnlyList<IJyroStatement>>.Success(statementCollection);
    }
}