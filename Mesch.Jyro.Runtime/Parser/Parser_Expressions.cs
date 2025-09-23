using System.Globalization;

namespace Mesch.Jyro;

public sealed partial class Parser
{
    /// <summary>
    /// Entry point for expression parsing following the formal grammar precedence hierarchy.
    /// Expression = Ternary
    /// This method establishes the top level of the precedence chain.
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the parsed expression or error information.</returns>
    private ParseResult<IExpression> ParseExpression(TokenStream tokenStream) => ParseTernary(tokenStream);

    /// <summary>
    /// Parses ternary expressions with right-associative chaining following the grammar:
    /// Ternary = LogicalOr [ "?" Ternary ":" Ternary ]
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the ternary expression or error information.</returns>
    private ParseResult<IExpression> ParseTernary(TokenStream tokenStream)
    {
        var conditionResult = ParseLogicalOr(tokenStream);
        if (conditionResult.IsFailure)
        {
            return conditionResult;
        }

        if (tokenStream.Match(JyroTokenType.QuestionMark))
        {
            var questionToken = tokenStream.Previous;

            var trueExpressionResult = ParseTernary(tokenStream); // Right-associative
            if (trueExpressionResult.IsFailure)
            {
                return trueExpressionResult;
            }

            var colonResult = Require(tokenStream, JyroTokenType.Colon, ":");
            if (colonResult.IsFailure)
            {
                return ParseResult<IExpression>.Failure(colonResult.Error);
            }

            var falseExpressionResult = ParseTernary(tokenStream); // Right-associative
            if (falseExpressionResult.IsFailure)
            {
                return falseExpressionResult;
            }

            return ParseResult<IExpression>.Success(
                new TernaryExpression(conditionResult.Value, trueExpressionResult.Value, falseExpressionResult.Value,
                                     questionToken.LineNumber, questionToken.ColumnPosition));
        }

        return ParseResult<IExpression>.Success(conditionResult.Value);
    }

    /// <summary>
    /// Parses logical OR expressions with left-associative chaining following the grammar:
    /// LogicalOr = LogicalAnd { "or" LogicalAnd }
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the logical OR expression or error information.</returns>
    private ParseResult<IExpression> ParseLogicalOr(TokenStream tokenStream)
    {
        var leftOperandResult = ParseLogicalAnd(tokenStream);
        if (leftOperandResult.IsFailure)
        {
            return leftOperandResult;
        }

        var currentExpression = leftOperandResult.Value;

        while (tokenStream.Match(JyroTokenType.Or))
        {
            var operatorToken = tokenStream.Previous;
            var rightOperandResult = ParseLogicalAnd(tokenStream);
            if (rightOperandResult.IsFailure)
            {
                return rightOperandResult;
            }

            currentExpression = new BinaryExpression(currentExpression, operatorToken.Type, rightOperandResult.Value,
                                       currentExpression.LineNumber, currentExpression.ColumnPosition);
        }

        return ParseResult<IExpression>.Success(currentExpression);
    }

    /// <summary>
    /// Parses logical AND expressions with left-associative chaining following the grammar:
    /// LogicalAnd = Equality { "and" Equality }
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the logical AND expression or error information.</returns>
    private ParseResult<IExpression> ParseLogicalAnd(TokenStream tokenStream)
    {
        var leftOperandResult = ParseEquality(tokenStream);
        if (leftOperandResult.IsFailure)
        {
            return leftOperandResult;
        }

        var currentExpression = leftOperandResult.Value;

        while (tokenStream.Match(JyroTokenType.And))
        {
            var operatorToken = tokenStream.Previous;
            var rightOperandResult = ParseEquality(tokenStream);
            if (rightOperandResult.IsFailure)
            {
                return rightOperandResult;
            }

            currentExpression = new BinaryExpression(currentExpression, operatorToken.Type, rightOperandResult.Value,
                                       currentExpression.LineNumber, currentExpression.ColumnPosition);
        }

        return ParseResult<IExpression>.Success(currentExpression);
    }

    /// <summary>
    /// Parses equality comparison expressions with left-associative chaining following the grammar:
    /// Equality = Relational { ("==" | "!=") Relational }
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the equality expression or error information.</returns>
    private ParseResult<IExpression> ParseEquality(TokenStream tokenStream)
    {
        var leftOperandResult = ParseRelational(tokenStream);
        if (leftOperandResult.IsFailure)
        {
            return leftOperandResult;
        }

        var currentExpression = leftOperandResult.Value;

        while (tokenStream.Match(JyroTokenType.EqualEqual, JyroTokenType.BangEqual))
        {
            var operatorToken = tokenStream.Previous;
            var rightOperandResult = ParseRelational(tokenStream);
            if (rightOperandResult.IsFailure)
            {
                return rightOperandResult;
            }

            currentExpression = new BinaryExpression(currentExpression, operatorToken.Type, rightOperandResult.Value,
                                       currentExpression.LineNumber, currentExpression.ColumnPosition);
        }

        return ParseResult<IExpression>.Success(currentExpression);
    }

    /// <summary>
    /// Parses relational and type checking expressions with left-associative chaining following the grammar:
    /// Relational = Additive { ("&lt;" | "&lt;=" | "&gt;" | "&gt;=" | "is") Additive }
    /// Special handling for "is" operator which expects type keywords.
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the relational expression or error information.</returns>
    private ParseResult<IExpression> ParseRelational(TokenStream tokenStream)
    {
        var leftOperandResult = ParseAdditive(tokenStream);
        if (leftOperandResult.IsFailure)
        {
            return leftOperandResult;
        }

        var currentExpression = leftOperandResult.Value;

        while (tokenStream.Match(JyroTokenType.Less, JyroTokenType.LessEqual,
                           JyroTokenType.Greater, JyroTokenType.GreaterEqual, JyroTokenType.Is))
        {
            var operatorToken = tokenStream.Previous;

            if (operatorToken.Type == JyroTokenType.Is)
            {
                // Special handling for type checking: "is" expects a type keyword
                var typeTokenResult = RequireAny(tokenStream, "type keyword",
                    JyroTokenType.NumberType, JyroTokenType.StringType, JyroTokenType.BooleanType,
                    JyroTokenType.ObjectType, JyroTokenType.ArrayType);

                if (typeTokenResult.IsFailure)
                {
                    return ParseResult<IExpression>.Failure(typeTokenResult.Error);
                }

                currentExpression = new TypeCheckExpression(currentExpression, typeTokenResult.Value.Type,
                                             operatorToken.LineNumber, operatorToken.ColumnPosition);
            }
            else
            {
                var rightOperandResult = ParseAdditive(tokenStream);
                if (rightOperandResult.IsFailure)
                {
                    return rightOperandResult;
                }

                currentExpression = new BinaryExpression(currentExpression, operatorToken.Type, rightOperandResult.Value,
                                           currentExpression.LineNumber, currentExpression.ColumnPosition);
            }
        }

        return ParseResult<IExpression>.Success(currentExpression);
    }

    /// <summary>
    /// Parses additive expressions with left-associative chaining following the grammar:
    /// Additive = Multiplicative { ("+" | "-") Multiplicative }
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the additive expression or error information.</returns>
    private ParseResult<IExpression> ParseAdditive(TokenStream tokenStream)
    {
        var leftOperandResult = ParseMultiplicative(tokenStream);
        if (leftOperandResult.IsFailure)
        {
            return leftOperandResult;
        }

        var currentExpression = leftOperandResult.Value;

        while (tokenStream.Match(JyroTokenType.Plus, JyroTokenType.Minus))
        {
            var operatorToken = tokenStream.Previous;
            var rightOperandResult = ParseMultiplicative(tokenStream);
            if (rightOperandResult.IsFailure)
            {
                return rightOperandResult;
            }

            currentExpression = new BinaryExpression(currentExpression, operatorToken.Type, rightOperandResult.Value,
                                       currentExpression.LineNumber, currentExpression.ColumnPosition);
        }

        return ParseResult<IExpression>.Success(currentExpression);
    }

    /// <summary>
    /// Parses multiplicative expressions with left-associative chaining following the grammar:
    /// Multiplicative = Unary { ("*" | "/" | "%") Unary }
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the multiplicative expression or error information.</returns>
    private ParseResult<IExpression> ParseMultiplicative(TokenStream tokenStream)
    {
        var leftOperandResult = ParseUnary(tokenStream);
        if (leftOperandResult.IsFailure)
        {
            return leftOperandResult;
        }

        var currentExpression = leftOperandResult.Value;

        while (tokenStream.Match(JyroTokenType.Star, JyroTokenType.Slash, JyroTokenType.Percent))
        {
            var operatorToken = tokenStream.Previous;
            var rightOperandResult = ParseUnary(tokenStream);
            if (rightOperandResult.IsFailure)
            {
                return rightOperandResult;
            }

            currentExpression = new BinaryExpression(currentExpression, operatorToken.Type, rightOperandResult.Value,
                                       currentExpression.LineNumber, currentExpression.ColumnPosition);
        }

        return ParseResult<IExpression>.Success(currentExpression);
    }

    /// <summary>
    /// Parses unary expressions with right-associative precedence following the grammar:
    /// Unary = [ "not" | "-" ] Primary
    /// Unary operators have higher precedence than binary operators.
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the unary expression or error information.</returns>
    private ParseResult<IExpression> ParseUnary(TokenStream tokenStream)
    {
        if (tokenStream.Match(JyroTokenType.Not, JyroTokenType.Minus))
        {
            var operatorToken = tokenStream.Previous;
            var operandResult = ParseUnary(tokenStream); // Right-associative recursion
            if (operandResult.IsFailure)
            {
                return operandResult;
            }

            return ParseResult<IExpression>.Success(
                new UnaryExpression(operatorToken.Type, operandResult.Value,
                                   operatorToken.LineNumber, operatorToken.ColumnPosition));
        }

        return ParsePrimary(tokenStream);
    }

    /// <summary>
    /// Parses primary expressions and handles chained access expressions following the grammar:
    /// Primary = Literal | Identifier | FunctionCall | "(" Expression ")" | ObjectLiteral | ArrayLiteral
    /// Also handles access expressions (property access, indexing, function calls) as postfix operations.
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the primary expression or error information.</returns>
    private ParseResult<IExpression> ParsePrimary(TokenStream tokenStream)
    {
        if (tokenStream.IsAtEnd)
        {
            return ParseResult<IExpression>.Failure(
                MessageCode.UnexpectedToken,
                tokenStream.Current,
                "Unexpected end of input while parsing expression");
        }

        var primaryExpressionResult = tokenStream.Current.Type switch
        {
            JyroTokenType.NumberLiteral => ParseNumberLiteral(tokenStream),
            JyroTokenType.StringLiteral => ParseStringLiteral(tokenStream),
            JyroTokenType.BooleanLiteral => ParseBooleanLiteral(tokenStream),
            JyroTokenType.NullLiteral => ParseNullLiteral(tokenStream),
            JyroTokenType.Identifier => ParseIdentifier(tokenStream),
            JyroTokenType.LeftParenthesis => ParseGroupedExpression(tokenStream),
            JyroTokenType.LeftBrace => ParseObjectLiteral(tokenStream),
            JyroTokenType.LeftBracket => ParseArrayLiteral(tokenStream),
            _ => ParseResult<IExpression>.Failure(
                MessageCode.UnexpectedToken,
                tokenStream.Current,
                $"Unexpected token {tokenStream.Current.Type} in expression context")
        };

        if (primaryExpressionResult.IsFailure)
        {
            return primaryExpressionResult;
        }

        // Handle chained access expressions (property, index, function call)
        return ParseAccessExpressions(tokenStream, primaryExpressionResult.Value);
    }

    /// <summary>
    /// Handles chained access expressions such as property access, indexing, and function calls.
    /// Supports patterns like: obj.property[index]() or array[0].method(args).field
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <param name="baseExpression">The base expression to apply access operations to.</param>
    /// <returns>A parse result containing the fully chained access expression or error information.</returns>
    private ParseResult<IExpression> ParseAccessExpressions(TokenStream tokenStream, IExpression baseExpression)
    {
        var currentExpression = baseExpression;

        while (true)
        {
            if (tokenStream.Match(JyroTokenType.Dot))
            {
                var propertyNameResult = Require(tokenStream, JyroTokenType.Identifier, "property name");
                if (propertyNameResult.IsFailure)
                {
                    return ParseResult<IExpression>.Failure(propertyNameResult.Error);
                }

                currentExpression = new PropertyAccessExpression(currentExpression, propertyNameResult.Value.Lexeme,
                                                     currentExpression.LineNumber, currentExpression.ColumnPosition);
            }
            else if (tokenStream.Match(JyroTokenType.LeftBracket))
            {
                var indexExpressionResult = ParseExpression(tokenStream);
                if (indexExpressionResult.IsFailure)
                {
                    return indexExpressionResult;
                }

                var closeBracketResult = Require(tokenStream, JyroTokenType.RightBracket, "]");
                if (closeBracketResult.IsFailure)
                {
                    return ParseResult<IExpression>.Failure(closeBracketResult.Error);
                }

                currentExpression = new IndexAccessExpression(currentExpression, indexExpressionResult.Value,
                                                   currentExpression.LineNumber, currentExpression.ColumnPosition);
            }
            else if (tokenStream.Match(JyroTokenType.LeftParenthesis))
            {
                var argumentListResult = ParseArgumentList(tokenStream);
                if (argumentListResult.IsFailure)
                {
                    return ParseResult<IExpression>.Failure(argumentListResult.Error);
                }

                var closeParenthesisResult = Require(tokenStream, JyroTokenType.RightParenthesis, ")");
                if (closeParenthesisResult.IsFailure)
                {
                    return ParseResult<IExpression>.Failure(closeParenthesisResult.Error);
                }

                currentExpression = new FunctionCallExpression(currentExpression, argumentListResult.Value,
                                                    currentExpression.LineNumber, currentExpression.ColumnPosition);
            }
            else
            {
                break; // No more access operations
            }
        }

        return ParseResult<IExpression>.Success(currentExpression);
    }

    /// <summary>
    /// Parses numeric literals with proper culture handling for consistent parsing across locales.
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the numeric literal expression or error information.</returns>
    private static ParseResult<IExpression> ParseNumberLiteral(TokenStream tokenStream)
    {
        var numberToken = tokenStream.Advance();

        if (double.TryParse(numberToken.Lexeme, NumberStyles.Float, CultureInfo.InvariantCulture, out var numericValue))
        {
            return ParseResult<IExpression>.Success(
                new LiteralExpression(numericValue, numberToken.LineNumber, numberToken.ColumnPosition));
        }

        return ParseResult<IExpression>.Failure(
            MessageCode.InvalidNumberFormat,
            numberToken,
            $"Invalid number format: {numberToken.Lexeme}");
    }

    /// <summary>
    /// Parses string literals, using the lexeme directly as the string value.
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the string literal expression.</returns>
    private static ParseResult<IExpression> ParseStringLiteral(TokenStream tokenStream)
    {
        var stringToken = tokenStream.Advance();
        return ParseResult<IExpression>.Success(
            new LiteralExpression(stringToken.Lexeme, stringToken.LineNumber, stringToken.ColumnPosition));
    }

    /// <summary>
    /// Parses boolean literals, converting "true" and "false" lexemes to boolean values.
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the boolean literal expression.</returns>
    private static ParseResult<IExpression> ParseBooleanLiteral(TokenStream tokenStream)
    {
        var booleanToken = tokenStream.Advance();
        var booleanValue = string.Equals(booleanToken.Lexeme, "true", StringComparison.OrdinalIgnoreCase);
        return ParseResult<IExpression>.Success(
            new LiteralExpression(booleanValue, booleanToken.LineNumber, booleanToken.ColumnPosition));
    }

    /// <summary>
    /// Parses null literals, creating literal expressions with null values.
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the null literal expression.</returns>
    private static ParseResult<IExpression> ParseNullLiteral(TokenStream tokenStream)
    {
        var nullToken = tokenStream.Advance();
        return ParseResult<IExpression>.Success(
            new LiteralExpression(null, nullToken.LineNumber, nullToken.ColumnPosition));
    }

    /// <summary>
    /// Parses identifier tokens as variable reference expressions.
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the variable expression.</returns>
    private static ParseResult<IExpression> ParseIdentifier(TokenStream tokenStream)
    {
        var identifierToken = tokenStream.Advance();
        return ParseResult<IExpression>.Success(
            new VariableExpression(identifierToken.Lexeme, identifierToken.LineNumber, identifierToken.ColumnPosition));
    }

    /// <summary>
    /// Parses grouped expressions with parentheses following the grammar:
    /// "(" Expression ")"
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the grouped expression or error information.</returns>
    private ParseResult<IExpression> ParseGroupedExpression(TokenStream tokenStream)
    {
        tokenStream.Advance(); // consume "("

        var innerExpressionResult = ParseExpression(tokenStream);
        if (innerExpressionResult.IsFailure)
        {
            return innerExpressionResult;
        }

        var closeParenthesisResult = Require(tokenStream, JyroTokenType.RightParenthesis, ")");
        if (closeParenthesisResult.IsFailure)
        {
            return ParseResult<IExpression>.Failure(closeParenthesisResult.Error);
        }

        return ParseResult<IExpression>.Success(innerExpressionResult.Value);
    }

    /// <summary>
    /// Parses object literal expressions following the grammar:
    /// "{" [ ObjectEntry { "," ObjectEntry } ] "}"
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the object literal expression or error information.</returns>
    private ParseResult<IExpression> ParseObjectLiteral(TokenStream tokenStream)
    {
        var openBraceToken = tokenStream.Advance(); // consume "{"

        var propertyCollection = new List<ObjectProperty>();

        if (!tokenStream.Check(JyroTokenType.RightBrace))
        {
            do
            {
                var propertyResult = ParseObjectProperty(tokenStream);
                if (propertyResult.IsFailure)
                {
                    return ParseResult<IExpression>.Failure(propertyResult.Error);
                }
                propertyCollection.Add(propertyResult.Value);
            }
            while (tokenStream.Match(JyroTokenType.Comma));
        }

        var closeBraceResult = Require(tokenStream, JyroTokenType.RightBrace, "}");
        if (closeBraceResult.IsFailure)
        {
            return ParseResult<IExpression>.Failure(closeBraceResult.Error);
        }

        return ParseResult<IExpression>.Success(
            new ObjectLiteralExpression(propertyCollection, openBraceToken.LineNumber, openBraceToken.ColumnPosition));
    }

    /// <summary>
    /// Parses array literal expressions following the grammar:
    /// "[" [ Expression { "," Expression } ] "]"
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the array literal expression or error information.</returns>
    private ParseResult<IExpression> ParseArrayLiteral(TokenStream tokenStream)
    {
        var openBracketToken = tokenStream.Advance(); // consume "["

        var elementCollection = new List<IExpression>();

        if (!tokenStream.Check(JyroTokenType.RightBracket))
        {
            do
            {
                var elementResult = ParseExpression(tokenStream);
                if (elementResult.IsFailure)
                {
                    return elementResult;
                }
                elementCollection.Add(elementResult.Value);
            }
            while (tokenStream.Match(JyroTokenType.Comma));
        }

        var closeBracketResult = Require(tokenStream, JyroTokenType.RightBracket, "]");
        if (closeBracketResult.IsFailure)
        {
            return ParseResult<IExpression>.Failure(closeBracketResult.Error);
        }

        return ParseResult<IExpression>.Success(
            new ArrayLiteralExpression(elementCollection, openBracketToken.LineNumber, openBracketToken.ColumnPosition));
    }

    /// <summary>
    /// Parses object properties following the grammar:
    /// (StringLiteral | InterpolatedKey) ":" Expression
    /// Supports both string literal keys and computed keys in brackets.
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the object property or error information.</returns>
    private ParseResult<ObjectProperty> ParseObjectProperty(TokenStream tokenStream)
    {
        string propertyKey;
        var keyStartToken = tokenStream.Current;

        if (tokenStream.Check(JyroTokenType.StringLiteral))
        {
            propertyKey = tokenStream.Advance().Lexeme;
        }
        else if (tokenStream.Match(JyroTokenType.LeftBracket))
        {
            // Interpolated key: [Expression]
            var keyExpressionResult = ParseExpression(tokenStream);
            if (keyExpressionResult.IsFailure)
            {
                return ParseResult<ObjectProperty>.Failure(keyExpressionResult.Error);
            }

            var closeBracketResult = Require(tokenStream, JyroTokenType.RightBracket, "]");
            if (closeBracketResult.IsFailure)
            {
                return ParseResult<ObjectProperty>.Failure(closeBracketResult.Error);
            }

            // For interpolated keys, use a special marker and store the expression
            // This simplified approach would need more sophisticated handling in a full implementation
            propertyKey = $"[{keyExpressionResult.Value}]";
        }
        else
        {
            return ParseResult<ObjectProperty>.Failure(
                MessageCode.UnexpectedToken,
                tokenStream.Current,
                "Expected string literal or [expression] for object property key");
        }

        var colonResult = Require(tokenStream, JyroTokenType.Colon, ":");
        if (colonResult.IsFailure)
        {
            return ParseResult<ObjectProperty>.Failure(colonResult.Error);
        }

        var valueExpressionResult = ParseExpression(tokenStream);
        if (valueExpressionResult.IsFailure)
        {
            return ParseResult<ObjectProperty>.Failure(valueExpressionResult.Error);
        }

        return ParseResult<ObjectProperty>.Success(
            new ObjectProperty(propertyKey, valueExpressionResult.Value, keyStartToken.LineNumber, keyStartToken.ColumnPosition));
    }

    /// <summary>
    /// Parses function argument lists following the grammar:
    /// [ Expression { "," Expression } ]
    /// </summary>
    /// <param name="tokenStream">The token stream to parse from.</param>
    /// <returns>A parse result containing the argument list or error information.</returns>
    private ParseResult<IReadOnlyList<IExpression>> ParseArgumentList(TokenStream tokenStream)
    {
        var argumentCollection = new List<IExpression>();

        if (!tokenStream.Check(JyroTokenType.RightParenthesis))
        {
            do
            {
                var argumentResult = ParseExpression(tokenStream);
                if (argumentResult.IsFailure)
                {
                    return ParseResult<IReadOnlyList<IExpression>>.Failure(argumentResult.Error);
                }
                argumentCollection.Add(argumentResult.Value);
            }
            while (tokenStream.Match(JyroTokenType.Comma));
        }

        return ParseResult<IReadOnlyList<IExpression>>.Success(argumentCollection);
    }
}