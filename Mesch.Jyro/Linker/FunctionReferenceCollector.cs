using Antlr4.CodeGenerator;

namespace Mesch.Jyro;

internal class FunctionReferenceCollector : JyroBaseVisitor<object?>
{
    private readonly HashSet<FunctionReference> _functionReferences = new();

    public HashSet<FunctionReference> CollectReferences(JyroParser.ProgramContext program)
    {
        Visit(program);
        return _functionReferences;
    }

    public override object? VisitPostfixExpr(JyroParser.PostfixExprContext context)
    {
        var primary = context.primaryExpr();

        if (primary.Identifier() != null && context.postfixSuffix().Length > 0)
        {
            var firstSuffix = context.postfixSuffix(0);
            if (firstSuffix.LPAREN() != null)
            {
                var functionName = primary.Identifier().GetText();
                var argumentCount = 0;

                if (firstSuffix.argList() != null)
                {
                    argumentCount = firstSuffix.argList().expression().Length;
                }

                var placeholderArguments = Enumerable.Repeat((JyroValue)JyroNull.Instance, argumentCount).ToList();

                _functionReferences.Add(new FunctionReference(
                    functionName,
                    placeholderArguments,
                    context.Start.Line,
                    context.Start.Column));
            }
        }

        return base.VisitPostfixExpr(context);
    }
}