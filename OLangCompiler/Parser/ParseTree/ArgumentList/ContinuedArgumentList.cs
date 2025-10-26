using OLangCompiler.Parser.ParseTree.Expression;

namespace OLangCompiler.Parser.ParseTree.ArgumentList;

public class ContinuedArgumentList(IExpression expression, IArgumentList argumentList) : ExpressionArgumentList(expression)
{
    public IArgumentList ArgumentList = argumentList;
}