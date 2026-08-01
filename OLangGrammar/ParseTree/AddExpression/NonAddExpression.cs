using OLangGrammar.ParseTree.MultExpression;

namespace OLangGrammar.ParseTree.AddExpression;

public class NonAddExpression(IMultExpression expression) : IAddExpression
{
    public IMultExpression Expression = expression;
}
