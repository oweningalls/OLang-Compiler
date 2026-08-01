using OLangGrammar.ParseTree.Term;

namespace OLangGrammar.ParseTree.UnaryExpression;

public class Not(ITerm term) : IUnaryExpression
{
    public ITerm Term = term;
}