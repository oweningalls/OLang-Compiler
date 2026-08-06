using Lexing;
using OLangGrammar.ParseTree.EqualityExpression;

namespace OLangGrammar.ParseTree.AndExpression;

public class AndExpression(IAndExpression lhs, IEqualityExpression rhs) : IAndExpression
{
    public SourceSpan Span { get; set; }
    public IAndExpression Lhs = lhs;
    public IEqualityExpression Rhs = rhs;
}
