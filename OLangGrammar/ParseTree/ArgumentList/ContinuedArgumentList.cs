using OLangGrammar.ParseTree.Expression;

namespace OLangGrammar.ParseTree.ArgumentList;

public class ContinuedArgumentList(IExpression expression, IArgumentList argumentList) : ExpressionArgumentList(expression)
{
    public IArgumentList ArgumentList = argumentList;
}