using Lexing;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Term;

public class ClassInstantiationTerm(IdentifierToken identifier) : ITerm
{
    public IdentifierToken Identifier = identifier;
    public SourceSpan Span { get; set; }
}