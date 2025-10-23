namespace OLangCompiler.Parser.Nodes;

public abstract class BaseBinaryExpressionNode(BaseExpressionNode lhs, BaseExpressionNode rhs) : BaseExpressionNode
{
    public BaseExpressionNode Lhs = lhs;
    public BaseExpressionNode Rhs = rhs;
}