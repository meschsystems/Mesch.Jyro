using Antlr4.CodeGenerator;

namespace Mesch.Jyro;

public sealed class LinkedProgram
{
    public LinkedProgram(
        JyroParser.ProgramContext programContext,
        IReadOnlyDictionary<string, IJyroFunction> availableFunctions)
    {
        ProgramContext = programContext ?? throw new ArgumentNullException(nameof(programContext));
        Functions = availableFunctions ?? throw new ArgumentNullException(nameof(availableFunctions));
    }

    public JyroParser.ProgramContext ProgramContext { get; }
    public IReadOnlyDictionary<string, IJyroFunction> Functions { get; }
}