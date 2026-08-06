using Lexing;
using OLangGrammar.ParseTree.UnaryExpression;

namespace OLangGrammar.ParseTree.MultExpression;

public class Mult(IMultExpression lhs, IUnaryExpression rhs) : IMultExpression
{
    public IMultExpression Lhs = lhs;
    public IUnaryExpression Rhs = rhs;
    public SourceSpan Span { get; set; }
}
