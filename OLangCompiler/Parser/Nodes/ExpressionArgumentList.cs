namespace OLangCompiler.Parser.Nodes;

public class ExpressionArgumentList(BaseExpressionNode expression) : IArgumentListNode
{
    public BaseExpressionNode Expression = expression;
}