namespace OLangCompiler.Parser.Nodes;

public class ContinuedArgumentList(BaseExpressionNode expressionNode, IArgumentListNode argumentList) : ExpressionArgumentList(expressionNode)
{
    public IArgumentListNode ArgumentList = argumentList;
}