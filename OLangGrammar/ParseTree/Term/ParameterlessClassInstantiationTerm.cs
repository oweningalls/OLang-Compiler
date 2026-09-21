using Lexing;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Term;

public class ParameterlessClassInstantiationTerm(IdentifierToken identifier) : ITerm
{
    public IdentifierToken Identifier = identifier;
    public SourceSpan Span { get; set; }
}