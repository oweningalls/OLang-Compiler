using OLangGrammar.ParseTree.Expression;

namespace OLangGrammar.ParseTree.Term;

public class Paren(IExpression expression) : ITerm
{
    public IExpression Expression = expression;
}