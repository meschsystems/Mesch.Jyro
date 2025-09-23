namespace Mesch.Jyro;

/// <summary>
/// Defines the core language specification for Jyro, including keyword mappings and structural contracts.
/// This specification ensures consistent language behavior across all platform implementations.
/// </summary>
public static class JyroLanguageSpecification
{
    /// <summary>
    /// Gets the immutable mapping of reserved keywords to their corresponding token types.
    /// Jyro implementations must recognize these exact keyword strings.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, JyroTokenType> Keywords = new Dictionary<string, JyroTokenType>
    {
        #region Variable declaration

        ["var"] = JyroTokenType.Var,

        #endregion

        #region Type keywords

        ["number"] = JyroTokenType.NumberType,
        ["string"] = JyroTokenType.StringType,
        ["boolean"] = JyroTokenType.BooleanType,
        ["object"] = JyroTokenType.ObjectType,
        ["array"] = JyroTokenType.ArrayType,

        #endregion

        #region Control flow keywords

        ["if"] = JyroTokenType.If,
        ["then"] = JyroTokenType.Then,
        ["else"] = JyroTokenType.Else,
        ["end"] = JyroTokenType.End,
        ["switch"] = JyroTokenType.Switch,
        ["case"] = JyroTokenType.Case,
        ["default"] = JyroTokenType.Default,
        ["return"] = JyroTokenType.Return,

        #endregion

        #region Loop keywords

        ["foreach"] = JyroTokenType.ForEach,
        ["in"] = JyroTokenType.In,
        ["do"] = JyroTokenType.Do,
        ["while"] = JyroTokenType.While,
        ["break"] = JyroTokenType.Break,
        ["continue"] = JyroTokenType.Continue,

        #endregion

        #region Logical operators

        ["and"] = JyroTokenType.And,
        ["or"] = JyroTokenType.Or,
        ["not"] = JyroTokenType.Not,

        #endregion

        #region  Type checking

        ["is"] = JyroTokenType.Is,

        #endregion

        #region  Literals

        ["true"] = JyroTokenType.BooleanLiteral,
        ["false"] = JyroTokenType.BooleanLiteral,
        ["null"] = JyroTokenType.NullLiteral,

        #endregion

    };
}