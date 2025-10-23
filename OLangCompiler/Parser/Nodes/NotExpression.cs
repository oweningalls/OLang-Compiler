namespace OLangCompiler.Parser.Nodes;

public class NotExpression(BaseExpressionNode expression) : BaseExpressionNode
{
    public BaseExpressionNode Expression = expression;
}