using Lexing;
using OLangGrammar.ParseTree.EqualityExpression;

namespace OLangGrammar.ParseTree.AndExpression;

public class NonAnd(IEqualityExpression expression) : IAndExpression
{
    public IEqualityExpression Expression = expression;
    public SourceSpan Span { get; set; }
}