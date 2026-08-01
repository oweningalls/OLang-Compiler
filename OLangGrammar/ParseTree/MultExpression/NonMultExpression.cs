using OLangGrammar.ParseTree.UnaryExpression;

namespace OLangGrammar.ParseTree.MultExpression;

public class NonMultExpression(IUnaryExpression expression) : IMultExpression
{
    public IUnaryExpression Expression = expression;
}