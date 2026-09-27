using Lexing;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Term;

public class FieldAccess(ITerm term, IdentifierToken identifier) : ITerm
{
    public ITerm Term = term;
    public IdentifierToken Identifier = identifier;
    public SourceSpan Span { get; set; }
}