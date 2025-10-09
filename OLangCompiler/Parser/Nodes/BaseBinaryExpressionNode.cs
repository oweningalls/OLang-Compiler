namespace OLangCompiler.Parser.Nodes;

public abstract class BaseBinaryExpressionNode(IExpressionNode lhs, IExpressionNode rhs) : IExpressionNode
{
    public IExpressionNode Lhs = lhs;
    public IExpressionNode Rhs = rhs;
}