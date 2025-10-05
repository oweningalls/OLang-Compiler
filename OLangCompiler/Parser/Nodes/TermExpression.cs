namespace OLangCompiler.Parser.Nodes;

public class TermExpression(ITermNode term) : IExpressionNode
{
    public ITermNode Term = term;
}