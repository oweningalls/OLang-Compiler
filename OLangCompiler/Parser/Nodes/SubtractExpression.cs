namespace OLangCompiler.Parser.Nodes;

public class SubtractExpression(ITermNode lhs, IExpressionNode rhs) : IExpressionNode
{
    public ITermNode Lhs = lhs;
    public IExpressionNode Rhs = rhs;
}