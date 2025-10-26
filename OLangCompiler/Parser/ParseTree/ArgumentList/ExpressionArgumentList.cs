using OLangCompiler.Parser.ParseTree.Expression;

namespace OLangCompiler.Parser.ParseTree.ArgumentList;

public class ExpressionArgumentList(BaseExpressionNode expression) : IArgumentListNode
{
    public BaseExpressionNode Expression = expression;
}