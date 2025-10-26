using OLangCompiler.Parser.ParseTree.Expression;

namespace OLangCompiler.Parser.ParseTree.ArgumentList;

public class ExpressionArgumentList(BaseExpression expression) : IArgumentList
{
    public BaseExpression Expression = expression;
}