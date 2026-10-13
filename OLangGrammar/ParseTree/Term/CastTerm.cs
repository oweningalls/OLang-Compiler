using Lexing;
using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.Type;

namespace OLangGrammar.ParseTree.Term;

public class CastTerm(IType type, ITerm term) : ITerm
{
    public IType Type = type;
    public ITerm Term = term;
    public SourceSpan Span { get; set; }
}