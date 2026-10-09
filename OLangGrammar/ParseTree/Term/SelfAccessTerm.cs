using Lexing;

namespace OLangGrammar.ParseTree.Term;

public class SelfAccessTerm : ITerm
{
    public SourceSpan Span { get; set; }
}