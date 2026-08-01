using OLangGrammar.ParseTree.UnaryExpression;

namespace OLangGrammar.ParseTree.MultExpression;

public class Div(IMultExpression lhs, IUnaryExpression rhs) : IMultExpression
{
    public IMultExpression Lhs = lhs;
    public IUnaryExpression Rhs = rhs;
}
