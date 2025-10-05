namespace OLangCompiler.Parser.Nodes;

public abstract class BaseBinaryExpressionNode(ITermNode lhs, IExpressionNode rhs) : IExpressionNode
{
    public ITermNode Lhs = lhs;
    public IExpressionNode Rhs = rhs;
}