using OLangGrammar.ParseTree.AndExpression;

namespace OLangGrammar.ParseTree.Expression;

public class NonExpression(IAndExpression expression) : IExpression
{
    public IAndExpression Expression = expression;
}