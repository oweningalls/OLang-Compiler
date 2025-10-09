namespace OLangCompiler.Parser.Nodes;

public class NotExpression(IExpressionNode expression) : IExpressionNode
{
    public IExpressionNode Expression = expression;
}