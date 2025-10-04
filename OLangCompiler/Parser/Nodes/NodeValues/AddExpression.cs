namespace OLangCompiler.Parser.Nodes.NodeValues;

public class AddExpression(TermNode lhs, IExpressionNode rhs) : IExpressionNode
{
    public TermNode Lhs = lhs;
    public IExpressionNode Rhs = rhs;
}