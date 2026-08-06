using Lexing;
using OLangGrammar.ParseTree.Term;

namespace OLangGrammar.ParseTree.UnaryExpression;

public class Negation(ITerm term) : IUnaryExpression
{
    public ITerm Term = term;
    public SourceSpan Span { get; set; }
}