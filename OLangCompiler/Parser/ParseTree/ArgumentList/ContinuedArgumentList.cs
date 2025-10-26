using OLangCompiler.Parser.ParseTree.Expression;

namespace OLangCompiler.Parser.ParseTree.ArgumentList;

public class ContinuedArgumentList(BaseExpressionNode expressionNode, IArgumentListNode argumentList) : ExpressionArgumentList(expressionNode)
{
    public IArgumentListNode ArgumentList = argumentList;
}