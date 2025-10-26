using OLangCompiler.Parser.ParseTree.Expression;

namespace OLangCompiler.Parser.ParseTree.ArgumentList;

public class ExpressionArgumentList(IExpression expression) : IArgumentList
{
    public IExpression Expression = expression;
}