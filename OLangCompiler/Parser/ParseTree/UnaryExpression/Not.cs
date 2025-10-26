using OLangCompiler.Parser.ParseTree.Term;

namespace OLangCompiler.Parser.ParseTree.UnaryExpression;

public class Not(ITerm term) : IUnaryExpression
{
    public ITerm Term = term;
}