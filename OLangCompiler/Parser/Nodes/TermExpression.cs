namespace OLangCompiler.Parser.Nodes.NodeValues;

public class TermExpression(ITermNode term) : IExpressionNode
{
    public ITermNode Term = term;
}