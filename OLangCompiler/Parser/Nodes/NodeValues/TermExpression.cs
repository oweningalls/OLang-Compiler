namespace OLangCompiler.Parser.Nodes.NodeValues;

public class TermExpression(TermNode term) : IExpressionNode
{
    public TermNode Term = term;
}