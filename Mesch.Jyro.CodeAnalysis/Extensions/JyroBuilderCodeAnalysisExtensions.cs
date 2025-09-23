namespace Mesch.Jyro.CodeAnalysis;

/// <summary>
/// Extension methods to add code analysis capabilities to JyroBuilder.
/// These extensions depend on the Runtime library but do not create circular dependencies.
/// </summary>
public static class JyroBuilderCodeAnalysisExtensions
{
    /// <summary>
    /// Adds code analysis to the builder pipeline with default options.
    /// Analysis is performed on the parsed AST before execution.
    /// </summary>
    public static CodeAnalysisBuilder WithCodeAnalysis(this JyroBuilder builder)
    {
        return new CodeAnalysisBuilder(builder, new CodeAnalysisOptions());
    }

    /// <summary>
    /// Adds code analysis to the builder pipeline with specified options.
    /// Analysis is performed on the parsed AST before execution.
    /// </summary>
    public static CodeAnalysisBuilder WithCodeAnalysis(this JyroBuilder builder, CodeAnalysisOptions options)
    {
        return new CodeAnalysisBuilder(builder, options);
    }
}
