namespace OLangCompiler.Parser.ParseTree.Expression;

public class NotExpression(BaseExpressionNode expression) : BaseExpressionNode
{
    public BaseExpressionNode Expression = expression;
}