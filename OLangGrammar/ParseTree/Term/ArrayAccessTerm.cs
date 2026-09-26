using Lexing;
using OLangGrammar.ParseTree.Expression;

namespace OLangGrammar.ParseTree.Term;

public class ArrayAccessTerm(ITerm arrayExpression, IExpression index) : ITerm
{
    public ITerm ArrayExpression = arrayExpression;
    public IExpression Index = index;
    public SourceSpan Span { get; set; }
}