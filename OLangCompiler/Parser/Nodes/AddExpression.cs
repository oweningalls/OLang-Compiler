namespace OLangCompiler.Parser.Nodes;

public class AddExpression(ITermNode lhs, IExpressionNode rhs) : IExpressionNode
{
    public ITermNode Lhs = lhs;
    public IExpressionNode Rhs = rhs;
}