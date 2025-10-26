using OLangCompiler.Parser.ParseTree.Term;

namespace OLangCompiler.Parser.ParseTree.UnaryExpression;

public class NonUnaryExpression(ITerm term) : IUnaryExpression
{
    public ITerm Term = term;
}