using Lexing;
using OLangGrammar.ParseTree.MultExpression;

namespace OLangGrammar.ParseTree.AddExpression;

public class Plus(IAddExpression lhs, IMultExpression rhs) : IAddExpression
{
    public IAddExpression Lhs = lhs;
    public IMultExpression Rhs = rhs;
    public SourceSpan Span { get; set; }
}
