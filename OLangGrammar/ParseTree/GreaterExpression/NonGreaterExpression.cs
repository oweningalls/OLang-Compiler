using OLangGrammar.ParseTree.AddExpression;

namespace OLangGrammar.ParseTree.GreaterExpression;

public class NonGreaterExpression(IAddExpression expression) : IGreaterExpression
{
    public IAddExpression Expression = expression;
}